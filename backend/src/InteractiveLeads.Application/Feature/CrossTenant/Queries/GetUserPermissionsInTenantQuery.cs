using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Responses;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace InteractiveLeads.Application.Feature.CrossTenant.Queries
{
    /// <summary>
    /// Query for retrieving user permissions in a specific tenant - available for SysAdmin and Support.
    /// </summary>
    /// <remarks>
    /// This query implements the CQRS pattern for cross-tenant user permission retrieval operations.
    /// It encapsulates the tenant context switching logic.
    /// </remarks>
    public sealed class GetUserPermissionsInTenantQuery : IRequest<IResponse>
    {
        /// <summary>
        /// Gets or sets the ID of the tenant.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the user to get permissions for.
        /// </summary>
        public Guid UserId { get; set; }
    }

    /// <summary>
    /// Handler for processing GetUserPermissionsInTenantQuery requests.
    /// </summary>
    /// <remarks>
    /// Executes the user permission retrieval operation in the specified tenant context.
    /// </remarks>
    public sealed class GetUserPermissionsInTenantQueryHandler : IRequestHandler<GetUserPermissionsInTenantQuery, IResponse>
    {
        private readonly ICrossTenantService _crossTenantService;
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the GetUserPermissionsInTenantQueryHandler class.
        /// </summary>
        /// <param name="crossTenantService">The cross-tenant service for context switching.</param>
        /// <param name="mediator">The mediator for sending internal queries.</param>
        public GetUserPermissionsInTenantQueryHandler(ICrossTenantService crossTenantService, IMediator mediator)
        {
            _crossTenantService = crossTenantService;
            _mediator = mediator;
        }

        /// <summary>
        /// Handles the GetUserPermissionsInTenantQuery request and retrieves user permissions from the tenant.
        /// </summary>
        /// <param name="request">The query request containing the tenant ID and user ID.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>A wrapped response containing the user permissions from the specified tenant.</returns>
        public async Task<IResponse> Handle(GetUserPermissionsInTenantQuery request, CancellationToken cancellationToken)
        {
            return await _crossTenantService.ExecuteInTenantContextAsync(request.TenantId,
                async (serviceProvider) => 
                {
                    // Get the UserService from the scoped service provider to ensure it uses the correct tenant context
                    var userService = serviceProvider.GetRequiredService<IUserService>();
                    return await userService.GetUserPermissionsAsync(request.UserId, cancellationToken);
                });
        }
    }
}
