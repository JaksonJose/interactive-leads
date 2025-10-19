using Finbuckle.MultiTenant.Abstractions;
using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Exceptions;
using InteractiveLeads.Application.Models;
using InteractiveLeads.Application.Responses;
using InteractiveLeads.Infrastructure.Constants;
using InteractiveLeads.Infrastructure.Tenancy.Models;

namespace InteractiveLeads.Infrastructure.Tenancy
{
    /// <summary>
    /// Service for handling cross-tenant operations.
    /// </summary>
    /// <remarks>
    /// Provides functionality to execute operations in different tenant contexts
    /// while maintaining proper isolation and authorization.
    /// </remarks>
    public class CrossTenantService : ICrossTenantService
    {
        private readonly ICrossTenantAuthorizationService _authService;
        private readonly IMultiTenantContextAccessor<InteractiveTenantInfo> _tenantContextAccessor;
        private readonly IMultiTenantContextSetter _tenantContextSetter;
        private readonly ITenantService _tenantService;
        private readonly ICurrentUserService _currentUserService;

        /// <summary>
        /// Initializes a new instance of the CrossTenantService class.
        /// </summary>
        /// <param name="authService">The authorization service for cross-tenant operations.</param>
        /// <param name="tenantContextAccessor">The multi-tenant context accessor.</param>
        /// <param name="tenantContextSetter">The multi-tenant context setter.</param>
        /// <param name="tenantService">The tenant service for tenant operations.</param>
        /// <param name="currentUserService">The current user service.</param>
        public CrossTenantService(
            ICrossTenantAuthorizationService authService,
            IMultiTenantContextAccessor<InteractiveTenantInfo> tenantContextAccessor,
            IMultiTenantContextSetter tenantContextSetter,
            ITenantService tenantService,
            ICurrentUserService currentUserService)
        {
            _authService = authService;
            _tenantContextAccessor = tenantContextAccessor;
            _tenantContextSetter = tenantContextSetter;
            _tenantService = tenantService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Executes an operation in the context of a specific tenant.
        /// </summary>
        /// <typeparam name="T">The return type of the operation.</typeparam>
        /// <param name="tenantId">The ID of the tenant to execute the operation in.</param>
        /// <param name="operation">The operation to execute.</param>
        /// <returns>The result of the operation.</returns>
        public async Task<T> ExecuteInTenantContextAsync<T>(string tenantId, Func<Task<T>> operation)
        {
            var currentUserIdString = _currentUserService.GetUserId();
            if (!Guid.TryParse(currentUserIdString, out var currentUserId))
            {
                currentUserId = Guid.Empty;
            }
            
            // Verify if the current user can access this tenant
            if (!await _authService.CanAccessTenantAsync(currentUserId, tenantId))
            {
                throw new ForbiddenException();
            }

            // For now, we'll execute the operation directly
            // In a full implementation, you would switch tenant context here
            // This is a simplified version that maintains the current structure
            
            try
            {
                var result = await operation();
                
                // Log the cross-tenant operation for audit
                await LogCrossTenantOperationAsync(currentUserId, null, tenantId, operation.Method.Name, true);
                
                return result;
            }
            catch (Exception)
            {
                // Log the failed operation
                await LogCrossTenantOperationAsync(currentUserId, null, tenantId, operation.Method.Name, false);
                throw;
            }
        }

        /// <summary>
        /// Executes an operation in the context of a specific tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant to execute the operation in.</param>
        /// <param name="operation">The operation to execute.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteInTenantContextAsync(string tenantId, Func<Task> operation)
        {
            await ExecuteInTenantContextAsync(tenantId, async () =>
            {
                await operation();
                return true; // Dummy return value
            });
        }

        /// <summary>
        /// Logs a cross-tenant operation for audit purposes.
        /// </summary>
        /// <param name="userId">The ID of the user performing the operation.</param>
        /// <param name="originalTenantId">The original tenant context.</param>
        /// <param name="targetTenantId">The target tenant being accessed.</param>
        /// <param name="operation">The operation being performed.</param>
        /// <param name="result">The result of the operation (success/failure).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogCrossTenantOperationAsync(Guid userId, string? originalTenantId, string targetTenantId, string operation, bool result = true)
        {
            // TODO: Implement logging to database or external logging service
            // For now, we'll just use console logging
            var logMessage = $"Cross-Tenant Operation: User {userId} from tenant '{originalTenantId}' performed '{operation}' on tenant '{targetTenantId}' - Result: {(result ? "Success" : "Failure")}";
            
            // In a real implementation, you would log this to:
            // - Database audit table
            // - External logging service (Serilog, Application Insights, etc.)
            // - Event bus for real-time monitoring
            
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {logMessage}");
            
            await Task.CompletedTask; // Placeholder for async logging implementation
        }
    }
}
