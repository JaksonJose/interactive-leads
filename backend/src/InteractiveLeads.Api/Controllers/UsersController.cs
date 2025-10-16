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
    public class UsersController : BaseApiController
    {
        [HttpPost("register")]
        [ShouldHavePermission(InteractiveAction.Create, InteractiveFeature.Users)]
        public async Task<IActionResult> RegisterUserAsync([FromBody] CreateUserRequest createUser)
        {
            var response = await Sender.Send(new CreateUserCommand { CreateUser = createUser });

            return Ok(response);
        }

        [HttpPut("update")]
        [ShouldHavePermission(InteractiveAction.Update, InteractiveFeature.Users)]
        public async Task<IActionResult> UpdateUserDetailsAsync([FromBody] UpdateUserRequest updateUser)
        {
            var response = await Sender.Send(new UpdateUserCommand { UpdateUser = updateUser });

            return Ok(response);
        }

        [HttpPut("update-status")]
        [ShouldHavePermission(InteractiveAction.Update, InteractiveFeature.Users)]
        public async Task<IActionResult> ChangeUserStatusAsync([FromBody] ChangeUserStatusRequest changeUserStatus)
        {
            var response = await Sender.Send(new UpdateUserStatusCommand { ChangeUserStatus = changeUserStatus });

            return Ok(response);
        }

        [HttpPut("update-roles/{userId}")]
        [ShouldHavePermission(InteractiveAction.Update, InteractiveFeature.UserRoles)]
        public async Task<IActionResult> UpdateUserRolesAsync([FromBody] UserRolesRequest userRolesRequest, Guid userId)
        {
            var response = await Sender.Send(new UpdateUserRolesCommand { UserRolesRequest = userRolesRequest, UserId = userId });

            return Ok(response);
        }

        [HttpDelete("delete/{userId}")]
        [ShouldHavePermission(InteractiveAction.Delete, InteractiveFeature.Users)]
        public async Task<IActionResult> DeleteUserAsync(Guid userId)
        {
            var response = await Sender.Send(new DeleteUserCommand { UserId = userId });

            return Ok(response);
        }

        [HttpGet("all")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.Users)]
        public async Task<IActionResult> GetUsersAsync()
        {
            var response = await Sender.Send(new GetAllUsersQuery());

            return Ok(response);
        }

        [HttpGet("{userId}")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.Users)]
        public async Task<IActionResult> GetUserByIdAsync(Guid userId)
        {
            var response = await Sender.Send(new GetUserByIdQuery { UserId = userId });

            return Ok(response);
        }

        [HttpGet("permissions/{userId}")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.RoleClaims)]
        public async Task<IActionResult> GetUserPermissionsAsync(Guid userId)
        {
            var response = await Sender.Send(new GetUserPermissionsQuery { UserId = userId });

            return Ok(response);
        }

        [HttpGet("user-roles/{userId}")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.UserRoles)]
        public async Task<IActionResult> GetUserRolesAsync(Guid userId)
        {
            var response = await Sender.Send(new GetUserRolesQuery { UserId = userId });

            return Ok(response);
        }
    }
}
