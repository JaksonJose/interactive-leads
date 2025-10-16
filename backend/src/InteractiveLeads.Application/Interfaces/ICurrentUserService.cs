using System.Security.Claims;

namespace InteractiveLeads.Application.Interfaces
{
    public interface ICurrentUserService
    {
        string Name { get; }
        string GetUserId();
        string GetUserEmail();
        string GetUserTenant();
        bool IsAuthenticated();
        bool IsInRole(string roleName);
        IEnumerable<Claim> GetUserClaims();
        void SetCurrentUser(ClaimsPrincipal principal);
    }
}
