using InteractiveLeads.Application.Exceptions;
using InteractiveLeads.Application.Responses;
using MediatR;

namespace InteractiveLeads.Application.Feature.Tenancy.Queries
{
    /// <summary>
    /// Query for retrieving a specific tenant by its identifier.
    /// </summary>
    /// <remarks>
    /// This query implements the CQRS pattern for tenant retrieval operations.
    /// </remarks>
    public sealed class GetTenantByIdQuery : IRequest<IResponse>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the tenant to retrieve.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handler for processing GetTenantByIdQuery requests.
    /// </summary>
    /// <remarks>
    /// Retrieves the specified tenant via ITenantService and validates its existence.
    /// </remarks>
    public sealed class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, IResponse>
    { 
        private readonly ITenantService _tenantService;

        /// <summary>
        /// Initializes a new instance of the GetTenantByIdQueryHandler class.
        /// </summary>
        /// <param name="tenantService">The tenant service for managing tenant operations.</param>
        public GetTenantByIdQueryHandler(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        /// <summary>
        /// Handles the GetTenantByIdQuery request and retrieves the tenant.
        /// </summary>
        /// <param name="request">The query containing the tenant identifier.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>A wrapped response containing the tenant data if found, otherwise a failure response.</returns>
        public async Task<IResponse> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken) 
        {
            var tenantInDb = await _tenantService.GetTenantsByIdAsync(request.TenantId);
            if (tenantInDb is not null)
            {
                var response = new Response<TenantResponse>(tenantInDb);
                response.AddSuccessMessage("Tenant retrieved successfully", "tenant.retrieved_successfully");
                return (IResponse)response;
            }

            var errorResponse = new Response();
            errorResponse.AddErrorMessage("Tenant does not exist", "tenant.not_found");
            throw new NotFoundException(errorResponse);
        }
    }
}
