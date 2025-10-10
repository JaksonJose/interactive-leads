using InteractiveLeads.Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;

namespace InteractiveLeads.Infrastructure.Identity.Auth
{
    /// <summary>
    /// Authorization handler that evaluates permission-based authorization requirements.
    /// </summary>
    /// <remarks>
    /// Checks if the current user has the required permission by examining their
    /// permission claims. If a matching permission claim is found, the authorization succeeds.
    /// </remarks>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        /// <summary>
        /// Handles the authorization requirement by checking if the user has the required permission.
        /// </summary>
        /// <param name="context">The authorization context containing user claims.</param>
        /// <param name="requirement">The permission requirement to evaluate.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var permissions = context.User.Claims
                .Where(claim => claim.Type == ClaimConstants.Permission 
                    && claim.Value == requirement.Permission);

            if (permissions.Any()) 
            {
                context.Succeed(requirement);
                await Task.CompletedTask;
            }
        }
    }
}
