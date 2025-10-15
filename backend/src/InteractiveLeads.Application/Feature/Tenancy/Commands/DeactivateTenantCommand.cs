using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Responses;
using MediatR;

namespace InteractiveLeads.Application.Feature.Tenancy.Commands
{
    /// <summary>
    /// Command for deactivating an existing tenant.
    /// </summary>
    /// <remarks>
    /// This command implements the CQRS pattern for tenant deactivation operations.
    /// Deactivating a tenant disables their access to the system.
    /// </remarks>
    public sealed class DeactivateTenantCommand : IRequest<IResponse>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the tenant to deactivate.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handler for processing DeactivateTenantCommand requests.
    /// </summary>
    /// <remarks>
    /// Deactivates the specified tenant via ITenantService.
    /// </remarks>
    public sealed class DeactivateTenantCommandHandler : IRequestHandler<DeactivateTenantCommand, IResponse>
    {
        private readonly ITenantService _tenantService;

        /// <summary>
        /// Initializes a new instance of the DeactivateTenantCommandHandler class.
        /// </summary>
        /// <param name="tenantService">The tenant service for managing tenant operations.</param>
        public DeactivateTenantCommandHandler(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        /// <summary>
        /// Handles the DeactivateTenantCommand request and deactivates the tenant.
        /// </summary>
        /// <param name="request">The command containing the tenant identifier.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>A wrapped response containing the deactivated tenant identifier if deactivation succeeds.</returns>
        public async Task<IResponse> Handle(DeactivateTenantCommand request, CancellationToken cancellationToken)
        {
            var tenantId = await _tenantService.DeactivateAsync(request.TenantId);

            return new SingleResponse<string>(tenantId)
                .AddSuccessMessage("Tenant deactivated successfully", "tenant.deactivated_successfully");
        }
    }
}
