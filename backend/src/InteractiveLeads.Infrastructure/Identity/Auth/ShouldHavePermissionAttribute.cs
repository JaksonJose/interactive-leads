using InteractiveLeads.Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;

namespace InteractiveLeads.Infrastructure.Identity.Auth
{
    /// <summary>
    /// Authorization attribute that requires a specific permission to access an endpoint.
    /// </summary>
    /// <remarks>
    /// Apply this attribute to controller actions to enforce permission-based authorization.
    /// The permission is constructed from the feature and action parameters using the
    /// InteractivePermission naming convention.
    /// </remarks>
    public class ShouldHavePermissionAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Initializes a new instance of the ShoudHavePermissionAttribute class.
        /// </summary>
        /// <param name="feature">The feature identifier (e.g., "Tenants", "Users").</param>
        /// <param name="action">The action identifier (e.g., "Create", "Read", "Update", "Delete").</param>
        public ShouldHavePermissionAttribute(string feature, string action)
        {
            Policy = InteractivePermission.NameFor(action, feature);
        }
    }
}
