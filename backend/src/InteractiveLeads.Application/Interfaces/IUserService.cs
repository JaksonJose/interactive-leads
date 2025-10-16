﻿using InteractiveLeads.Application.Feature.Users;
using InteractiveLeads.Application.Responses;

namespace InteractiveLeads.Application.Interfaces
{
    /// <summary>
    /// Service interface for user management operations.
    /// </summary>
    /// <remarks>
    /// Provides methods for creating, updating, deleting and managing users in the system.
    /// </remarks>
    public interface IUserService
    {
        /// <summary>
        /// Creates a new user in the system.
        /// </summary>
        /// <param name="request">User creation request data.</param>
        /// <returns>Result of the user creation operation.</returns>
        Task<ResultResponse> CreateAsync(CreateUserRequest request);

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="request">User update request data.</param>
        /// <returns>Result of the user update operation.</returns>
        Task<ResultResponse> UpdateAsync(UpdateUserRequest request);

        /// <summary>
        /// Deletes a user from the system.
        /// </summary>
        /// <param name="userId">Unique identifier of the user to delete.</param>
        /// <returns>Result of the user deletion operation.</returns>
        Task<ResultResponse> DeleteAsync(Guid userId);

        /// <summary>
        /// Activates or deactivates a user.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <param name="activation">True to activate, false to deactivate.</param>
        /// <returns>Result of the activation operation.</returns>
        Task<ResultResponse> ActivateOrDeactivateAsync(Guid userId, bool activation);

        /// <summary>
        /// Changes a user's password.
        /// </summary>
        /// <param name="request">Password change request data.</param>
        /// <returns>Result of the password change operation.</returns>
        Task<ResultResponse> ChangePasswordAsync(ChangePasswordRequest request);

        /// <summary>
        /// Assigns roles to a user.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <param name="request">Role assignment request data.</param>
        /// <returns>Result of the role assignment operation.</returns>
        Task<ResultResponse> AssignRolesAsync(Guid userId, UserRolesRequest request);

        /// <summary>
        /// Retrieves all users in the system.
        /// </summary>
        /// <param name="ct">Cancellation token for the async operation.</param>
        /// <returns>List of all users.</returns>
        Task<ListResponse<UserResponse>> GetAllAsync(CancellationToken ct);

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <param name="ct">Cancellation token for the async operation.</param>
        /// <returns>User data if found.</returns>
        Task<SingleResponse<UserResponse>> GetByIdAsync(Guid userId, CancellationToken ct);

        /// <summary>
        /// Retrieves all roles assigned to a user.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <param name="ct">Cancellation token for the async operation.</param>
        /// <returns>List of user roles.</returns>
        Task<ListResponse<UserRoleResponse>> GetUserRolesAsync(Guid userId, CancellationToken ct);

        /// <summary>
        /// Checks if an email is already taken by another user.
        /// </summary>
        /// <param name="email">Email address to check.</param>
        /// <returns>True if email is taken, false otherwise.</returns>
        Task<bool> IsEmailTakenAsync(string email);

        /// <summary>
        /// Retrieves all permissions assigned to a user.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <param name="ct">Cancellation token for the async operation.</param>
        /// <returns>List of user permissions.</returns>
        Task<ListResponse<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct);

        /// <summary>
        /// Checks if a specific permission is assigned to a user.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <param name="permission">Permission to check.</param>
        /// <param name="ct">Cancellation token for the async operation.</param>
        /// <returns>True if permission is assigned, false otherwise.</returns>
        Task<bool> IsPermissionAssigedAsync(Guid userId, string permission, CancellationToken ct = default);
    }
}
