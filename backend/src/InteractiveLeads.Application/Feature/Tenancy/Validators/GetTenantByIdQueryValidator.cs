using FluentValidation;
using InteractiveLeads.Application.Feature.Tenancy.Queries;

namespace InteractiveLeads.Application.Feature.Tenancy.Validators
{
    /// <summary>
    /// Validator for GetTenantByIdQuery requests.
    /// </summary>
    public class GetTenantByIdQueryValidator : AbstractValidator<GetTenantByIdQuery>
    {
        /// <summary>
        /// Initializes a new instance of the GetTenantByIdQueryValidator class.
        /// </summary>
        public GetTenantByIdQueryValidator()
        {
            RuleFor(request => request.TenantId)
                .NotEmpty()
                .WithMessage("tenancy.tenant_id_required:Tenant ID is required");
        }
    }
}
