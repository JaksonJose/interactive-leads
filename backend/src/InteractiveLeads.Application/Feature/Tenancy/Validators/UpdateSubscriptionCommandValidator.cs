using FluentValidation;
using InteractiveLeads.Application.Feature.Tenancy.Commands;

namespace InteractiveLeads.Application.Feature.Tenancy.Validators
{
    /// <summary>
    /// Validator for UpdateSubscriptionCommand requests.
    /// </summary>
    public class UpdateSubscriptionCommandValidator : AbstractValidator<UpdateSubscriptionCommand>
    {
        /// <summary>
        /// Initializes a new instance of the UpdateSubscriptionCommandValidator class.
        /// </summary>
        public UpdateSubscriptionCommandValidator()
        {
            RuleFor(request => request.UpdateTenantSubscription)
                .NotNull()
                .WithMessage("tenancy.update_subscription_required:Subscription update data is required");

            When(request => request.UpdateTenantSubscription != null, () =>
            {
                RuleFor(request => request.UpdateTenantSubscription.TenantId)
                    .NotEmpty()
                    .WithMessage("tenancy.tenant_id_required:Tenant ID is required");

                RuleFor(request => request.UpdateTenantSubscription.NewExpirationDate)
                    .GreaterThan(DateTime.UtcNow)
                    .WithMessage("tenancy.expiration_date_future:New expiration date must be in the future");
            });
        }
    }
}
