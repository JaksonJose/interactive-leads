using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Pipelines;
using InteractiveLeads.Application.Responses;
using MediatR;

namespace InteractiveLeads.Application.Feature.Tenancy.Commands
{
    /// <summary>
    /// Command for updating an existing tenant in the system.
    /// </summary>
    /// <remarks>
    /// This command implements the CQRS pattern for tenant update operations.
    /// </remarks>
    public sealed class UpdateTenantCommand : IRequest<IResponse>, IValidate
    {
        /// <summary>
        /// Gets or sets the tenant update request containing tenant details.
        /// </summary>
        public UpdateTenantRequest UpdateTenant { get; set; } = new();
    }

    /// <summary>
    /// Handler for processing UpdateTenantCommand requests.
    /// </summary>
    /// <remarks>
    /// Updates an existing tenant via ITenantService and returns the result.
    /// </remarks>
    public sealed class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, IResponse>
    {
        private readonly ITenantService _tenantService;

        /// <summary>
        /// Initializes a new instance of the UpdateTenantCommandHandler class.
        /// </summary>
        /// <param name="tenantService">The tenant service for managing tenant operations.</param>
        public UpdateTenantCommandHandler(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        /// <summary>
        /// Handles the UpdateTenantCommand request and updates the tenant.
        /// </summary>
        /// <param name="request">The command containing tenant update details.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>A wrapped response containing the operation result.</returns>
        public async Task<IResponse> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
        {
            return await _tenantService.UpdateTenantAsync(request.UpdateTenant, cancellationToken);
        }
    }
}

