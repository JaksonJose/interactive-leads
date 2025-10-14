using InteractiveLeads.Application.Responses;
using MediatR;

namespace InteractiveLeads.Application.Feature.Tenancy.Commands
{
    /// <summary>
    /// Command for creating a new tenant in the system.
    /// </summary>
    /// <remarks>
    /// This command implements the CQRS pattern for tenant creation operations.
    /// </remarks>
    public sealed class CreateTenantCommand : IRequest<IResponse>
    {
        /// <summary>
        /// Gets or sets the tenant creation request containing tenant details.
        /// </summary>
        public CreateTenantRequest CreateTenant { get; set; } = new();
    }

    /// <summary>
    /// Handler for processing CreateTenantCommand requests.
    /// </summary>
    /// <remarks>
    /// Creates a new tenant via ITenantService and returns the tenant identifier.
    /// </remarks>
    public sealed class CreateTeanantCommandHandler : IRequestHandler<CreateTenantCommand, IResponse>
    {
        private readonly ITenantService _tenantService;

        /// <summary>
        /// Initializes a new instance of the CreateTeanantCommandHandler class.
        /// </summary>
        /// <param name="tenantService">The tenant service for managing tenant operations.</param>
        public CreateTeanantCommandHandler(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        /// <summary>
        /// Handles the CreateTenantCommand request and creates the tenant.
        /// </summary>
        /// <param name="request">The command containing tenant creation details.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>A wrapped response containing the new tenant identifier if creation succeeds.</returns>
        public async Task<IResponse> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
        {
            var tenantId = await _tenantService.CreateTenantAsync(request.CreateTenant, cancellationToken);

            return new Response<string>(tenantId)
                .AddSuccessMessage("Tenant created successfully", "tenant.created_successfully");
        }
    }
}
