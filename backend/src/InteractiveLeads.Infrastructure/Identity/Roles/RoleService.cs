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

        public async Task<string> CreateAsync(CreateRoleRequest request)
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

            return newRole.Name;
        }

        public async Task<string> DeleteAsync(Guid id)
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

            return roleInDb.Name!;
        }

        public async Task<bool> DoesItExistsAsync(string name)
        {
            return await _roleManager.RoleExistsAsync(name);
        }

        public async Task<List<RoleResponse>> GetAllAsync(CancellationToken ct)
        {
            var rolesInDb = await _roleManager
                .Roles
                .ToListAsync(ct);

            return rolesInDb.Adapt<List<RoleResponse>>();
        }

        public async Task<RoleResponse> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var roleInDb = await _context.Roles
                .FirstOrDefaultAsync(role => role.Id == id, ct);
            
            if (roleInDb == null)
            {
                var notFoundResponse = new ResultResponse();
                notFoundResponse.AddErrorMessage("Role does not exist.", "role.not_found");

                throw new NotFoundException(notFoundResponse);
            }

            return roleInDb.Adapt<RoleResponse>();
        }

        public async Task<RoleResponse> GetRoleWithPermissionsAsync(Guid id, CancellationToken ct)
        {
            var role = await GetByIdAsync(id, ct);

            role.Permissions = await _context.RoleClaims
                .Where(rc => rc.RoleId == id && rc.ClaimType == ClaimConstants.Permission && rc.ClaimValue != null)
                .Select(rc => rc.ClaimValue!)
                .ToListAsync(ct);

            return role;
        }

        public async Task<string> UpdateAsync(UpdateRoleRequest request)
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
            return roleInDb.Name;
        }

        public async Task<string> UpdatePermissionsAsync(UpdateRolePermissionsRequest request)
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

            return "Permissions Updated Successfully.";
        }
    }
}
