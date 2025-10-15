using InteractiveLeads.Api.Controllers.Base;
using InteractiveLeads.Application.Feature.Identity.Tokens;
using InteractiveLeads.Application.Feature.Identity.Tokens.Queries;
using InteractiveLeads.Infrastructure.Constants;
using InteractiveLeads.Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace InteractiveLeads.Api.Controllers
{
    /// <summary>
    /// Controller for handling JWT token operations including login and token refresh.
    /// </summary>
    /// <remarks>
    /// Provides endpoints for user authentication and JWT token management.
    /// </remarks>
    public class TokenController : BaseApiController
    {
        /// <summary>
        /// Authenticates a user and returns a JWT access token and refresh token.
        /// </summary>
        /// <param name="tokenRequest">The login credentials containing username/email and password.</param>
        /// <returns>Returns Ok with JWT tokens if authentication is successful, otherwise BadRequest.</returns>
        /// <remarks>
        /// This endpoint allows anonymous access for initial login.
        /// The tenant is automatically resolved from the user's email address.
        /// </remarks>
        [HttpPost("login")]
        [AllowAnonymous]
        [OpenApiOperation("Used to obtain jwt for login")]
        public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequest tokenRequest)
        {
            var response = await Sender.Send(new GetTokenQuery { TokenRequest = tokenRequest });
            return Ok(response);
        }

        /// <summary>
        /// Generates a new JWT access token using a valid refresh token.
        /// </summary>
        /// <param name="refreshTokenRequest">The request containing the refresh token.</param>
        /// <returns>Returns Ok with new JWT tokens if the refresh token is valid, otherwise BadRequest.</returns>
        /// <remarks>
        /// Requires RefreshToken permission for the Tokens feature.
        /// This allows users to obtain a new access token without re-authenticating.
        /// </remarks>
        [HttpPost("refresh-token")]
        [OpenApiOperation("Used to generate new jwt from refresh token")]
        [ShoudHavePermission(action: InteractiveAction.RefreshToken, feature: InteractiveFeature.Tokens)]
        public async Task<IActionResult> GetRefreshTokenAsync([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            var response = await Sender.Send(new GetRefreshTokenQuery { RefreshToken = refreshTokenRequest });
            return Ok(response);
        }
    }
}
