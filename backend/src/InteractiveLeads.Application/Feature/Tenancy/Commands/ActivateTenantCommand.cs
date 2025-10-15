using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Responses;
using MediatR;

namespace InteractiveLeads.Application.Feature.Tenancy.Commands
{
    /// <summary>
    /// Command for activating an existing tenant.
    /// </summary>
    /// <remarks>
    /// This command implements the CQRS pattern for tenant activation operations.
    /// Activating a tenant enables their access to the system.
    /// </remarks>
    public sealed class ActivateTenantCommand : IRequest<IResponse>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the tenant to activate.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handler for processing ActivateTenantCommand requests.
    /// </summary>
    /// <remarks>
    /// Activates the specified tenant via ITenantService.
    /// </remarks>
    public sealed class ActivateTenantCommandHandler : IRequestHandler<ActivateTenantCommand, IResponse>
    {
        private readonly ITenantService _tenantService;

        /// <summary>
        /// Initializes a new instance of the ActivateTenantCommandHandler class.
        /// </summary>
        /// <param name="tenantService">The tenant service for managing tenant operations.</param>
        public ActivateTenantCommandHandler(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        /// <summary>
        /// Handles the ActivateTenantCommand request and activates the tenant.
        /// </summary>
        /// <param name="request">The command containing the tenant identifier.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>A wrapped response containing the activated tenant identifier if activation succeeds.</returns>
        public async Task<IResponse> Handle(ActivateTenantCommand request, CancellationToken cancellationToken)
        {
            var tenantId = await _tenantService.ActivateAsync(request.TenantId);

            return new SingleResponse<string>(tenantId)
                .AddSuccessMessage("Tenant activated successfully", "tenant.activated_successfully");
        }
    }
}
