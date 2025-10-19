using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace InteractiveLeads.Infrastructure.Identity
{
    /// <summary>
    /// Service implementation for accessing current user information.
    /// </summary>
    /// <remarks>
    /// Provides methods to access the currently authenticated user's identity and claims
    /// from the current HTTP context.
    /// </remarks>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the CurrentUserService class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor for accessing the current request context.</param>
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets the name of the current user.
        /// </summary>
        public string Name => _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty;

        /// <summary>
        /// Gets the unique identifier of the current user.
        /// </summary>
        /// <returns>User identifier as string.</returns>
        public string GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.GetUserId() ?? string.Empty;
        }

        /// <summary>
        /// Gets the email address of the current user.
        /// </summary>
        /// <returns>User email address.</returns>
        public string GetUserEmail()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.GetEmail() ?? string.Empty;
        }

        /// <summary>
        /// Gets the tenant identifier of the current user.
        /// </summary>
        /// <returns>Tenant identifier.</returns>
        public string GetUserTenant()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.GetTenant() ?? string.Empty;
        }

        /// <summary>
        /// Checks if the current user is authenticated.
        /// </summary>
        /// <returns>True if user is authenticated, false otherwise.</returns>
        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

        /// <summary>
        /// Checks if the current user is in the specified role.
        /// </summary>
        /// <param name="roleName">Name of the role to check.</param>
        /// <returns>True if user is in the role, false otherwise.</returns>
        public bool IsInRole(string roleName)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.IsInRole(roleName) ?? false;
        }

        /// <summary>
        /// Gets all claims for the current user.
        /// </summary>
        /// <returns>Collection of user claims.</returns>
        public IEnumerable<Claim> GetUserClaims()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Claims ?? Enumerable.Empty<Claim>();
        }

        /// <summary>
        /// Sets the current user principal.
        /// </summary>
        /// <param name="principal">Claims principal representing the current user.</param>
        public void SetCurrentUser(ClaimsPrincipal principal)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.User = principal;
            }
        }
    }
}
