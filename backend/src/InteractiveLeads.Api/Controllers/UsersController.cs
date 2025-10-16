using Application.Features.Identity.Users.Queries;
using InteractiveLeads.Api.Controllers.Base;
using InteractiveLeads.Application.Feature.Users;
using InteractiveLeads.Application.Feature.Users.Commands;
using InteractiveLeads.Application.Feature.Users.Queries;
using InteractiveLeads.Infrastructure.Constants;
using InteractiveLeads.Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Mvc;

namespace InteractiveLeads.Api.Controllers
{
    /// <summary>
    /// Controller for user management operations.
    /// </summary>
    /// <remarks>
    /// Provides endpoints for creating, updating, retrieving and managing system users.
    /// </remarks>
    public class UsersController : BaseApiController
    {
        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="createUser">User data to be created.</param>
        /// <returns>Result of the creation operation.</returns>
        [HttpPost("register")]
        [ShouldHavePermission(InteractiveAction.Create, InteractiveFeature.Users)]
        public async Task<IActionResult> RegisterUserAsync([FromBody] CreateUserRequest createUser)
        {
            var response = await Sender.Send(new CreateUserCommand { CreateUser = createUser });

            return Ok(response);
        }

        /// <summary>
        /// Updates the details of an existing user.
        /// </summary>
        /// <param name="updateUser">Updated user data.</param>
        /// <returns>Result of the update operation.</returns>
        [HttpPut("update")]
        [ShouldHavePermission(InteractiveAction.Update, InteractiveFeature.Users)]
        public async Task<IActionResult> UpdateUserDetailsAsync([FromBody] UpdateUserRequest updateUser)
        {
            var response = await Sender.Send(new UpdateUserCommand { UpdateUser = updateUser });

            return Ok(response);
        }

        /// <summary>
        /// Changes the activation status of a user.
        /// </summary>
        /// <param name="changeUserStatus">User status change data.</param>
        /// <returns>Result of the status change operation.</returns>
        [HttpPut("update-status")]
        [ShouldHavePermission(InteractiveAction.Update, InteractiveFeature.Users)]
        public async Task<IActionResult> ChangeUserStatusAsync([FromBody] ChangeUserStatusRequest changeUserStatus)
        {
            var response = await Sender.Send(new UpdateUserStatusCommand { ChangeUserStatus = changeUserStatus });

            return Ok(response);
        }

        /// <summary>
        /// Updates the roles assigned to a user.
        /// </summary>
        /// <param name="userRolesRequest">List of roles to be assigned to the user.</param>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <returns>Result of the roles update operation.</returns>
        [HttpPut("update-roles/{userId}")]
        [ShouldHavePermission(InteractiveAction.Update, InteractiveFeature.UserRoles)]
        public async Task<IActionResult> UpdateUserRolesAsync([FromBody] UserRolesRequest userRolesRequest, Guid userId)
        {
            var response = await Sender.Send(new UpdateUserRolesCommand { UserRolesRequest = userRolesRequest, UserId = userId });

            return Ok(response);
        }

        /// <summary>
        /// Removes a user from the system.
        /// </summary>
        /// <param name="userId">Unique identifier of the user to be removed.</param>
        /// <returns>Result of the removal operation.</returns>
        [HttpDelete("delete/{userId}")]
        [ShouldHavePermission(InteractiveAction.Delete, InteractiveFeature.Users)]
        public async Task<IActionResult> DeleteUserAsync(Guid userId)
        {
            var response = await Sender.Send(new DeleteUserCommand { UserId = userId });

            return Ok(response);
        }

        /// <summary>
        /// Retrieves all users in the system.
        /// </summary>
        /// <returns>List of all users.</returns>
        [HttpGet("all")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.Users)]
        public async Task<IActionResult> GetUsersAsync()
        {
            var response = await Sender.Send(new GetAllUsersQuery());

            return Ok(response);
        }

        /// <summary>
        /// Retrieves a specific user by their identifier.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <returns>Requested user data.</returns>
        [HttpGet("{userId}")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.Users)]
        public async Task<IActionResult> GetUserByIdAsync(Guid userId)
        {
            var response = await Sender.Send(new GetUserByIdQuery { UserId = userId });

            return Ok(response);
        }

        /// <summary>
        /// Retrieves all permissions assigned to a specific user.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <returns>List of user permissions.</returns>
        [HttpGet("permissions/{userId}")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.RoleClaims)]
        public async Task<IActionResult> GetUserPermissionsAsync(Guid userId)
        {
            var response = await Sender.Send(new GetUserPermissionsQuery { UserId = userId });

            return Ok(response);
        }

        /// <summary>
        /// Retrieves all roles assigned to a specific user.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <returns>List of user roles.</returns>
        [HttpGet("user-roles/{userId}")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.UserRoles)]
        public async Task<IActionResult> GetUserRolesAsync(Guid userId)
        {
            var response = await Sender.Send(new GetUserRolesQuery { UserId = userId });

            return Ok(response);
        }
    }
}
