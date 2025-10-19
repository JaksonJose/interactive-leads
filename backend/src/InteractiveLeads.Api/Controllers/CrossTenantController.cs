using InteractiveLeads.Api.Controllers.Base;
using InteractiveLeads.Application.Feature.Users;
using InteractiveLeads.Application.Feature.Users.Commands;
using Application.Features.Identity.Users.Queries;
using InteractiveLeads.Application.Feature.Users.Queries;
using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Models;
using InteractiveLeads.Infrastructure.Constants;
using InteractiveLeads.Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace InteractiveLeads.Api.Controllers
{
    /// <summary>
    /// Controller for cross-tenant operations allowing access to multiple tenants.
    /// </summary>
    /// <remarks>
    /// Provides endpoints for SysAdmin and Support users to manage users across different tenants.
    /// Requires appropriate cross-tenant permissions.
    /// </remarks>
    public class CrossTenantController : BaseApiController
    {
        private readonly ICrossTenantService _crossTenantService;
        private readonly ICrossTenantAuthorizationService _authService;
        private readonly ICurrentUserService _currentUserService;

        /// <summary>
        /// Initializes a new instance of the CrossTenantController class.
        /// </summary>
        /// <param name="crossTenantService">The cross-tenant service for context switching.</param>
        /// <param name="authService">The authorization service for cross-tenant operations.</param>
        /// <param name="currentUserService">The current user service.</param>
        public CrossTenantController(
            ICrossTenantService crossTenantService,
            ICrossTenantAuthorizationService authService,
            ICurrentUserService currentUserService)
        {
            _crossTenantService = crossTenantService;
            _authService = authService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Lists users in a specific tenant - available for SysAdmin and Support.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant to list users from.</param>
        /// <returns>List of users in the specified tenant.</returns>
        /// <remarks>
        /// Requires Read permission for CrossTenantUsers feature.
        /// </remarks>
        [HttpGet("tenants/{tenantId}/users")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.CrossTenantUsers)]
        [OpenApiOperation("List users in a specific tenant")]
        public async Task<IActionResult> GetUsersInTenantAsync(string tenantId)
        {
            var response = await _crossTenantService.ExecuteInTenantContextAsync(tenantId,
                async () => await Sender.Send(new GetAllUsersQuery()));
            
            return Ok(response);
        }

        /// <summary>
        /// Gets a specific user from a tenant - available for SysAdmin and Support.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <returns>User data from the specified tenant.</returns>
        /// <remarks>
        /// Requires Read permission for CrossTenantUsers feature.
        /// </remarks>
        [HttpGet("tenants/{tenantId}/users/{userId}")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.CrossTenantUsers)]
        [OpenApiOperation("Get a specific user from a tenant")]
        public async Task<IActionResult> GetUserInTenantAsync(string tenantId, Guid userId)
        {
            var response = await _crossTenantService.ExecuteInTenantContextAsync(tenantId,
                async () => await Sender.Send(new GetUserByIdQuery { UserId = userId }));
            
            return Ok(response);
        }

        /// <summary>
        /// Creates a new user in a specific tenant - SysAdmin only.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant to create the user in.</param>
        /// <param name="createUser">User data to be created.</param>
        /// <returns>Result of the user creation operation.</returns>
        /// <remarks>
        /// Requires Create permission for CrossTenantUsers feature.
        /// Only SysAdmin users can create users in other tenants.
        /// </remarks>
        [HttpPost("tenants/{tenantId}/users")]
        [ShouldHavePermission(InteractiveAction.Create, InteractiveFeature.CrossTenantUsers)]
        [OpenApiOperation("Create a user in a specific tenant")]
        public async Task<IActionResult> CreateUserInTenantAsync(string tenantId, [FromBody] CreateUserRequest createUser)
        {
            // Additional verification: only SysAdmin can create users
            var userIdString = _currentUserService.GetUserId();
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Forbid("Invalid user ID");
            }
            if (!await _authService.IsSystemAdminAsync(userId))
            {
                return Forbid("Only system administrators can create users in other tenants");
            }

            var response = await _crossTenantService.ExecuteInTenantContextAsync(tenantId,
                async () => await Sender.Send(new CreateUserCommand { CreateUser = createUser }));
            
            return Ok(response);
        }

        /// <summary>
        /// Updates a user in a specific tenant - available for SysAdmin and Support.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="userId">The ID of the user to update.</param>
        /// <param name="updateUser">Updated user data.</param>
        /// <returns>Result of the user update operation.</returns>
        /// <remarks>
        /// Requires Update permission for CrossTenantUsers feature.
        /// </remarks>
        [HttpPut("tenants/{tenantId}/users/{userId}")]
        [ShouldHavePermission(InteractiveAction.Update, InteractiveFeature.CrossTenantUsers)]
        [OpenApiOperation("Update a user in a specific tenant")]
        public async Task<IActionResult> UpdateUserInTenantAsync(string tenantId, Guid userId, [FromBody] UpdateUserRequest updateUser)
        {
            var response = await _crossTenantService.ExecuteInTenantContextAsync(tenantId,
                async () => await Sender.Send(new UpdateUserCommand { UpdateUser = updateUser }));
            
            return Ok(response);
        }

        /// <summary>
        /// Changes user status in a specific tenant - available for SysAdmin and Support.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="userId">The ID of the user to update.</param>
        /// <param name="changeUserStatus">User status change data.</param>
        /// <returns>Result of the status change operation.</returns>
        /// <remarks>
        /// Requires Update permission for CrossTenantUsers feature.
        /// </remarks>
        [HttpPut("tenants/{tenantId}/users/{userId}/status")]
        [ShouldHavePermission(InteractiveAction.Update, InteractiveFeature.CrossTenantUsers)]
        [OpenApiOperation("Change user status in a specific tenant")]
        public async Task<IActionResult> ChangeUserStatusInTenantAsync(string tenantId, Guid userId, [FromBody] ChangeUserStatusRequest changeUserStatus)
        {
            var response = await _crossTenantService.ExecuteInTenantContextAsync(tenantId,
                async () => await Sender.Send(new UpdateUserStatusCommand { ChangeUserStatus = changeUserStatus }));
            
            return Ok(response);
        }

        /// <summary>
        /// Updates user roles in a specific tenant - available for SysAdmin and Support.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="userId">The ID of the user to update.</param>
        /// <param name="userRolesRequest">List of roles to be assigned to the user.</param>
        /// <returns>Result of the roles update operation.</returns>
        /// <remarks>
        /// Requires Update permission for CrossTenantRoles feature.
        /// </remarks>
        [HttpPut("tenants/{tenantId}/users/{userId}/roles")]
        [ShouldHavePermission(InteractiveAction.Update, InteractiveFeature.CrossTenantRoles)]
        [OpenApiOperation("Update user roles in a specific tenant")]
        public async Task<IActionResult> UpdateUserRolesInTenantAsync(string tenantId, Guid userId, [FromBody] UserRolesRequest userRolesRequest)
        {
            var response = await _crossTenantService.ExecuteInTenantContextAsync(tenantId,
                async () => await Sender.Send(new UpdateUserRolesCommand { UserRolesRequest = userRolesRequest, UserId = userId }));
            
            return Ok(response);
        }

        /// <summary>
        /// Deletes a user from a specific tenant - SysAdmin only.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <returns>Result of the user deletion operation.</returns>
        /// <remarks>
        /// Requires Delete permission for CrossTenantUsers feature.
        /// Only SysAdmin users can delete users from other tenants.
        /// </remarks>
        [HttpDelete("tenants/{tenantId}/users/{userId}")]
        [ShouldHavePermission(InteractiveAction.Delete, InteractiveFeature.CrossTenantUsers)]
        [OpenApiOperation("Delete a user from a specific tenant")]
        public async Task<IActionResult> DeleteUserInTenantAsync(string tenantId, Guid userId)
        {
            // Additional verification: only SysAdmin can delete users
            var userIdString2 = _currentUserService.GetUserId();
            if (!Guid.TryParse(userIdString2, out var userId2))
            {
                return Forbid("Invalid user ID");
            }
            if (!await _authService.IsSystemAdminAsync(userId2))
            {
                return Forbid("Only system administrators can delete users from other tenants");
            }

            var response = await _crossTenantService.ExecuteInTenantContextAsync(tenantId,
                async () => await Sender.Send(new DeleteUserCommand { UserId = userId }));
            
            return Ok(response);
        }

        /// <summary>
        /// Gets all tenants accessible to the current user.
        /// </summary>
        /// <param name="pageNumber">Page number for pagination (default: 1).</param>
        /// <param name="pageSize">Number of items per page (default: 50).</param>
        /// <returns>List of accessible tenant IDs with pagination info.</returns>
        /// <remarks>
        /// Returns different tenant lists based on the user's role.
        /// For cross-tenant users, returns "*" indicating access to all tenants.
        /// </remarks>
        [HttpGet("tenants")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.CrossTenantTenants)]
        [OpenApiOperation("Get accessible tenants for current user")]
        public async Task<IActionResult> GetAccessibleTenantsAsync(int pageNumber = 1, int pageSize = 50)
        {
            var userIdString = _currentUserService.GetUserId();
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return BadRequest("Invalid user ID");
            }

            // Check if user has cross-tenant access
            var hasAllTenantsAccess = await _authService.HasAllTenantsAccessAsync(userId);
            
            if (hasAllTenantsAccess)
            {
                // For cross-tenant users, return special marker
                return Ok(new { 
                    TenantIds = new[] { "*" }, 
                    HasMore = false,
                    Message = "User has access to all tenants" 
                });
            }

            // For regular users, get their specific tenant
            var accessibleTenants = await _authService.GetAccessibleTenantsAsync(userId);
            return Ok(new { 
                TenantIds = accessibleTenants, 
                HasMore = false,
                Message = "User has access to specific tenants" 
            });
        }

        /// <summary>
        /// Gets user permissions for a specific user in a tenant - available for SysAdmin and Support.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="userId">The ID of the user to get permissions for.</param>
        /// <returns>List of user permissions.</returns>
        /// <remarks>
        /// Requires Read permission for CrossTenantRoles feature.
        /// </remarks>
        [HttpGet("tenants/{tenantId}/users/{userId}/permissions")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.CrossTenantRoles)]
        [OpenApiOperation("Get user permissions in a specific tenant")]
        public async Task<IActionResult> GetUserPermissionsInTenantAsync(string tenantId, Guid userId)
        {
            var response = await _crossTenantService.ExecuteInTenantContextAsync(tenantId,
                async () => await Sender.Send(new GetUserPermissionsQuery { UserId = userId }));
            
            return Ok(response);
        }

        /// <summary>
        /// Gets user roles for a specific user in a tenant - available for SysAdmin and Support.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="userId">The ID of the user to get roles for.</param>
        /// <returns>List of user roles.</returns>
        /// <remarks>
        /// Requires Read permission for CrossTenantRoles feature.
        /// </remarks>
        [HttpGet("tenants/{tenantId}/users/{userId}/roles")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.CrossTenantRoles)]
        [OpenApiOperation("Get user roles in a specific tenant")]
        public async Task<IActionResult> GetUserRolesInTenantAsync(string tenantId, Guid userId)
        {
            var response = await _crossTenantService.ExecuteInTenantContextAsync(tenantId,
                async () => await Sender.Send(new GetUserRolesQuery { UserId = userId }));
            
            return Ok(response);
        }
    }
}
