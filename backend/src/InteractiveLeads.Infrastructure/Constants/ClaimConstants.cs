namespace InteractiveLeads.Infrastructure.Constants
{
    /// <summary>
    /// Constants for JWT claim types used throughout the application.
    /// </summary>
    /// <remarks>
    /// Defines standard claim names for tenant identification and permission management.
    /// </remarks>
    public static class ClaimConstants
    {
        /// <summary>
        /// Claim type for identifying the tenant in multi-tenant scenarios.
        /// </summary>
        public const string Tenant = "tenant";

        /// <summary>
        /// Claim type for user permissions in the authorization system.
        /// </summary>
        public const string Permission = "permission";
    }
}
