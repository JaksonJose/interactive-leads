using Application.Features.Identity.Users;
using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Responses;
using MediatR;

namespace InteractiveLeads.Application.Feature.Users.Commands
{
    public class CreateUserCommand : IRequest<IResponse>
    {
        public CreateUserRequest CreateUser { get; set; }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, IResponse>
    {
        private readonly IUserService _userService;

        public CreateUserCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var response = await _userService.CreateAsync(request.CreateUser);
            return response;
        }
    }
}
