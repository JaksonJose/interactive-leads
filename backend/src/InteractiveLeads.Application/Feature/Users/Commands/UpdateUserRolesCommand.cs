using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Responses;
using MediatR;

namespace InteractiveLeads.Application.Feature.Users.Commands
{
    public class UpdateUserRolesCommand : IRequest<IResponse>
    {
        public Guid UserId { get; set; }
        public UserRolesRequest UserRolesRequest { get; set; }
    }

    public class UpdateUserRolesCommandHandler(IUserService userService) : IRequestHandler<UpdateUserRolesCommand, IResponse>
    {
        private readonly IUserService _userService = userService;

        public async Task<IResponse> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
        {
            var response = await _userService.AssignRolesAsync(request.UserId, request.UserRolesRequest);
            return response;
        }
    }
}
