using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Models;
using InteractiveLeads.Application.Responses;
using MediatR;

namespace InteractiveLeads.Application.Feature.Tenancy.Queries
{
    /// <summary>
    /// Query for retrieving tenants in the system with pagination support.
    /// </summary>
    /// <remarks>
    /// This query implements the CQRS pattern for retrieving tenant records with pagination.
    /// </remarks>
    public sealed class GetTenantsQuery : IRequest<IResponse>
    {
        /// <summary>
        /// Gets or sets the pagination parameters for the query.
        /// </summary>
        public PaginationRequest Pagination { get; set; } = new();
    }

    /// <summary>
    /// Handler for processing GetTenantsQuery requests.
    /// </summary>
    /// <remarks>
    /// Retrieves all tenants via ITenantService and returns them as a list.
    /// </remarks>
    public sealed class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, IResponse>
    {
        private readonly ITenantService _tenantService;

        /// <summary>
        /// Initializes a new instance of the GetTenantsQueryHandler class.
        /// </summary>
        /// <param name="tenantService">The tenant service for managing tenant operations.</param>
        public GetTenantsQueryHandler(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        /// <summary>
        /// Handles the GetTenantsQuery request and retrieves tenants with pagination.
        /// </summary>
        /// <param name="request">The query request containing pagination parameters.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>A wrapped response containing a paginated list of tenants.</returns>
        public async Task<IResponse> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
        {
            return await _tenantService.GetTenantsAsync(request.Pagination, cancellationToken);
        }
    }
}
