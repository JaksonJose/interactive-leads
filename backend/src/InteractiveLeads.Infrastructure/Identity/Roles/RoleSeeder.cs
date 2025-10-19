using InteractiveLeads.Application.Responses;
using InteractiveLeads.Infrastructure.Constants;
using InteractiveLeads.Infrastructure.Context.Application;
using InteractiveLeads.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InteractiveLeads.Infrastructure.Identity.Roles
{
    /// <summary>
    /// Service for seeding roles and permissions into the database.
    /// </summary>
    /// <remarks>
    /// Automatically creates all system roles with their appropriate permissions
    /// during database initialization.
    /// </remarks>
    public class RoleSeeder
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the RoleSeeder class.
        /// </summary>
        /// <param name="roleManager">The role manager for role operations.</param>
        /// <param name="context">The application database context.</param>
        public RoleSeeder(RoleManager<ApplicationRole> roleManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }

        /// <summary>
        /// Seeds all system roles and their permissions.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result of the seeding operation.</returns>
        public async Task<ResultResponse> SeedRolesAsync(CancellationToken cancellationToken = default)
        {
            var response = new ResultResponse();
            var createdRoles = new List<string>();

            try
            {
                // Seed all roles
                await SeedRoleAsync(RoleConstants.SysAdmin, "System Administrator", "Full system access across all tenants", InteractivePermissions.SysAdmin, createdRoles, cancellationToken);
                await SeedRoleAsync(RoleConstants.Support, "Support User", "Limited cross-tenant access for customer support", InteractivePermissions.Support, createdRoles, cancellationToken);
                await SeedRoleAsync(RoleConstants.Owner, "Tenant Owner", "Full control over tenant operations", InteractivePermissions.Owner, createdRoles, cancellationToken);
                await SeedRoleAsync(RoleConstants.Manager, "Tenant Manager", "User management within tenant", InteractivePermissions.Manager, createdRoles, cancellationToken);
                await SeedRoleAsync(RoleConstants.Agent, "Tenant Agent", "Basic operations within tenant", InteractivePermissions.Agent, createdRoles, cancellationToken);

                // Seed legacy roles for backward compatibility
                await SeedRoleAsync(RoleConstants.Admin, "Tenant Administrator", "Legacy admin role - use Owner instead", InteractivePermissions.Admin, createdRoles, cancellationToken);
                await SeedRoleAsync(RoleConstants.Basic, "Basic User", "Legacy basic role - use Agent instead", InteractivePermissions.Basic, createdRoles, cancellationToken);

                response.AddSuccessMessage($"Successfully seeded {createdRoles.Count} roles: {string.Join(", ", createdRoles)}", "roles.seeded_successfully");
            }
            catch (Exception ex)
            {
                response.AddErrorMessage($"Error seeding roles: {ex.Message}", "roles.seeding_failed");
            }

            return response;
        }

        /// <summary>
        /// Seeds a specific role with its permissions.
        /// </summary>
        /// <param name="roleName">The role name.</param>
        /// <param name="description">The role description.</param>
        /// <param name="longDescription">The detailed role description.</param>
        /// <param name="permissions">The permissions to assign to the role.</param>
        /// <param name="createdRoles">List to track created roles.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        private async Task SeedRoleAsync(string roleName, string description, string longDescription, 
            IReadOnlyList<InteractivePermission> permissions, List<string> createdRoles, CancellationToken cancellationToken)
        {
            // Check if role already exists
            var existingRole = await _roleManager.FindByNameAsync(roleName);
            if (existingRole != null)
            {
                // Update existing role permissions
                await UpdateRolePermissionsAsync(existingRole, permissions, cancellationToken);
                return;
            }

            // Create new role
            var role = new ApplicationRole
            {
                Name = roleName,
                Description = description,
                NormalizedName = roleName.ToUpperInvariant()
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Add permissions to the role
            await AddPermissionsToRoleAsync(role, permissions, cancellationToken);
            createdRoles.Add(roleName);
        }

        /// <summary>
        /// Updates permissions for an existing role.
        /// </summary>
        /// <param name="role">The role to update.</param>
        /// <param name="permissions">The new permissions.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        private async Task UpdateRolePermissionsAsync(ApplicationRole role, IReadOnlyList<InteractivePermission> permissions, CancellationToken cancellationToken)
        {
            // Get current permissions
            var currentClaims = await _roleManager.GetClaimsAsync(role);
            var currentPermissions = currentClaims
                .Where(c => c.Type == ClaimConstants.Permission)
                .Select(c => c.Value)
                .ToList();

            // Get new permission names
            var newPermissions = permissions.Select(p => p.Name).ToList();

            // Remove permissions that are no longer needed
            var permissionsToRemove = currentPermissions.Except(newPermissions).ToList();
            foreach (var permission in permissionsToRemove)
            {
                var claim = currentClaims.FirstOrDefault(c => c.Type == ClaimConstants.Permission && c.Value == permission);
                if (claim != null)
                {
                    await _roleManager.RemoveClaimAsync(role, claim);
                }
            }

            // Add new permissions
            var permissionsToAdd = newPermissions.Except(currentPermissions).ToList();
            await AddPermissionsToRoleAsync(role, permissions.Where(p => permissionsToAdd.Contains(p.Name)).ToList(), cancellationToken);
        }

        /// <summary>
        /// Adds permissions to a role.
        /// </summary>
        /// <param name="role">The role to add permissions to.</param>
        /// <param name="permissions">The permissions to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        private async Task AddPermissionsToRoleAsync(ApplicationRole role, IReadOnlyList<InteractivePermission> permissions, CancellationToken cancellationToken)
        {
            foreach (var permission in permissions)
            {
                var claim = new ApplicationRoleClaim
                {
                    RoleId = role.Id,
                    ClaimType = ClaimConstants.Permission,
                    ClaimValue = permission.Name,
                    Description = permission.Description,
                    Group = permission.group
                };

                await _context.RoleClaims.AddAsync(claim, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Gets all seeded roles with their permissions.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of roles with their permissions.</returns>
        public async Task<List<RoleWithPermissionsResponse>> GetSeededRolesAsync(CancellationToken cancellationToken = default)
        {
            var roles = await _context.Roles
                .Include(r => r.Claims)
                .ToListAsync(cancellationToken);

            return roles.Select(role => new RoleWithPermissionsResponse
            {
                Id = role.Id,
                Name = role.Name!,
                Description = role.Description,
                Permissions = role.Claims
                    .Where(c => c.ClaimType == ClaimConstants.Permission)
                    .Select(c => c.ClaimValue!)
                    .ToList()
            }).ToList();
        }

        /// <summary>
        /// Clears all roles and permissions (for testing purposes).
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result of the operation.</returns>
        public async Task<ResultResponse> ClearAllRolesAsync(CancellationToken cancellationToken = default)
        {
            var response = new ResultResponse();

            try
            {
                // Remove all role claims first
                var allRoleClaims = await _context.RoleClaims.ToListAsync(cancellationToken);
                _context.RoleClaims.RemoveRange(allRoleClaims);

                // Remove all roles
                var allRoles = await _context.Roles.ToListAsync(cancellationToken);
                _context.Roles.RemoveRange(allRoles);

                await _context.SaveChangesAsync(cancellationToken);

                response.AddSuccessMessage("All roles and permissions cleared successfully", "roles.cleared_successfully");
            }
            catch (Exception ex)
            {
                response.AddErrorMessage($"Error clearing roles: {ex.Message}", "roles.clear_failed");
            }

            return response;
        }
    }

    /// <summary>
    /// Response model for role with permissions.
    /// </summary>
    public class RoleWithPermissionsResponse
    {
        /// <summary>
        /// The role ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The role name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The role description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The role permissions.
        /// </summary>
        public List<string> Permissions { get; set; } = new();
    }
}
