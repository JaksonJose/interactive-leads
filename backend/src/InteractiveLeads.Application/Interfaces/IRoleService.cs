using InteractiveLeads.Application.Feature.Identity.Roles;

namespace InteractiveLeads.Application.Interfaces
{
    public interface IRoleService
    {
        Task<string> CreateAsync(CreateRoleRequest request);
        Task<string> UpdateAsync(UpdateRoleRequest request);
        Task<string> DeleteAsync(Guid id);
        Task<string> UpdatePermissionsAsync(UpdateRolePermissionsRequest request);
        Task<bool> DoesItExistsAsync(string name);
        Task<List<RoleResponse>> GetAllAsync(CancellationToken ct);
        Task<RoleResponse> GetByIdAsync(Guid id, CancellationToken ct);
        Task<RoleResponse> GetRoleWithPermissionsAsync(Guid id, CancellationToken ct);
    }
}
