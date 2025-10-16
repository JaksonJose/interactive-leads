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
        /// <param name="action">The action identifier (e.g., "Create", "Read", "Update", "Delete").</param>
        /// <param name="feature">The feature identifier (e.g., "Tenants", "Users").</param>
        public ShouldHavePermissionAttribute(string action, string feature)
        {
            Policy = InteractivePermission.NameFor(action, feature);
        }
    }
}
