using System.Collections.ObjectModel;

namespace InteractiveLeads.Infrastructure.Constants
{
    /// <summary>
    /// Constants defining available actions for permission-based authorization.
    /// </summary>
    /// <remarks>
    /// Used in combination with InteractiveFeature to create granular permissions.
    /// </remarks>
    public static class InteractiveAction
    {
        /// <summary>
        /// Permission action for reading/viewing resources.
        /// </summary>
        public const string Read = nameof(Read);

        /// <summary>
        /// Permission action for creating new resources.
        /// </summary>
        public const string Create = nameof(Create);

        /// <summary>
        /// Permission action for updating existing resources.
        /// </summary>
        public const string Update = nameof(Update);

        /// <summary>
        /// Permission action for deleting resources.
        /// </summary>
        public const string Delete = nameof(Delete);

        /// <summary>
        /// Permission action for refreshing authentication tokens.
        /// </summary>
        public const string RefreshToken = nameof(RefreshToken);

        /// <summary>
        /// Permission action for upgrading tenant subscriptions.
        /// </summary>
        public const string UpgradeSubscription = nameof(UpgradeSubscription);
    }

    /// <summary>
    /// Constants defining application features for permission-based authorization.
    /// </summary>
    /// <remarks>
    /// Used in combination with InteractiveAction to create specific permissions.
    /// </remarks>
    public static class InteractiveFeature
    {
        /// <summary>
        /// Feature identifier for tenant management operations.
        /// </summary>
        public const string Tenants = nameof(Tenants);

        /// <summary>
        /// Feature identifier for user management operations.
        /// </summary>
        public const string Users = nameof(Users);

        /// <summary>
        /// Feature identifier for role management operations.
        /// </summary>
        public const string Roles = nameof(Roles);

        /// <summary>
        /// Feature identifier for user-role assignment operations.
        /// </summary>
        public const string UserRoles = nameof(UserRoles);

        /// <summary>
        /// Feature identifier for role claim/permission operations.
        /// </summary>
        public const string RoleClaims = nameof(RoleClaims);

        /// <summary>
        /// Feature identifier for token/authentication operations.
        /// </summary>
        public const string Tokens = nameof(Tokens);
    }

    /// <summary>
    /// Record representing a single permission in the system.
    /// </summary>
    /// <param name="Action">The action type (e.g., Create, Read, Update, Delete).</param>
    /// <param name="Feature">The feature this permission applies to.</param>
    /// <param name="Description">Human-readable description of the permission.</param>
    /// <param name="group">The permission group for organizational purposes.</param>
    /// <param name="IsBasic">Indicates if this permission is granted to basic users.</param>
    /// <param name="IsRoot">Indicates if this permission is reserved for root administrators.</param>
    /// <remarks>
    /// Permissions are composed of an action and a feature, creating a unique permission name.
    /// </remarks>
    public record InteractivePermission(string Action, string Feature, string Description, string group, bool IsBasic = false, bool IsRoot = false)
    {
        /// <summary>
        /// Gets the fully qualified name of the permission.
        /// </summary>
        public string Name => NameFor(Action, Feature);

        /// <summary>
        /// Creates a permission name from an action and feature.
        /// </summary>
        /// <param name="action">The action type.</param>
        /// <param name="feature">The feature name.</param>
        /// <returns>A formatted permission name.</returns>
        public static string NameFor(string action, string feature) => $"Permission.{feature}.{action}";
    }

