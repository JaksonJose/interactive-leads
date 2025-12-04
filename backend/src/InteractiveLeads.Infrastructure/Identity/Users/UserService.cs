using Finbuckle.MultiTenant.Abstractions;
using InteractiveLeads.Application.Exceptions;
using InteractiveLeads.Application.Feature.Users;
using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Responses;
using InteractiveLeads.Infrastructure.Constants;
using InteractiveLeads.Infrastructure.Context.Application;
using InteractiveLeads.Infrastructure.Identity.Models;
using InteractiveLeads.Infrastructure.Tenancy.Models;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InteractiveLeads.Infrastructure.Identity.Users
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IMultiTenantContextAccessor<InteractiveTenantInfo> _tenantContextAccessor;

        public UserService(
            UserManager<ApplicationUser> userManager, 
            RoleManager<ApplicationRole> roleManager, 
            ApplicationDbContext context, 
            IMultiTenantContextAccessor<InteractiveTenantInfo> tenantContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _tenantContextAccessor = tenantContextAccessor;
        }

        public async Task<ResultResponse> ActivateOrDeactivateAsync(Guid userId, bool activation)
        {
            var userInDb = await GetUserAsync(userId);

            userInDb.IsActive = activation;

            var result = await _userManager.UpdateAsync(userInDb);

            if (!result.Succeeded)
            {
                var identityResponse = new ResultResponse();
                foreach (var error in IdentityHelper.GetIdentityResultErrorDescriptions(result))
                {
                    identityResponse.AddErrorMessage(error, "identity.operation_failed");
                }

                throw new IdentityException(identityResponse);
            }

            var response = new ResultResponse();
            response.AddSuccessMessage("User status updated successfully", "user.status_updated_successfully");
            return response;
        }

        public async Task<ResultResponse> AssignRolesAsync(Guid userId, UserRolesRequest request)
        {
            var userInDb = await GetUserAsync(userId);

            if (await _userManager.IsInRoleAsync(userInDb, RoleConstants.SysAdmin)
                && request.UserRoles.Any(ur => !ur.IsAssigned && ur.Name == RoleConstants.SysAdmin))
            {
                var adminUsersCount = (await _userManager.GetUsersInRoleAsync(RoleConstants.SysAdmin)).Count;
                if (userInDb.Email == TenancyConstants.Root.Email)
                {
                    if (_tenantContextAccessor.MultiTenantContext.TenantInfo?.Id == TenancyConstants.Root.Id)
                    {
                        var conflictResponse = new ResultResponse();
                        conflictResponse.AddErrorMessage("Not allowed to remove Admin role for a Root Tenant User.", "user.admin_role_removal_not_allowed");
                        throw new ConflictException(conflictResponse);
                    }
                }
                else if(adminUsersCount <= 2)
                {
                    var conflictResponse = new ResultResponse();
                    conflictResponse.AddErrorMessage("Not allowed. Tenant should have at least two Admin Users.", "user.min_admin_users_required");
                    throw new ConflictException(conflictResponse);
                }
            }

            foreach (var userRole in request.UserRoles)
            {
                if (userRole.IsAssigned)
                {
                    if (!await _userManager.IsInRoleAsync(userInDb, userRole.Name))
                    {
                        await _userManager.AddToRoleAsync(userInDb, userRole.Name);
                    }
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(userInDb, userRole.Name);
                }
            }

            var response = new ResultResponse();
            response.AddSuccessMessage("User roles updated successfully", "user.roles_updated_successfully");
            return response;
        }

        public async Task<ResultResponse> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var userInDb = await GetUserAsync(request.UserId);

            if (request.NewPassword != request.ConfirmNewPassword)
            {
                var conflictResponse = new ResultResponse();
                conflictResponse.AddErrorMessage("Passwords do not match.", "user.passwords_not_match");
                throw new ConflictException(conflictResponse);
            }

            var result = await _userManager.ChangePasswordAsync(userInDb, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                var identityResponse = new ResultResponse();
                foreach (var error in IdentityHelper.GetIdentityResultErrorDescriptions(result))
                {
                    identityResponse.AddErrorMessage(error, "identity.operation_failed");
                }
                throw new IdentityException(identityResponse);
            }

            var response = new ResultResponse();
            response.AddSuccessMessage("Password changed successfully", "user.password_changed_successfully");
            return response;
        }

        public async Task<ResultResponse> CreateAsync(CreateUserRequest request)
        {
            if (request.Password != request.ConfirmPassword)
            {
                var conflictResponse = new ResultResponse();
                conflictResponse.AddErrorMessage("Passwords do not match.", "user.passwords_not_match");
                throw new ConflictException(conflictResponse);
            }

            if (await IsEmailTakenAsync(request.Email))
            {
                var conflictResponse = new ResultResponse();
                conflictResponse.AddErrorMessage("Email already taken.", "user.email_already_taken");
                throw new ConflictException(conflictResponse);
            }

            var tenantId = _tenantContextAccessor.MultiTenantContext.TenantInfo?.Id;
            if (string.IsNullOrEmpty(tenantId))
            {
                var errorResponse = new ResultResponse();
                errorResponse.AddErrorMessage("Tenant context is not available.", "tenant.context_not_available");
                throw new ConflictException(errorResponse);
            }

            var newUser = new ApplicationUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                IsActive = request.IsActive,
                UserName = request.Email,
                EmailConfirmed = true,
                TenantId = tenantId
            };

            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                var identityResponse = new ResultResponse();
                foreach (var error in IdentityHelper.GetIdentityResultErrorDescriptions(result))
                {
                    identityResponse.AddErrorMessage(error, "identity.operation_failed");
                }
                throw new IdentityException(identityResponse);
            }

            var response = new ResultResponse();
            response.AddSuccessMessage("User created successfully", "user.created_successfully");
            return response;
        }

        public async Task<ResultResponse> DeleteAsync(Guid userId)
        {
            var userInDb = await GetUserAsync(userId);

            if (userInDb.Email == TenancyConstants.Root.Email)
            {
                var conflictResponse = new ResultResponse();
                conflictResponse.AddErrorMessage("Not allowed to remove Admin User for a Root Tenant.", "user.root_admin_deletion_not_allowed");
                throw new ConflictException(conflictResponse);
            }

            _context.Users.Remove(userInDb);
            await _context.SaveChangesAsync();

            var response = new ResultResponse();
            response.AddSuccessMessage("User deleted successfully", "user.deleted_successfully");
            return response;
        }

        public async Task<ListResponse<UserResponse>> GetAllAsync(CancellationToken ct)
        {
            var usersInDb = await _userManager.Users.ToListAsync(ct);

            var userResponses = usersInDb.Adapt<List<UserResponse>>();
            var response = new ListResponse<UserResponse>(userResponses, userResponses.Count);

            return response;
        }

        public async Task<SingleResponse<UserResponse>> GetByIdAsync(Guid userId, CancellationToken ct)
        {
            var userInDb = await GetUserAsync(userId);

            var userResponse = userInDb.Adapt<UserResponse>();
            var response = new SingleResponse<UserResponse>(userResponse);
            response.AddSuccessMessage("User retrieved successfully", "user.retrieved_successfully");
            return response;
        }

        public async Task<SingleResponse<UserResponse>> GetByEmailAsync(string email, CancellationToken ct)
        {
            var userInDb = await _userManager.FindByEmailAsync(email);
            if (userInDb == null)
            {
                var notFoundResponse = new SingleResponse<UserResponse>();
                notFoundResponse.AddErrorMessage("User not found with the provided email.", "user.not_found");
                return notFoundResponse;
            }

            var userResponse = userInDb.Adapt<UserResponse>();
            var response = new SingleResponse<UserResponse>(userResponse);
            response.AddSuccessMessage("User retrieved successfully", "user.retrieved_successfully");
            return response;
        }

        public async Task<ListResponse<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct)
        {
            var userInDb = await GetUserAsync(userId);

            var userRolesNames = await _userManager.GetRolesAsync(userInDb);

            var permissions = new List<string>();

            foreach (var role in await _roleManager
                .Roles
                .Where(r => userRolesNames.Contains(r.Name!))
                .ToListAsync(ct))
            {
                permissions.AddRange(await _context
                    .RoleClaims
                    .Where(rc => rc.RoleId == role.Id && rc.ClaimType == ClaimConstants.Permission)
                    .Select(rc => rc.ClaimValue!)
                    .ToListAsync(ct));
            }

            var distinctPermissions = permissions.Distinct().ToList();
            var response = new ListResponse<string>(distinctPermissions, distinctPermissions.Count);
            response.AddSuccessMessage("User permissions retrieved successfully", "user.permissions_retrieved_successfully");
            return response;
        }

        public async Task<ListResponse<UserRoleResponse>> GetUserRolesAsync(Guid userId, CancellationToken ct)
        {
            var userInDb = await GetUserAsync(userId);

            var userRoles = new List<UserRoleResponse>();

            var rolesInDb = await _roleManager.Roles.ToListAsync(ct);

            foreach (var role in rolesInDb)
            {
                userRoles.Add(new UserRoleResponse
                {
                    RoleId = role.Id,
                    Name = role.Name!,
                    Description = role.Description,
                    IsAssigned = await _userManager.IsInRoleAsync(userInDb, role.Name!),
                });
            }

            var response = new ListResponse<UserRoleResponse>(userRoles, userRoles.Count);
            response.AddSuccessMessage("User roles retrieved successfully", "user.roles_retrieved_successfully");
            return response;
        }

        public async Task<bool> IsEmailTakenAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) is not null;
        }

        public async Task<bool> IsPermissionAssigedAsync(Guid userId, string permission, CancellationToken ct = default)
        {
            var permissionsResponse = await GetUserPermissionsAsync(userId, ct);
            return permissionsResponse.Items.Contains(permission);
        }

        public async Task<ResultResponse> UpdateAsync(UpdateUserRequest request)
        {
            var userInDb = await GetUserAsync(request.Id);

            userInDb.FirstName = request.FirstName;
            userInDb.LastName = request.LastName;
            userInDb.PhoneNumber = request.PhoneNumber;

            var result = await _userManager.UpdateAsync(userInDb);

            if (!result.Succeeded)
            {
                var identityResponse = new ResultResponse();
                foreach (var error in IdentityHelper.GetIdentityResultErrorDescriptions(result))
                {
                    identityResponse.AddErrorMessage(error, "identity.operation_failed");
                }
                throw new IdentityException(identityResponse);
            }

            var response = new ResultResponse();
            response.AddSuccessMessage("User updated successfully", "user.updated_successfully");
            return response;
        }

        private async Task<ApplicationUser> GetUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                var notFoundResponse = new ResultResponse();
                notFoundResponse.AddErrorMessage("User does not exist.", "user.not_found");
                throw new NotFoundException(notFoundResponse);
            }
            return user;
        }
    }
}
