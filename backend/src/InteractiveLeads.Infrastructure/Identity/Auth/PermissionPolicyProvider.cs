using InteractiveLeads.Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace InteractiveLeads.Infrastructure.Identity.Auth
{
    /// <summary>
    /// Custom authorization policy provider that dynamically creates policies for permissions.
    /// </summary>
    /// <param name="options">The authorization options configuration.</param>
    /// <remarks>
    /// This provider intercepts policy requests and creates permission-based policies
    /// on-the-fly when the policy name starts with the permission prefix. For all other
    /// policies, it delegates to the default policy provider.
    /// </remarks>
    public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
    {
        /// <summary>
        /// Gets the fallback policy provider for non-permission policies.
        /// </summary>
        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; } = new DefaultAuthorizationPolicyProvider(options);

        /// <summary>
        /// Gets the default authorization policy.
        /// </summary>
        /// <returns>A task containing the default authorization policy.</returns>
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return FallbackPolicyProvider.GetDefaultPolicyAsync();
        }

        /// <summary>
        /// Gets the fallback authorization policy.
        /// </summary>
        /// <returns>A task containing null as this provider does not define a fallback policy.</returns>
        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return Task.FromResult<AuthorizationPolicy?>(null);
        }

        /// <summary>
        /// Gets an authorization policy by name, creating permission policies dynamically.
        /// </summary>
        /// <param name="permission">The policy name (permission name if it's a permission policy).</param>
        /// <returns>A task containing the authorization policy if found or created, otherwise null.</returns>
        /// <remarks>
        /// If the policy name starts with "permission", creates a new policy with a PermissionRequirement.
        /// Otherwise, delegates to the fallback policy provider.
        /// </remarks>
        public Task<AuthorizationPolicy?> GetPolicyAsync(string permission)
        {
            if (permission.StartsWith(ClaimConstants.Permission, StringComparison.OrdinalIgnoreCase))
            {
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new PermissionRequirement(permission));

                return Task.FromResult<AuthorizationPolicy?>(policy.Build());
            }

            return FallbackPolicyProvider.GetPolicyAsync(permission);
        }
    }
}
