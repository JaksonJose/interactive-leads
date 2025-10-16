using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Responses;
using MediatR;

namespace InteractiveLeads.Application.Feature.Users.Queries
{
    public class GetUserPermissionsQuery : IRequest<IResponse>
    {
        public Guid UserId { get; set; }
    }

    public class GetUserPermissionsQueryHanlder(IUserService userService)
        : IRequestHandler<GetUserPermissionsQuery, IResponse>
    {
        private readonly IUserService _userService = userService;

        public async Task<IResponse> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
        {
            var permissions = await _userService.GetUserPermissionsAsync(request.UserId, cancellationToken);
            return permissions;
        }
    }
}
