namespace InteractiveLeads.Application.Feature.Identity.Tokens
{
    /// <summary>
    /// Service interface for handling JWT token operations.
    /// </summary>
    /// <remarks>
    /// Provides methods for user authentication and token refresh operations.
    /// </remarks>
    public interface ITokenService
    {
        /// <summary>
        /// Authenticates a user and generates JWT tokens.
        /// </summary>
        /// <param name="request">The login credentials containing username and password.</param>
        /// <returns>A task containing the token response with JWT and refresh token.</returns>
        Task<TokenResponse> LoginAsync(TokenRequest request);

        /// <summary>
        /// Refreshes an expired JWT access token using a valid refresh token.
        /// </summary>
        /// <param name="request">The refresh token request containing current tokens.</param>
        /// <returns>A task containing the token response with new JWT and refresh token.</returns>
        Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
    }
}
