using InteractiveLeads.Api.Controllers.Base;
using InteractiveLeads.Application.Feature.Tenancy;
using InteractiveLeads.Application.Feature.Tenancy.Commands;
using InteractiveLeads.Application.Feature.Tenancy.Queries;
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
        [ShoudHavePermission(feature: InteractiveFeature.Tenants, action: InteractiveAction.Create)]
        [OpenApiOperation("Create a tenant by request")]
        public async Task<IActionResult> CreateTenantAsync([FromBody] CreateTenantRequest request)
        {
            var response = await Sender.Send(new CreateTenantCommand { CreateTenant = request });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
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
        [ShoudHavePermission(feature: InteractiveFeature.Tenants, action: InteractiveAction.Update)]
        [OpenApiOperation("Active a tenant")]
        public async Task<IActionResult> ActivateTenantAsync(string tenantId)
        {
            var response = await Sender.Send(new ActivateTenantCommand { TenantId = tenantId });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
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
        [ShoudHavePermission(feature: InteractiveFeature.Tenants, action: InteractiveAction.Update)]
        [OpenApiOperation("Deactive a tenant")]
        public async Task<IActionResult> DeactivateTenantAsync(string tenantId)
        {
            var response = await Sender.Send(new DeactivateTenantCommand { TenantId = tenantId });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
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
        [ShoudHavePermission(feature: InteractiveFeature.Tenants, action: InteractiveAction.UpgradeSubscription)]
        [OpenApiOperation("Upgrade the subscription")]
        public async Task<IActionResult> UpgradeTenantSubscriptionAsync([FromBody] UpdateTenantSubscriptionRequest request)
        {
            var response = await Sender.Send(new UpdateSubscriptionCommand { UpdateTenantSubscription = request });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
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
        [ShoudHavePermission(feature: InteractiveFeature.Tenants, action: InteractiveAction.Read)]
        [OpenApiOperation("Fetch a tenant")]
        public async Task<IActionResult> GetTenantByIdAsync(string tenantId)
        {
            var response = await Sender.Send(new GetTenantByIdQuery { TenantId = tenantId });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        /// <summary>
        /// Retrieves all tenants in the system.
        /// </summary>
        /// <returns>Returns Ok with a list of all tenants if successful, otherwise BadRequest.</returns>
        /// <remarks>
        /// Requires Read permission for the Tenants feature.
        /// </remarks>
        [HttpGet("all")]
        [ShoudHavePermission(feature: InteractiveFeature.Tenants, action: InteractiveAction.Read)]
        [OpenApiOperation("Fetch all tenants")]
        public async Task<IActionResult> GetTenantsAsync()
        {
            var response = await Sender.Send(new GetTenantsQuery());
            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}
