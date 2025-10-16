using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Pipelines;
using InteractiveLeads.Application.Responses;
using MediatR;

namespace InteractiveLeads.Application.Feature.Users.Queries
{
    public class GetUserPermissionsQuery : IRequest<IResponse>, IValidate
    {
        public Guid UserId { get; set; }
    }

    public class GetUserPermissionsQueryHanlder(IUserService userService)
        : IRequestHandler<GetUserPermissionsQuery, IResponse>
    {
        private readonly IUserService _userService = userService;

        public async Task<IResponse> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetUserPermissionsAsync(request.UserId, cancellationToken);
        }
    }
}
