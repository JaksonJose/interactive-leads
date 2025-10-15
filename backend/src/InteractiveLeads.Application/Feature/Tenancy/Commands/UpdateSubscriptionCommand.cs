using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Responses;
using MediatR;

namespace InteractiveLeads.Application.Feature.Tenancy.Commands
{
    /// <summary>
    /// Command for updating a tenant's subscription information.
    /// </summary>
    /// <remarks>
    /// This command implements the CQRS pattern for tenant subscription update operations.
    /// Typically used to extend or modify subscription expiration dates.
    /// </remarks>
    public sealed class UpdateSubscriptionCommand : IRequest<IResponse>
    {
        /// <summary>
        /// Gets or sets the subscription update request containing new subscription details.
        /// </summary>
        public UpdateTenantSubscriptionRequest UpdateTenantSubscription { get; set; } = new();
    }

    /// <summary>
    /// Handler for processing UpdateSubscriptionCommand requests.
    /// </summary>
    /// <remarks>
    /// Updates the tenant's subscription via ITenantService.
    /// </remarks>
    public sealed class UpdateTeantSubscriptionCommandHandler : IRequestHandler<UpdateSubscriptionCommand, IResponse>
    {
        /// <summary>
        /// The tenant service for managing tenant operations.
        /// </summary>
        public readonly ITenantService _tenantService;

        /// <summary>
        /// Initializes a new instance of the UpdateTeantSubscriptionCommandHandler class.
        /// </summary>
        /// <param name="tenantService">The tenant service for managing tenant operations.</param>
        public UpdateTeantSubscriptionCommandHandler(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        /// <summary>
        /// Handles the UpdateSubscriptionCommand request and updates the tenant's subscription.
        /// </summary>
        /// <param name="request">The command containing the subscription update details.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>A wrapped response containing the tenant identifier if subscription update succeeds.</returns>
        public async Task<IResponse> Handle(UpdateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            string tenantId = await _tenantService.UpdateSubscriptionAsync(request.UpdateTenantSubscription);

            return new SingleResponse<string>(tenantId)
                .AddSuccessMessage("Tenant subscription updated successfully", "tenant.subscription_updated_successfully");
        }
    }
}
