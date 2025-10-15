using InteractiveLeads.Api.Controllers.Base;
using InteractiveLeads.Application.Feature.Identity.Roles;
using InteractiveLeads.Application.Feature.Identity.Roles.Commands;
using InteractiveLeads.Application.Feature.Identity.Roles.Queries;
using InteractiveLeads.Infrastructure.Constants;
using InteractiveLeads.Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Mvc;

namespace InteractiveLeads.Api.Controllers
{
    public class RolesController : BaseApiController
    {
        [HttpPost("add")]
        [ShouldHavePermission(InteractiveAction.Create, InteractiveFeature.Roles)]
        public async Task<IActionResult> AddRoleAsync([FromBody] CreateRoleRequest createRole)
        {
            var response = await Sender.Send(new CreateRoleCommand { CreateRole = createRole });

            return Ok(response);
        }

        [HttpPut("update")]
        [ShouldHavePermission(InteractiveAction.Update, InteractiveFeature.Roles)]
        public async Task<IActionResult> UpdateRoleAsync([FromBody] UpdateRoleRequest updateRole)
        {
            var response = await Sender.Send(new UpdateRoleCommand { UpdateRole = updateRole });

            return Ok(response);
        }

        [HttpPut("update-permissions")]
        [ShouldHavePermission(InteractiveAction.Update, InteractiveFeature.RoleClaims)]
        public async Task<IActionResult> UpdateRoleClaimsAsync([FromBody] UpdateRolePermissionsRequest updateRoleClaims)
        {
            var response = await Sender.Send(new UpdateRolePermissionsCommand { UpdateRolePermissions = updateRoleClaims });

            return Ok(response);
        }

        [HttpDelete("delete/{roleId}")]
        [ShouldHavePermission(InteractiveAction.Delete, InteractiveFeature.Roles)]
        public async Task<IActionResult> DeleteRoleAsync(Guid roleId)
        {
            var response = await Sender.Send(new DeleteRoleCommand { RoleId = roleId });

            return Ok(response);
        }

        [HttpGet("all")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.Roles)]
        public async Task<IActionResult> GetRolesAsync()
        {
            var response = await Sender.Send(new GetRolesQuery());

            return Ok(response);
        }

        [HttpGet("partial/{roleId}")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.Roles)]
        public async Task<IActionResult> GetPartialRoleByIdAsync(Guid roleId)
        {
            var response = await Sender.Send(new GetRoleByIdQuery { RoleId = roleId });

            return Ok(response);
        }

        [HttpGet("full/{roleId}")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.Roles)]
        public async Task<IActionResult> GetDetailedRoleByIdAsync(Guid roleId)
        {
            var response = await Sender.Send(new GetRoleWithPermissionsQuery { RoleId = roleId });

            return Ok(response);
        }
    }
}
