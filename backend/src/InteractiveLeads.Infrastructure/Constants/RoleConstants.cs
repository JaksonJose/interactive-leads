using System.Collections.ObjectModel;

namespace InteractiveLeads.Infrastructure.Constants
{
    /// <summary>
    /// Constants for system role names and role management.
    /// </summary>
    /// <remarks>
    /// Defines default roles that are created automatically for each tenant
    /// and provides utilities for role validation.
    /// </remarks>
    public static class RoleConstants
    {
        /// <summary>
        /// Role name for tenant administrators with elevated permissions.
        /// </summary>
        public const string Admin = nameof(Admin);

        /// <summary>
        /// Role name for basic users with standard permissions.
        /// </summary>
        public const string Basic = nameof(Basic);

        /// <summary>
        /// Gets the list of default roles created for each tenant.
        /// </summary>
        public static IReadOnlyList<string> DefaultRoles { get; } = new ReadOnlyCollection<string>([Admin, Basic]);

        /// <summary>
        /// Determines whether the specified role name is a default system role.
        /// </summary>
        /// <param name="roleName">The role name to check.</param>
        /// <returns>True if the role is a default role, otherwise false.</returns>
        public static bool IsDefaultRole(string roleName) => DefaultRoles.Contains(roleName);
    }
}
