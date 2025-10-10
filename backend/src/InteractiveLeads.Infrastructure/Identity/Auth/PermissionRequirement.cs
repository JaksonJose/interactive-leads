using Microsoft.AspNetCore.Authorization;

namespace InteractiveLeads.Infrastructure.Identity.Auth
{
    /// <summary>
    /// Authorization requirement that enforces permission-based access control.
    /// </summary>
    /// <remarks>
    /// Used by the ASP.NET Core authorization system to evaluate whether a user
    /// has a specific permission required to access a resource.
    /// </remarks>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Gets or sets the permission name that is required.
        /// </summary>
        public string Permission {  get; set; }

        /// <summary>
        /// Initializes a new instance of the PermissionRequirement class.
        /// </summary>
        /// <param name="permission">The permission name that is required.</param>
        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
