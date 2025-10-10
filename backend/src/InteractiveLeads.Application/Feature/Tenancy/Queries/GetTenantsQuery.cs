using InteractiveLeads.Application.Wrappers;
using MediatR;

namespace InteractiveLeads.Application.Feature.Tenancy.Queries
{
    /// <summary>
    /// Query for retrieving all tenants in the system.
    /// </summary>
    /// <remarks>
    /// This query implements the CQRS pattern for retrieving all tenant records.
    /// </remarks>
    public sealed class GetTenantsQuery : IRequest<IResponseWrapper>
    {
    }

    /// <summary>
    /// Handler for processing GetTenantsQuery requests.
    /// </summary>
    /// <remarks>
    /// Retrieves all tenants via ITenantService and returns them as a list.
    /// </remarks>
    public sealed class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, IResponseWrapper>
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
        /// Handles the GetTenantsQuery request and retrieves all tenants.
        /// </summary>
        /// <param name="request">The query request (contains no parameters).</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>A wrapped response containing a list of all tenants.</returns>
        public async Task<IResponseWrapper> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
        {
            var tenantInDb = await _tenantService.GetTenantsAsync();

            return await ResponseWrapper<List<TenantResponse>>.SuccessAsync(data: tenantInDb);
        }
    }
}
