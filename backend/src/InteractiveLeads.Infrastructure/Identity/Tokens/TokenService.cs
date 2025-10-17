﻿using Finbuckle.MultiTenant.Abstractions;
using InteractiveLeads.Application;
using InteractiveLeads.Application.Exceptions;
using InteractiveLeads.Application.Feature.Identity.Tokens;
using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Responses;
using InteractiveLeads.Infrastructure.Constants;
using InteractiveLeads.Infrastructure.Identity.Models;
using InteractiveLeads.Infrastructure.Tenancy.Models;
using InteractiveLeads.Infrastructure.Context.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace InteractiveLeads.Infrastructure.Identity.Tokens
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMultiTenantContextAccessor<InteractiveTenantInfo> _multiTenantContextAccessor;
        private readonly ApplicationDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public TokenService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IMultiTenantContextAccessor<InteractiveTenantInfo> multiTenantContextAccessor,
            ApplicationDbContext context,
            IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _multiTenantContextAccessor = multiTenantContextAccessor;
            _context = context;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<SingleResponse<TokenResponse>> LoginAsync(TokenRequest request, CancellationToken ct = default)
        {
            ResultResponse result = new();

            #region validations
            if (_multiTenantContextAccessor.MultiTenantContext.TenantInfo is null)
            {
                result.AddErrorMessage("Incorrect username or password", "auth.invalid_credentials");
                throw new UnauthorizedException(result);
            }

            if (!_multiTenantContextAccessor.MultiTenantContext.TenantInfo.IsActive)
            {
                result.AddErrorMessage("Tenant subscription is not active. Contact administrator.", "tenant.subscription_not_active");
                throw new UnauthorizedException(result);
            }

            var userInDb = await _userManager.FindByNameAsync(request.UserName);
            if (userInDb is null || !await _userManager.CheckPasswordAsync(userInDb, request.Password)) 
            {
                result.AddErrorMessage("Incorrect username or password", "auth.invalid_credentials");
                throw new UnauthorizedException(result);
            }

            // TODO: Add TenantId validation when implementing UserTenantLookupStrategy
            // if (userInDb.TenantId != _multiTenantContextAccessor.MultiTenantContext.TenantInfo.Id)
            // {
            //     response.AddErrorMessage("Incorrect username or password", "auth.invalid_credentials");
            //     throw new UnauthorizedException(response);
            // }

            if (!userInDb.IsActive)
            {
                result.AddErrorMessage("User not active. Contact administrator.", "auth.user_not_active");
                throw new UnauthorizedException(result);
            }

            if (_multiTenantContextAccessor.MultiTenantContext.TenantInfo.Id is not TenancyConstants.Root.Id)
            {
                if (_multiTenantContextAccessor.MultiTenantContext.TenantInfo.ExpirationDate < DateTime.UtcNow)
                {
                    result.AddErrorMessage("Subscription has expired. Contact administrator.", "auth.subscription_expired");
                    throw new UnauthorizedException(result);
                }
            }
            #endregion

            var tokenResponse = await GenerateJwtTokenAndUpdateUserAsync(userInDb);
            
            var response = new SingleResponse<TokenResponse>(tokenResponse);
            response.AddSuccessMessage("Authentication successful", "auth.login_successful");
            return response;
        }

        public async Task<SingleResponse<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
        {
            ResultResponse response = new();

            var userPrincipal = GetClaimsPrincipalFromExpiringToken(request.CurrentJwt);
            var userEmail = userPrincipal.GetEmail();

            var userInDb = await _userManager.FindByEmailAsync(userEmail);
            if (userInDb is null)
            {
                response.AddErrorMessage("Authentication failed.", "auth.authentication_failed");
                throw new UnauthorizedException(response);
            }

            // Find the refresh token in the database
            var hashedRefreshToken = HashRefreshToken(request.CurrentRefreshToken);
            var refreshTokenInDb = await _context.RefreshTokens
                .Where(rt => rt.UserId == userInDb.Id && rt.Token == hashedRefreshToken && !rt.IsRevoked)
                .FirstOrDefaultAsync(ct);

            if (refreshTokenInDb is null)
            {
                response.AddErrorMessage("Invalid refresh token.", "auth.invalid_refresh_token");
                throw new UnauthorizedException(response);
            }

            // Check if the refresh token has not expired
            if (refreshTokenInDb.ExpirationTime < DateTime.UtcNow)
            {
                response.AddErrorMessage("Refresh token expired.", "auth.refresh_token_expired");
                throw new UnauthorizedException(response);
            }

            // Revoke the current refresh token (one-time use)
            refreshTokenInDb.IsRevoked = true;
            refreshTokenInDb.UpdatedAt = DateTime.UtcNow;

            var tokenResponse = await GenerateJwtTokenAndUpdateUserAsync(userInDb);
            
            var tokenResponseObject = new SingleResponse<TokenResponse>(tokenResponse);
            tokenResponseObject.AddSuccessMessage("Token refreshed successfully", "auth.token_refreshed_successfully");
            return tokenResponseObject;
        }

        private ClaimsPrincipal GetClaimsPrincipalFromExpiringToken(string expiringToken)
        {
            ResultResponse response = new();

            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role,
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(expiringToken, tokenValidationParams, out var securitytoken);

            if (securitytoken is not JwtSecurityToken jwtSecurityToken 
                || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)) 
            {
                response.AddErrorMessage("Invalid token provided. Failed to generate new token.", "auth.invalid_token");
                throw new UnauthorizedException(response);
            }

            return principal;
        }

        private async Task<TokenResponse> GenerateJwtTokenAndUpdateUserAsync(ApplicationUser user)
        {
            var newJwt = await GenerateJwtTokenAsync(user);

            // Generate new refresh token
            var refreshTokenValue = GenerateRefreshToken();
            var hashedRefreshToken = HashRefreshToken(refreshTokenValue);

            // Create new refresh token in the database
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = hashedRefreshToken,
                ExpirationTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshExpiresInDays),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            // Clean up expired refresh tokens for the user
            await CleanExpiredRefreshTokensAsync(user.Id);

            return new TokenResponse
            {
                Jwt = newJwt,
                RefreshToken = refreshTokenValue, // Return the original value (not hashed)
                RefreshTokenExpirationDate = refreshToken.ExpirationTime
            };
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            return GenerateEncryptedToken(GenerateSigningCredentials(), await GetUserClaimsAsync(user));
        }

        private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
        {
            var token = new JwtSecurityToken(
                claims: claims,
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpiresInMinutes),
               signingCredentials: signingCredentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        private SigningCredentials GenerateSigningCredentials()
        {
            byte[] secret = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
        }

        private async Task<IEnumerable<Claim>> GetUserClaimsAsync(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();
            var permissionClaims = new List<Claim>();

            foreach (var userRole in userRoles) 
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, userRole));
                var currentRole = await _roleManager.FindByNameAsync(userRole);

                var allPermissionsForCurrentRole = await _roleManager.GetClaimsAsync(currentRole!);

                permissionClaims.AddRange(allPermissionsForCurrentRole);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.Name, user.FirstName),
                new(ClaimTypes.Surname, user.LastName),
                new(ClaimConstants.Tenant, _multiTenantContextAccessor.MultiTenantContext.TenantInfo?.Id!),
                new(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
            }
            .Union(roleClaims)
            .Union(permissionClaims)
            .Union(userClaims);

            return claims;
        }

        private static string GenerateRefreshToken()
        {
            byte[] randomNumber = new byte[32];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// Generates a SHA-256 hash of the refresh token for secure storage.
        /// </summary>
        /// <param name="refreshToken">The refresh token in plain text.</param>
        /// <returns>The SHA-256 hash of the refresh token.</returns>
        private static string HashRefreshToken(string refreshToken)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// Removes all expired refresh tokens for the specified user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        private async Task CleanExpiredRefreshTokensAsync(Guid userId)
        {
            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.ExpirationTime < DateTime.UtcNow)
                .ToListAsync();

            if (expiredTokens.Any())
            {
                _context.RefreshTokens.RemoveRange(expiredTokens);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Revokes all active refresh tokens for a user (useful for logout from all devices).
        /// </summary>
        /// <param name="userId">The user ID.</param>
        public async Task<ResultResponse> RevokeUserRefreshTokensAsync(Guid userId)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpirationTime > DateTime.UtcNow)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                token.UpdatedAt = DateTime.UtcNow;
            }

            if (activeTokens.Any())
            {
                await _context.SaveChangesAsync();
            }

            var response = new ResultResponse();
            response.AddSuccessMessage("Logout successful", "auth.logout_successful");
            return response;
        }

        /// <summary>
        /// Revokes a specific refresh token for a user (useful for logout from current device).
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="refreshToken">The refresh token to revoke.</param>
        public async Task<ResultResponse> RevokeSpecificRefreshTokenAsync(Guid userId, string refreshToken)
        {
            var hashedRefreshToken = HashRefreshToken(refreshToken);
            var tokenToRevoke = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.Token == hashedRefreshToken && !rt.IsRevoked && rt.ExpirationTime > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            var response = new ResultResponse();

            if (tokenToRevoke == null)
            {
                response.AddErrorMessage("Refresh token not found or already revoked", "auth.token_not_found_or_revoked");
                return response;
            }

            tokenToRevoke.IsRevoked = true;
            tokenToRevoke.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            response.AddSuccessMessage("Device logout successful", "auth.device_logout_successful");
            return response;
        }
    }
}
