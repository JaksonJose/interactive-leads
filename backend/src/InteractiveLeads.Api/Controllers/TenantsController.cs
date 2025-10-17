using InteractiveLeads.Api.Controllers.Base;
using InteractiveLeads.Application.Feature.Tenancy;
using InteractiveLeads.Application.Feature.Tenancy.Commands;
using InteractiveLeads.Application.Feature.Tenancy.Queries;
using InteractiveLeads.Application.Models;
using InteractiveLeads.Infrastructure.Constants;
using InteractiveLeads.Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace InteractiveLeads.Api.Controllers
{
    /// <summary>
    /// Controller for managing tenant operations in the multi-tenant system.
    /// </summary>
    /// <remarks>
    /// Provides endpoints for creating, activating, deactivating, and retrieving tenant information.
    /// All operations require appropriate permissions.
    /// </remarks>
    public class TenantsController : BaseApiController
    {
        /// <summary>
        /// Creates a new tenant in the system.
        /// </summary>
        /// <param name="request">The tenant creation request containing tenant details.</param>
        /// <returns>Returns Ok with the tenant response if successful, otherwise BadRequest.</returns>
        /// <remarks>
        /// Requires Create permission for the Tenants feature.
        /// </remarks>
        [HttpPost("add")]
        [ShouldHavePermission(InteractiveAction.Create, InteractiveFeature.Tenants)]
        [OpenApiOperation("Create a tenant by request")]
        public async Task<IActionResult> CreateTenantAsync([FromBody] CreateTenantRequest request)
        {
            var response = await Sender.Send(new CreateTenantCommand { CreateTenant = request });
            return Ok(response);
        }

        /// <summary>
        /// Activates an existing tenant.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the tenant to activate.</param>
        /// <returns>Returns Ok with the operation result if successful, otherwise BadRequest.</returns>
        /// <remarks>
        /// Requires Update permission for the Tenants feature.
        /// </remarks>
        [HttpPut("{tenantId}/activate")]
        [ShouldHavePermission(InteractiveAction.Update, InteractiveFeature.Tenants)]
        [OpenApiOperation("Active a tenant")]
        public async Task<IActionResult> ActivateTenantAsync(string tenantId)
        {
            var response = await Sender.Send(new ActivateTenantCommand { TenantId = tenantId });
            return Ok(response);
        }

        /// <summary>
        /// Deactivates an existing tenant.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the tenant to deactivate.</param>
        /// <returns>Returns Ok with the operation result if successful, otherwise BadRequest.</returns>
        /// <remarks>
        /// Requires Update permission for the Tenants feature.
        /// </remarks>
        [HttpPut("{tenantId}/deactivate")]
        [ShouldHavePermission(InteractiveAction.Update, InteractiveFeature.Tenants)]
        [OpenApiOperation("Deactive a tenant")]
        public async Task<IActionResult> DeactivateTenantAsync(string tenantId)
        {
            var response = await Sender.Send(new DeactivateTenantCommand { TenantId = tenantId });
            return Ok(response);
        }

        /// <summary>
        /// Upgrades a tenant's subscription plan.
        /// </summary>
        /// <param name="request">The subscription update request containing new subscription details.</param>
        /// <returns>Returns Ok with the operation result if successful, otherwise BadRequest.</returns>
        /// <remarks>
        /// Requires UpgradeSubscription permission for the Tenants feature.
        /// </remarks>
        [HttpPut("upgrade")]
        [ShouldHavePermission(InteractiveAction.UpgradeSubscription, InteractiveFeature.Tenants)]
        [OpenApiOperation("Upgrade the subscription")]
        public async Task<IActionResult> UpgradeTenantSubscriptionAsync([FromBody] UpdateTenantSubscriptionRequest request)
        {
            var response = await Sender.Send(new UpdateSubscriptionCommand { UpdateTenantSubscription = request });
            return Ok(response);
        }

        /// <summary>
        /// Retrieves a specific tenant by its identifier.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the tenant to retrieve.</param>
        /// <returns>Returns Ok with the tenant data if found, otherwise BadRequest.</returns>
        /// <remarks>
        /// Requires Read permission for the Tenants feature.
        /// </remarks>
        [HttpGet("{tenantId}")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.Tenants)]
        [OpenApiOperation("Fetch a tenant")]
        public async Task<IActionResult> GetTenantByIdAsync(string tenantId)
        {
            var response = await Sender.Send(new GetTenantByIdQuery { TenantId = tenantId });
            return Ok(response);
        }

        /// <summary>
        /// Retrieves tenants in the system with pagination support.
        /// </summary>
        /// <param name="page">Page number (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>Returns Ok with a paginated list of tenants if successful, otherwise BadRequest.</returns>
        /// <remarks>
        /// Requires Read permission for the Tenants feature.
        /// Default values: page=1, pageSize=10
        /// Maximum pageSize: 100
        /// </remarks>
        [HttpGet("all")]
        [ShouldHavePermission(InteractiveAction.Read, InteractiveFeature.Tenants)]
        [OpenApiOperation("Fetch tenants with pagination")]
        public async Task<IActionResult> GetTenantsAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var pagination = new PaginationRequest
            {
                Page = page,
                PageSize = pageSize
            };

            var response = await Sender.Send(new GetTenantsQuery { Pagination = pagination });

            return Ok(response);
        }
    }
}
