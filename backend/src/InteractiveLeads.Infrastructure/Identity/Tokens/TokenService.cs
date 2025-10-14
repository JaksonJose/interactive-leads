using Finbuckle.MultiTenant.Abstractions;
using InteractiveLeads.Application;
using InteractiveLeads.Application.Exceptions;
using InteractiveLeads.Application.Feature.Identity.Tokens;
using InteractiveLeads.Application.Responses;
using InteractiveLeads.Infrastructure.Constants;
using InteractiveLeads.Infrastructure.Identity.Models;
using InteractiveLeads.Infrastructure.Tenancy.Models;
using Microsoft.AspNetCore.Identity;
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
        private readonly JwtSettings _jwtSettings;

        public TokenService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IMultiTenantContextAccessor<InteractiveTenantInfo> multiTenantContextAccessor,
            IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _multiTenantContextAccessor = multiTenantContextAccessor;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<TokenResponse> LoginAsync(TokenRequest request)
        {
            var response = new Response();

            #region validations
            if (_multiTenantContextAccessor.MultiTenantContext.TenantInfo is null)
            {
                response.AddErrorMessage("Incorrect username or password", "auth.invalid_credentials");
                throw new UnauthorizedException(response);
            }

            if (!_multiTenantContextAccessor.MultiTenantContext.TenantInfo.IsActive)
            {
                response.AddErrorMessage("Tenant subscription is not active. Contact administrator.", "tenant.subscription_not_active");
                throw new UnauthorizedException(response);
            }

            var userInDb = await _userManager.FindByNameAsync(request.UserName);
            if (userInDb is null || !await _userManager.CheckPasswordAsync(userInDb, request.Password)) 
            {
                response.AddErrorMessage("Incorrect username or password", "auth.invalid_credentials");
                throw new UnauthorizedException(response);
            }

            if (!userInDb.IsActive)
            {
                response.AddErrorMessage("User not active. Contact administrator.", "auth.user_not_active");
                throw new UnauthorizedException(response);
            }

            if (_multiTenantContextAccessor.MultiTenantContext.TenantInfo.Id is not TenancyConstants.Root.Id)
            {
                if (_multiTenantContextAccessor.MultiTenantContext.TenantInfo.ExpirationDate < DateTime.UtcNow)
                {
                    response.AddErrorMessage("Subscription has expired. Contact administrator.", "auth.subscription_expired");
                    throw new UnauthorizedException(response);
                }
            }
            #endregion

            return await GenerateJwtTokenAndUpdateUserAsync(userInDb);
        }

        public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            Response response = new();

            var userPrincipal = GetClaimsPrincipalFromExpiringToken(request.CurrentJwt);
            var userEmail = userPrincipal.GetEmail();

            var userInDb = await _userManager.FindByEmailAsync(userEmail);
            if (userInDb is null)
            {
                response.AddErrorMessage("Authentication failed.", "auth.authentication_failed");
                throw new UnauthorizedException(response);
            }

            //if (userInDb.RefreshToken != request.CurrentRefreshToken || userInDb.RefreshTokenExpiryTime < DateTime.UtcNow)
            //{
            //    throw new UnauthorizedException(["Invalid token."]);
            //}

            return await GenerateJwtTokenAndUpdateUserAsync(userInDb);
        }

        private ClaimsPrincipal GetClaimsPrincipalFromExpiringToken(string expiringToken)
        {
            Response response = new();

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

            var refreshToken = new RefreshToken();
            refreshToken.Token = GenerateRefreshToken();
            refreshToken.ExpirationTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshExpiresInDays);

            await _userManager.UpdateAsync(user);

            return new TokenResponse
            {
                Jwt = newJwt,
                RefreshToken = refreshToken.Token,
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

                var allPermissionsForCurrentRole = await _roleManager.GetClaimsAsync(currentRole);

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
    }
}
