using FluentValidation;
using InteractiveLeads.Application.Feature.Users.Queries;

namespace InteractiveLeads.Application.Feature.Users.Validators
{
    /// <summary>
    /// Validator for GetUserPermissionsQuery requests.
    /// </summary>
    public class GetUserPermissionsQueryValidator : AbstractValidator<GetUserPermissionsQuery>
    {
        /// <summary>
        /// Initializes a new instance of the GetUserPermissionsQueryValidator class.
        /// </summary>
        public GetUserPermissionsQueryValidator()
        {
            RuleFor(request => request.UserId)
                .NotEqual(Guid.Empty)
                .WithMessage("users.user_id_required:User ID is required");
        }
    }
}
