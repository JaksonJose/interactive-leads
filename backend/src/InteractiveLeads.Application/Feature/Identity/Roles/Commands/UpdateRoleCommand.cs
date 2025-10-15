using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Responses;
using MediatR;

namespace InteractiveLeads.Application.Feature.Identity.Roles.Commands
{
    public class UpdateRoleCommand : IRequest<IResponse>
    {
        public UpdateRoleRequest UpdateRole { get; set; }
    }

    public class UpdateRoleCommandHandler(IRoleService roleService) : IRequestHandler<UpdateRoleCommand, IResponse>
    {
        private readonly IRoleService _roleService = roleService;

        public async Task<IResponse> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            var updatedRole = await _roleService.UpdateAsync(request.UpdateRole);
            return new SingleResponse<string>(updatedRole).AddSuccessMessage(message: $"Role '{updatedRole}' updated successfully.");
        }
    }
}