    /// <summary>
    /// Central repository of all system permissions organized by role level.
    /// </summary>
    /// <remarks>
    /// Defines the complete permission structure for the application including:
    /// - All available permissions
    /// - Root/super admin permissions
    /// - Regular admin permissions
    /// - Basic user permissions
    /// </remarks>
    public static class InteractivePermissions
    {
        private static readonly InteractivePermission[] _allPermissions = 
        [
            new InteractivePermission(InteractiveAction.Create, InteractiveFeature.Tenants, "Create Tenants","Tenancy", IsRoot: true),
            new InteractivePermission(InteractiveAction.Read, InteractiveFeature.Tenants, "Read Tenants","Tenancy", IsRoot: true),
            new InteractivePermission(InteractiveAction.Update, InteractiveFeature.Tenants, "Update Tenants","Tenancy", IsRoot: true),
            new InteractivePermission(InteractiveAction.UpgradeSubscription, InteractiveFeature.Tenants,"Tenancy", "Upgrade Tenants subscription", IsRoot: true),

            new InteractivePermission(InteractiveAction.Create, InteractiveFeature.Users, "Create Users", "SystemAccess"),
            new InteractivePermission(InteractiveAction.Read, InteractiveFeature.Users, "Read Users", "SystemAccess"),
            new InteractivePermission(InteractiveAction.Update, InteractiveFeature.Users, "Update Users", "SystemAccess"),
            new InteractivePermission(InteractiveAction.Delete, InteractiveFeature.Users, "Delete Users", "SystemAccess"),

            new InteractivePermission(InteractiveAction.Read, InteractiveFeature.UserRoles, "Read User Roles", "SystemAccess"),
            new InteractivePermission(InteractiveAction.Update, InteractiveFeature.UserRoles, "Update User Roles", "SystemAccess"),

            new InteractivePermission(InteractiveAction.Create, InteractiveFeature.Roles, "Create Roles", "SystemAccess"),
            new InteractivePermission(InteractiveAction.Read, InteractiveFeature.Roles, "Read Roles", "SystemAccess"),
            new InteractivePermission(InteractiveAction.Update, InteractiveFeature.Users, "Update Roles", "SystemAccess"),
            new InteractivePermission(InteractiveAction.Delete, InteractiveFeature.Users, "Delete Roles", "SystemAccess"),

            new InteractivePermission(InteractiveAction.Read, InteractiveFeature.RoleClaims, "Read Role claims/Permissions", "SystemAccess"),
            new InteractivePermission(InteractiveAction.Update, InteractiveFeature.RoleClaims, "Update Role claims/Permissions", "SystemAccess"),

            //new InteractivePermission(InteractiveAction.Create, InteractiveFeature.Schools, "Create Schools","Academics", IsBasic: true),
            //new InteractivePermission(InteractiveAction.Read, InteractiveFeature.Schools, "Read Schools","Academics"),
            //new InteractivePermission(InteractiveAction.Update, InteractiveFeature.Schools, "Update Schools","Academics"),
            //new InteractivePermission(InteractiveAction.Delete, InteractiveFeature.Schools, "Delete Schools","Academics"),
            
            new InteractivePermission(InteractiveAction.RefreshToken, InteractiveFeature.Tokens, "Generate Refresh Token","SystemAccess", IsBasic: true),
        ];

        /// <summary>
        /// Gets all permissions available in the system.
        /// </summary>
        public static IReadOnlyList<InteractivePermission> All { get; } = new ReadOnlyCollection<InteractivePermission>(_allPermissions);

        /// <summary>
        /// Gets root/super administrator permissions for system-wide tenant management.
        /// </summary>
        /// <remarks>
        /// Root permissions are reserved for the highest level administrators
        /// who manage the multi-tenant infrastructure.
        /// </remarks>
        public static IReadOnlyList<InteractivePermission> Root { get; } = new ReadOnlyCollection<InteractivePermission>([.. _allPermissions.Where(p => p.IsRoot)]);
        
        /// <summary>
        /// Gets administrator permissions for tenant-level management.
        /// </summary>
        /// <remarks>
        /// Admin permissions exclude root permissions and are for
        /// managing users and resources within a single tenant.
        /// </remarks>
        public static IReadOnlyList<InteractivePermission> Admin { get; } = new ReadOnlyCollection<InteractivePermission>([.. _allPermissions.Where(p => !p.IsRoot)]);
        
        /// <summary>
        /// Gets basic user permissions for standard operations.
        /// </summary>
        /// <remarks>
        /// Basic permissions provide minimal access required for regular users.
        /// </remarks>
        public static IReadOnlyList<InteractivePermission> Basic { get; } = new ReadOnlyCollection<InteractivePermission>([.. _allPermissions.Where(p => p.IsBasic)]);
    }
}
