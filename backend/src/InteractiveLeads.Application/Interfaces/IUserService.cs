using InteractiveLeads.Application.Feature.Users;
using InteractiveLeads.Application.Responses;

namespace InteractiveLeads.Application.Interfaces
{
    public interface IUserService
    {
        Task<ResultResponse> CreateAsync(CreateUserRequest request);
        Task<ResultResponse> UpdateAsync(UpdateUserRequest request);
        Task<ResultResponse> DeleteAsync(Guid userId);
        Task<ResultResponse> ActivateOrDeactivateAsync(Guid userId, bool activation);
        Task<ResultResponse> ChangePasswordAsync(ChangePasswordRequest request);
        Task<ResultResponse> AssignRolesAsync(Guid userId, UserRolesRequest request);
        Task<ListResponse<UserResponse>> GetAllAsync(CancellationToken ct);
        Task<SingleResponse<UserResponse>> GetByIdAsync(Guid userId, CancellationToken ct);
        Task<ListResponse<UserRoleResponse>> GetUserRolesAsync(Guid userId, CancellationToken ct);
        Task<bool> IsEmailTakenAsync(string email);
        Task<ListResponse<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct);
        Task<bool> IsPermissionAssigedAsync(Guid userId, string permission, CancellationToken ct = default);
    }
}
