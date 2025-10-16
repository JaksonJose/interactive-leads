using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Pipelines;
using InteractiveLeads.Application.Responses;
using MediatR;

namespace InteractiveLeads.Application.Feature.Identity.Roles.Commands
{
    public class UpdateRolePermissionsCommand : IRequest<IResponse>, IValidate
    {
        public UpdateRolePermissionsRequest UpdateRolePermissions { get; set; }
    }

    public class UpdateRolePermissionsCommandHandler(IRoleService roleService)
        : IRequestHandler<UpdateRolePermissionsCommand, IResponse>
    {
        private readonly IRoleService _roleService = roleService;

        public async Task<IResponse> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
        {
            return await _roleService.UpdatePermissionsAsync(request.UpdateRolePermissions, cancellationToken);
        }
    }
}
