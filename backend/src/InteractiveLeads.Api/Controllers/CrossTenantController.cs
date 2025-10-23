using InteractiveLeads.Api.Controllers.Base;
using InteractiveLeads.Application.Feature.CrossTenant.Commands;
using InteractiveLeads.Application.Feature.CrossTenant.Queries;
using InteractiveLeads.Application.Feature.Tenancy.Queries;
using InteractiveLeads.Application.Feature.Users;
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
        /// <summary>
        /// Initializes a new instance of the CrossTenantController class.
        /// </summary>
        public CrossTenantController()
        {
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
            var response = await Sender.Send(new GetUsersInTenantQuery { TenantId = tenantId });
            
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
            var response = await Sender.Send(new GetUserInTenantQuery { TenantId = tenantId, UserId = userId });
            
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
            var response = await Sender.Send(new CreateUserInTenantCommand 
            { 
                TenantId = tenantId, 
                CreateUser = createUser 
            });
            
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
            var response = await Sender.Send(new UpdateUserInTenantCommand 
            { 
                TenantId = tenantId, 
                UserId = userId, 
                UpdateUser = updateUser 
            });
            
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
            var response = await Sender.Send(new ChangeUserStatusInTenantCommand 
            { 
                TenantId = tenantId, 
                UserId = userId, 
                ChangeUserStatus = changeUserStatus 
            });
            
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
            var response = await Sender.Send(new UpdateUserRolesInTenantCommand 
            { 
                TenantId = tenantId, 
                UserId = userId, 
                UserRolesRequest = userRolesRequest 
            });
            
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
            var response = await Sender.Send(new DeleteUserInTenantCommand 
            { 
                TenantId = tenantId, 
                UserId = userId 
            });
            
            return Ok(response);
        }

        /// <summary>
        /// Gets a specific tenant with its associated user - available for SysAdmin and Support.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant to retrieve.</param>
        /// <returns>Tenant information with its associated user.</returns>
        /// <remarks>
        /// Requires Read permission for CrossTenantTenants feature.
        /// Returns tenant details and finds the associated user by matching the tenant's email.
        /// </remarks>
        [HttpGet("tenants/{tenantId}")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.CrossTenantTenants)]
        [OpenApiOperation("Get a specific tenant with its associated user")]
        public async Task<IActionResult> GetTenantWithUserAsync(string tenantId)
        {
            var response = await Sender.Send(new GetTenantWithUserQuery { TenantId = tenantId });
            
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
        /// For cross-tenant users, returns all tenants. For regular users, returns only their tenant.
        /// </remarks>
        [HttpGet("tenants")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.CrossTenantTenants)]
        [OpenApiOperation("Get accessible tenants for current user")]
        public async Task<IActionResult> GetAccessibleTenantsAsync(int pageNumber = 1, int pageSize = 50)
        {
            var pagination = new PaginationRequest
            {
                Page = pageNumber,
                PageSize = pageSize
            };

            var response = await Sender.Send(new GetAccessibleTenantsQuery { Pagination = pagination });

            return Ok(response);
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
            var response = await Sender.Send(new GetUserPermissionsInTenantQuery 
            { 
                TenantId = tenantId, 
                UserId = userId 
            });
            
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
            var response = await Sender.Send(new GetUserRolesInTenantQuery 
            { 
                TenantId = tenantId, 
                UserId = userId 
            });
            
            return Ok(response);
        }
    }
}
