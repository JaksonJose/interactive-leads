
using Finbuckle.MultiTenant.Abstractions;
using InteractiveLeads.Infrastructure.Constants;
using InteractiveLeads.Infrastructure.Context.Tenancy;
using InteractiveLeads.Infrastructure.Identity.Models;
using InteractiveLeads.Infrastructure.Tenancy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace InteractiveLeads.Infrastructure.Context.Application
{
    public class ApplicationDbSeeder(
        IMultiTenantContextAccessor<InteractiveTenantInfo> tenantInfoContextAccessor,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext applicationDbContext,
        IServiceProvider serviceProvider)
    {
        private readonly IMultiTenantContextAccessor<InteractiveTenantInfo> _tenantInfoContextAccessor = tenantInfoContextAccessor;
        private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
        {
            if (_applicationDbContext.Database.GetMigrations().Any())
            {
                if ((await _applicationDbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any()) 
                { 
                    await _applicationDbContext.Database.MigrateAsync(cancellationToken);
                }

                if (await _applicationDbContext.Database.CanConnectAsync(cancellationToken))
                {
                    await InitializeDefaultRoleAsync(cancellationToken);
                    await InitializeAdminUserAsync();
                }
            }
        }

        private async Task InitializeDefaultRoleAsync(CancellationToken cancellationToken)
        {
            foreach (var roleName in RoleConstants.DefaultRoles)
            {
                if (await _roleManager.Roles.SingleOrDefaultAsync(role => role.Name == roleName, cancellationToken) is not ApplicationRole incomingRole)
                {
                    incomingRole = new ApplicationRole()
                    {
                        Name = roleName,
                        Description = $"{roleName} Role",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    };

                    await _roleManager.CreateAsync(incomingRole);
                }

                // Assign permissions
                if (roleName == RoleConstants.Basic)
                {
                    await AssignPermissionsToRole(InteractivePermissions.Basic, incomingRole, cancellationToken);
                }
                else if (roleName == RoleConstants.Admin)
                {
                    await AssignPermissionsToRole(InteractivePermissions.Admin, incomingRole, cancellationToken);

                    if (_tenantInfoContextAccessor.MultiTenantContext.TenantInfo?.Id == TenancyConstants.Root.Id)
                    {
                        await AssignPermissionsToRole(InteractivePermissions.Root, incomingRole, cancellationToken);
                    }
                }
            }
        }

        private async Task AssignPermissionsToRole(IReadOnlyList<InteractivePermission> rolePermissions, ApplicationRole role, CancellationToken cancellationToken)
        {
            IList<Claim> currentClaims = await _roleManager.GetClaimsAsync(role);

            foreach (var rolePermission in rolePermissions)
            {
                if (!currentClaims.Any(c => c.Type == ClaimConstants.Permission && c.Value == rolePermission.Name))
                {
                    await _applicationDbContext.RoleClaims.AddAsync(new ApplicationRoleClaim
                    {
                        RoleId = role.Id,
                        ClaimType = ClaimConstants.Permission,
                        ClaimValue = rolePermission.Name,
                        Description = rolePermission.Description,
                        Group = rolePermission.group,
                    }, cancellationToken);

                    await _applicationDbContext.SaveChangesAsync(cancellationToken);
                }
            }
        }
        private async Task InitializeAdminUserAsync()
        {
            if (string.IsNullOrEmpty(_tenantInfoContextAccessor.MultiTenantContext.TenantInfo?.Email)) return;

            if (await _userManager.Users.SingleOrDefaultAsync(user => user.Email == _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email) is not ApplicationUser incomingUser)
            {
                incomingUser = new ApplicationUser
                {
                    FirstName = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.FirstName,
                    LastName = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.LastName,
                    Email = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email,
                    UserName = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email,
                    NormalizedEmail = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email.ToUpperInvariant(),
                    NormalizedUserName = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo?.Email?.ToUpperInvariant(),
                    TenantId = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Id, // ← Set user's tenant
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    IsActive = true
                };

                var passwordHash = new PasswordHasher<ApplicationUser>();

                incomingUser.PasswordHash = passwordHash.HashPassword(incomingUser, TenancyConstants.DefaultPassword);
                await _userManager.CreateAsync(incomingUser);

                // Create user-tenant mapping for optimized performance
                await CreateUserTenantMappingAsync(incomingUser.Email, incomingUser.TenantId);
            }

            if (!await _userManager.IsInRoleAsync(incomingUser, RoleConstants.Admin))
            {
                await _userManager.AddToRoleAsync(incomingUser, RoleConstants.Admin);
            }
        }

        private async Task CreateUserTenantMappingAsync(string email, string tenantId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var tenantDbContext = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
                
                // Check if mapping already exists
                var existingMapping = await tenantDbContext.UserTenantMappings
                    .Where(m => m.Email == email)
                    .FirstOrDefaultAsync();
                
                if (existingMapping == null)
                {
                    var mapping = new UserTenantMapping
                    {
                        Email = email,
                        TenantId = tenantId,
                        IsActive = true
                    };
                    
                    tenantDbContext.UserTenantMappings.Add(mapping);
                    await tenantDbContext.SaveChangesAsync();
                }
            }
            catch
            {
                // Log error if needed, but don't fail the user creation
                // The system can still work with the fallback strategy
            }
        }
    }
}
