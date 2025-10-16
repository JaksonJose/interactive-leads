using InteractiveLeads.Application.Feature.Identity.Roles;

namespace InteractiveLeads.Application.Interfaces
{
    /// <summary>
    /// Service interface for role management operations.
    /// </summary>
    /// <remarks>
    /// Provides methods for creating, updating, deleting and managing roles in the system.
    /// </remarks>
    public interface IRoleService
    {
        /// <summary>
        /// Creates a new role in the system.
        /// </summary>
        /// <param name="request">Role creation request data.</param>
        /// <returns>Identifier of the created role.</returns>
        Task<string> CreateAsync(CreateRoleRequest request);

        /// <summary>
        /// Updates an existing role.
        /// </summary>
        /// <param name="request">Role update request data.</param>
        /// <returns>Identifier of the updated role.</returns>
        Task<string> UpdateAsync(UpdateRoleRequest request);

        /// <summary>
        /// Deletes a role from the system.
        /// </summary>
        /// <param name="id">Unique identifier of the role to delete.</param>
        /// <returns>Identifier of the deleted role.</returns>
        Task<string> DeleteAsync(Guid id);

        /// <summary>
        /// Updates the permissions assigned to a role.
        /// </summary>
        /// <param name="request">Role permissions update request data.</param>
        /// <returns>Identifier of the role with updated permissions.</returns>
        Task<string> UpdatePermissionsAsync(UpdateRolePermissionsRequest request);

        /// <summary>
        /// Checks if a role with the specified name exists.
        /// </summary>
        /// <param name="name">Name of the role to check.</param>
        /// <returns>True if role exists, false otherwise.</returns>
        Task<bool> DoesItExistsAsync(string name);

        /// <summary>
        /// Retrieves all roles in the system.
        /// </summary>
        /// <param name="ct">Cancellation token for the async operation.</param>
        /// <returns>List of all roles.</returns>
        Task<List<RoleResponse>> GetAllAsync(CancellationToken ct);

        /// <summary>
        /// Retrieves a role by its unique identifier.
        /// </summary>
        /// <param name="id">Unique identifier of the role.</param>
        /// <param name="ct">Cancellation token for the async operation.</param>
        /// <returns>Role data if found.</returns>
        Task<RoleResponse> GetByIdAsync(Guid id, CancellationToken ct);

        /// <summary>
        /// Retrieves a role by its unique identifier including permissions.
        /// </summary>
        /// <param name="id">Unique identifier of the role.</param>
        /// <param name="ct">Cancellation token for the async operation.</param>
        /// <returns>Role data with permissions if found.</returns>
        Task<RoleResponse> GetRoleWithPermissionsAsync(Guid id, CancellationToken ct);
    }
}
