using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using InteractiveLeads.Application.Feature.Tenancy;
using InteractiveLeads.Infrastructure.Context.Application;
using InteractiveLeads.Infrastructure.Tenancy.Models;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace InteractiveLeads.Infrastructure.Tenancy
{
    public class TenantService : ITenantService
    {
        private readonly IMultiTenantStore<InteractiveTenantInfo> _tenantStore;
        private readonly ApplicationDbSeeder _dbSeeder;
        private readonly IServiceProvider _serviceProvider;

        public TenantService(IMultiTenantStore<InteractiveTenantInfo> tenantStore, ApplicationDbSeeder dbSeeder, IServiceProvider serviceProvider)
        {
            _dbSeeder = dbSeeder;
            _tenantStore = tenantStore;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> ActivateAsync(string id)
        {
            var tenantInDb = await _tenantStore.TryGetAsync(id);
            tenantInDb.IsActive = true;

            await _tenantStore.TryUpdateAsync(tenantInDb);

            return tenantInDb.Identifier;
        }

        public async Task<string> CreateTenantAsync(CreateTenantRequest createTenantRequest, CancellationToken ct)
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

            return newTenant.Identifier;
        }

        public async Task<string> DeactivateAsync(string id)
        {
            var tenantInDb = await _tenantStore.TryGetAsync(id);
            tenantInDb.IsActive = false;

            await _tenantStore.TryUpdateAsync(tenantInDb);

            return tenantInDb.Identifier;
        }

        public async Task<List<TenantResponse>> GetTenantsAsync()
        {
            var tenantInDb = await _tenantStore.GetAllAsync();

            return tenantInDb.Adapt<List<TenantResponse>>();
        }

        public async Task<TenantResponse> GetTenantsByIdAsync(string id)
        {
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
            return tenantInDb.Adapt<TenantResponse>();
        }

        public async Task<string> UpdateSubscriptionAsync(UpdateTenantSubscriptionRequest updateTenantSubscriptionRequest)
        {
            var tenantInDb = await _tenantStore.TryGetAsync(updateTenantSubscriptionRequest.TenantId);

            tenantInDb.ExpirationDate = updateTenantSubscriptionRequest.NewExpirationDate;

            await _tenantStore.TryUpdateAsync(tenantInDb);

            return tenantInDb.Identifier;
        }
    }
}
