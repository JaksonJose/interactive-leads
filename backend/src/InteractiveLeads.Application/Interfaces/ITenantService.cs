using InteractiveLeads.Application.Feature.Tenancy;

namespace InteractiveLeads.Application.Interfaces
{
    /// <summary>
    /// Service interface for managing tenant operations in the multi-tenant system.
    /// </summary>
    /// <remarks>
    /// Provides methods for creating, activating, deactivating, and managing tenant subscriptions.
    /// </remarks>
    public interface ITenantService
    {
        /// <summary>
        /// Creates a new tenant with the provided information.
        /// </summary>
        /// <param name="createTenantRequest">The request containing tenant details.</param>
        /// <param name="ct">Cancellation token for the async operation.</param>
        /// <returns>A task containing the identifier of the newly created tenant.</returns>
        Task<string> CreateTenantAsync(CreateTenantRequest createTenantRequest, CancellationToken ct);

        /// <summary>
        /// Activates a tenant, enabling access to the system.
        /// </summary>
        /// <param name="id">The unique identifier of the tenant to activate.</param>
        /// <returns>A task containing the identifier of the activated tenant.</returns>
        Task<string> ActivateAsync(string id);

        /// <summary>
        /// Deactivates a tenant, disabling access to the system.
        /// </summary>
        /// <param name="id">The unique identifier of the tenant to deactivate.</param>
        /// <returns>A task containing the identifier of the deactivated tenant.</returns>
        Task<string> DeactivateAsync(string id);

        /// <summary>
        /// Updates the subscription information for a tenant.
        /// </summary>
        /// <param name="updateTenantSubscriptionRequest">The request containing updated subscription details.</param>
        /// <returns>A task containing the identifier of the tenant with updated subscription.</returns>
        Task<string> UpdateSubscriptionAsync(UpdateTenantSubscriptionRequest updateTenantSubscriptionRequest);

        /// <summary>
        /// Retrieves all tenants in the system.
        /// </summary>
        /// <returns>A task containing a list of all tenant responses.</returns>
        Task<List<TenantResponse>> GetTenantsAsync();

        /// <summary>
        /// Retrieves a specific tenant by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the tenant to retrieve.</param>
        /// <returns>A task containing the tenant response if found.</returns>
        Task<TenantResponse> GetTenantsByIdAsync(string id);
    }
}
