using Finbuckle.MultiTenant.Abstractions;
using InteractiveLeads.Application.Exceptions;
using InteractiveLeads.Application.Feature.Identity.Roles;
using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Responses;
using InteractiveLeads.Infrastructure.Constants;
using InteractiveLeads.Infrastructure.Context.Application;
using InteractiveLeads.Infrastructure.Identity.Models;
using InteractiveLeads.Infrastructure.Tenancy.Models;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InteractiveLeads.Infrastructure.Identity.Roles
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IMultiTenantContextAccessor<InteractiveTenantInfo> _tenantInfoContextAccessor;

        public RoleService(
            RoleManager<ApplicationRole> roleManager, 
            UserManager<ApplicationUser> userManager, 
            ApplicationDbContext context, 
            IMultiTenantContextAccessor<InteractiveTenantInfo> tenantInfoContextAccessor)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
            _tenantInfoContextAccessor = tenantInfoContextAccessor;
        }

        public async Task<ResultResponse> CreateAsync(CreateRoleRequest request, CancellationToken ct = default)
        {
            var newRole = new ApplicationRole()
            {
                Name = request.Name,
                Description = request.Description
            };

            var result = await _roleManager.CreateAsync(newRole);

            if (!result.Succeeded)
            {
                var identityResponse = new ResultResponse();
                foreach (var error in IdentityHelper.GetIdentityResultErrorDescriptions(result))
                {
                    identityResponse.AddErrorMessage(error, "identity.operation_failed");
                }
                throw new IdentityException(identityResponse);
            }

            var response = new ResultResponse();
            response.AddSuccessMessage("Role created successfully", "role.created_successfully");
            return response;
        }

        public async Task<ResultResponse> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var roleInDb = await _roleManager.FindByIdAsync(id.ToString());
            if (roleInDb is null)
            {
                var notFoundResponse = new ResultResponse();
                notFoundResponse.AddErrorMessage("Role does not exist.", "role.not_found");
                
                throw new NotFoundException(notFoundResponse);
            }

            if (RoleConstants.IsDefaultRole(roleInDb.Name!))
            {
                var conflictResponse = new ResultResponse();
                conflictResponse.AddErrorMessage($"Not allowed to delete '{roleInDb.Name}' role.", "role.delete_default_forbidden");

                throw new ConflictException(conflictResponse);
            }

            if ((await _userManager.GetUsersInRoleAsync(roleInDb.Name!)).Count > 0)
            {
                var conflictResponse = new ResultResponse();
                conflictResponse.AddErrorMessage($"Not allowed to delete '{roleInDb.Name}' role as is currently assigned to users.", "role.delete_assigned_forbidden");
                
                throw new ConflictException(conflictResponse);
            }

            var result = await _roleManager.DeleteAsync(roleInDb);
            if (!result.Succeeded)
            {
                var identityResponse = new ResultResponse();
                foreach (var error in IdentityHelper.GetIdentityResultErrorDescriptions(result))
                {
                    identityResponse.AddErrorMessage(error, "identity.operation_failed");
                }
                throw new IdentityException(identityResponse);
            }

            var response = new ResultResponse();
            response.AddSuccessMessage("Role deleted successfully", "role.deleted_successfully");
            return response;
        }

        public async Task<bool> DoesItExistsAsync(string name, CancellationToken ct = default)
        {
            return await _roleManager.RoleExistsAsync(name);
        }

        public async Task<ListResponse<RoleResponse>> GetAllAsync(CancellationToken ct)
        {
            var rolesInDb = await _roleManager
                .Roles
                .ToListAsync(ct);

            var roles = rolesInDb.Adapt<List<RoleResponse>>();

            var response = new ListResponse<RoleResponse>(roles, roles.Count);
            response.AddSuccessMessage("Roles retrieved successfully", "roles.retrieved_successfully");
            return response;
        }

        public async Task<SingleResponse<RoleResponse>> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var roleInDb = await _context.Roles
                .FirstOrDefaultAsync(role => role.Id == id, ct);
            
            if (roleInDb == null)
            {
                var notFoundResponse = new ResultResponse();
                notFoundResponse.AddErrorMessage("Role does not exist.", "role.not_found");

                throw new NotFoundException(notFoundResponse);
            }

            var roleResponse = roleInDb.Adapt<RoleResponse>();
            
            var response = new SingleResponse<RoleResponse>(roleResponse);
            response.AddSuccessMessage("Role retrieved successfully", "role.retrieved_successfully");
            return response;
        }

        public async Task<SingleResponse<RoleResponse>> GetRoleWithPermissionsAsync(Guid id, CancellationToken ct)
        {
            var roleResult = await GetByIdAsync(id, ct);
            var role = roleResult.Data!;

            role.Permissions = await _context.RoleClaims
                .Where(rc => rc.RoleId == id && rc.ClaimType == ClaimConstants.Permission && rc.ClaimValue != null)
                .Select(rc => rc.ClaimValue!)
                .ToListAsync(ct);

            var response = new SingleResponse<RoleResponse>(role);
            response.AddSuccessMessage("Role with permissions retrieved successfully", "role.retrieved_with_permissions_successfully");
            return response;
        }

        public async Task<ResultResponse> UpdateAsync(UpdateRoleRequest request, CancellationToken ct = default)
        {
            var roleInDb = await _roleManager.FindByIdAsync(request.Id);
            if (roleInDb == null)
            {
                var notFoundResponse = new ResultResponse();
                notFoundResponse.AddErrorMessage("Role does not exist.", "role.not_found");
                
                throw new NotFoundException(notFoundResponse);
            }

            if (RoleConstants.IsDefaultRole(roleInDb.Name!))
            {
                var conflictResponse = new ResultResponse();
                conflictResponse.AddErrorMessage($"Changes not allowed on system role '{roleInDb.Name}'.", "role.update_default_forbidden");
                throw new ConflictException(conflictResponse);
            }

            roleInDb.Name = request.Name;
            roleInDb.Description = request.Description;
            roleInDb.NormalizedName = request.Name.ToUpperInvariant();

            var result = await _roleManager.UpdateAsync(roleInDb);

            if (!result.Succeeded)
            {
                var identityResponse = new ResultResponse();
                foreach (var error in IdentityHelper.GetIdentityResultErrorDescriptions(result))
                {
                    identityResponse.AddErrorMessage(error, "identity.operation_failed");
                }
                throw new IdentityException(identityResponse);
            }
            
            var response = new ResultResponse();
            response.AddSuccessMessage("Role updated successfully", "role.updated_successfully");
            return response;
        }

        public async Task<ResultResponse> UpdatePermissionsAsync(UpdateRolePermissionsRequest request, CancellationToken ct = default)
        {
            var roleInDb = await _roleManager.FindByIdAsync(request.RoleId);
            if (roleInDb == null)
            {
                var notFoundResponse = new ResultResponse();
                notFoundResponse.AddErrorMessage("Role does not exist.", "role.not_found");
                
                throw new NotFoundException(notFoundResponse);
            }

            if (roleInDb.Name == RoleConstants.Admin)
            {
                var conflictResponse = new ResultResponse();
                conflictResponse.AddErrorMessage($"Not allowed to change permissions for '{roleInDb.Name}' role.", "role.update_admin_permissions_forbidden");
                
                throw new ConflictException(conflictResponse);
            }

            if (_tenantInfoContextAccessor.MultiTenantContext.TenantInfo?.Id != TenancyConstants.Root.Id)
            {
                request.NewPermissions.RemoveAll(p => p.StartsWith("Permission.Tenants."));
            }

            var currentClaims = await _roleManager.GetClaimsAsync(roleInDb);

            foreach (var claim in currentClaims.Where(c => !request.NewPermissions.Any(p => p == c.Value)))
            {
                var result = await _roleManager.RemoveClaimAsync(roleInDb, claim);

                if (!result.Succeeded)
                {
                    var identityResponse = new ResultResponse();
                    foreach (var error in IdentityHelper.GetIdentityResultErrorDescriptions(result))
                    {
                        identityResponse.AddErrorMessage(error, "identity.operation_failed");
                    }
                    throw new IdentityException(identityResponse);
                }
            }

            foreach (var newPermission in request.NewPermissions.Where(p => !currentClaims.Any(c => c.Value == p)))
            {
                await _context
                    .RoleClaims
                    .AddAsync(new ApplicationRoleClaim
                    {
                        RoleId = roleInDb.Id,
                        ClaimType = ClaimConstants.Permission,
                        ClaimValue = newPermission,
                        Description = "",
                        Group = ""
                    });
            }

            await _context.SaveChangesAsync();

            var response = new ResultResponse();
            response.AddSuccessMessage("Role permissions updated successfully", "role.permissions_updated_successfully");
            return response;
        }
    }
}
