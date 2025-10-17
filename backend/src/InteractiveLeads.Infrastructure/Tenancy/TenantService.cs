using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using InteractiveLeads.Application.Feature.Tenancy;
using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Models;
using InteractiveLeads.Application.Responses;
using InteractiveLeads.Infrastructure.Constants;
using InteractiveLeads.Infrastructure.Context.Application;
using InteractiveLeads.Infrastructure.Context.Tenancy;
using InteractiveLeads.Infrastructure.Tenancy.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InteractiveLeads.Infrastructure.Tenancy
{
    public class TenantService : ITenantService
    {
        private readonly IMultiTenantStore<InteractiveTenantInfo> _tenantStore;
        private readonly TenantDbContext _tenantDbContext;
        private readonly ApplicationDbSeeder _dbSeeder;
        private readonly IServiceProvider _serviceProvider;

        public TenantService(
            IMultiTenantStore<InteractiveTenantInfo> tenantStore, 
            TenantDbContext tenantDbContext,
            ApplicationDbSeeder dbSeeder, 
            IServiceProvider serviceProvider)
        {
            _dbSeeder = dbSeeder;
            _tenantStore = tenantStore;
            _tenantDbContext = tenantDbContext;
            _serviceProvider = serviceProvider;
        }

        public async Task<ResultResponse> ActivateAsync(string id, CancellationToken ct = default)
        {
            // Block operations on root tenant
            if (id == TenancyConstants.Root.Id)
            {
                var errorResponse = new ResultResponse();
                errorResponse.AddErrorMessage("Cannot modify root tenant", "tenant.root_modification_denied");
                return errorResponse;
            }

            var tenantInDb = await _tenantStore.TryGetAsync(id);
            tenantInDb.IsActive = true;

            await _tenantStore.TryUpdateAsync(tenantInDb);

            var response = new ResultResponse();
            response.AddSuccessMessage("Tenant activated successfully", "tenant.activated_successfully");
            return response;
        }

        public async Task<ResultResponse> CreateTenantAsync(CreateTenantRequest createTenantRequest, CancellationToken ct)
        {
            var newTenant = new InteractiveTenantInfo
            {
                Id = createTenantRequest.Identifier,
                Name = createTenantRequest.Name,
                Identifier = createTenantRequest.Identifier,
                IsActive = createTenantRequest.IsActive,
                ConnectionString = createTenantRequest.ConnectionString,
                Email = createTenantRequest.Email,
                FirstName = createTenantRequest.FirstName,
                LastName = createTenantRequest.LastName,
                ExpirationDate = createTenantRequest.ExpirationDate
            };

            bool isSuccess = await _tenantStore.TryAddAsync(newTenant);
            //if (!isSuccess) throw custom exception

            // Seeding tenant data
            await using var scope = _serviceProvider.CreateAsyncScope();

            _serviceProvider.GetRequiredService<IMultiTenantContextSetter>()
                .MultiTenantContext = new MultiTenantContext<InteractiveTenantInfo>()
                {
                    TenantInfo = newTenant
                };

            
            await scope.ServiceProvider.GetRequiredService<ApplicationDbSeeder>()
                .InitializeDatabaseAsync(ct);

            var response = new ResultResponse();
            response.AddSuccessMessage("Tenant created successfully", "tenant.created_successfully");
            return response;
        }

        public async Task<ResultResponse> DeactivateAsync(string id, CancellationToken ct = default)
        {
            // Block operations on root tenant
            if (id == TenancyConstants.Root.Id)
            {
                var errorResponse = new ResultResponse();
                errorResponse.AddErrorMessage("Cannot modify root tenant", "tenant.root_modification_denied");
                return errorResponse;
            }

            var tenantInDb = await _tenantStore.TryGetAsync(id);
            tenantInDb.IsActive = false;

            await _tenantStore.TryUpdateAsync(tenantInDb);

            var response = new ResultResponse();
            response.AddSuccessMessage("Tenant deactivated successfully", "tenant.deactivated_successfully");
            return response;
        }

        public async Task<ListResponse<TenantResponse>> GetTenantsAsync(PaginationRequest pagination, CancellationToken ct)
        {
            // Validate pagination parameters
            if (!pagination.IsValid())
            {
                pagination = new PaginationRequest(); // Use default values if invalid
            }

            // Get total count efficiently from database, excluding root tenant
            var totalTenants = await _tenantDbContext.TenantInfo
                .Where(t => t.Identifier != TenancyConstants.Root.Id)
                .CountAsync(ct);

            // Get all tenants and filter out root tenant, then apply pagination manually
            var allTenants = await _tenantStore.GetAllAsync();
            var filteredTenants = allTenants
                .Where(t => t.Identifier != TenancyConstants.Root.Id)
                .Skip(pagination.CalculateSkip())
                .Take(pagination.PageSize)
                .ToList();
            
            var tenants = filteredTenants.Adapt<List<TenantResponse>>();

            var response = new ListResponse<TenantResponse>(tenants, totalTenants);
            response.AddSuccessMessage("Tenants retrieved successfully", "tenants.retrieved_successfully");
            return response;
        }

        public async Task<SingleResponse<TenantResponse>> GetTenantsByIdAsync(string id, CancellationToken ct)
        {
            // Block access to root tenant
            if (id == TenancyConstants.Root.Id)
            {
                var errorResponse = new SingleResponse<TenantResponse>(null);
                errorResponse.AddErrorMessage("Access to root tenant is not allowed", "tenant.root_access_denied");
                return errorResponse;
            }

            var tenantInDb = await _tenantStore.TryGetAsync(id);

            var newTenant = new InteractiveTenantInfo
            {
                Id = tenantInDb.Identifier,
                Identifier = tenantInDb.Identifier,
                IsActive = tenantInDb.IsActive,
                ConnectionString = tenantInDb.ConnectionString,
                Email = tenantInDb.Email,
                FirstName = tenantInDb.FirstName,
                LastName = tenantInDb.LastName,
                ExpirationDate = tenantInDb.ExpirationDate
            };

            // Mapster
            var tenantResponse = tenantInDb.Adapt<TenantResponse>();
            
            var response = new SingleResponse<TenantResponse>(tenantResponse);
            response.AddSuccessMessage("Tenant retrieved successfully", "tenant.retrieved_successfully");
            return response;
        }

        public async Task<ResultResponse> UpdateSubscriptionAsync(UpdateTenantSubscriptionRequest updateTenantSubscriptionRequest, CancellationToken ct = default)
        {
            var tenantInDb = await _tenantStore.TryGetAsync(updateTenantSubscriptionRequest.TenantId);

            tenantInDb.ExpirationDate = updateTenantSubscriptionRequest.NewExpirationDate;

            await _tenantStore.TryUpdateAsync(tenantInDb);

            var response = new ResultResponse();
            response.AddSuccessMessage("Tenant subscription updated successfully", "tenant.subscription_updated_successfully");
            return response;
        }
    }
}
