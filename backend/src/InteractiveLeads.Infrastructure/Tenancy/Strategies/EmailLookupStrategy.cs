using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Strategies;
using InteractiveLeads.Infrastructure.Tenancy.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace InteractiveLeads.Infrastructure.Tenancy.Strategies
{
    /// <summary>
    /// Resolves tenant automatically from user email in login requests.
    /// Looks up the email in the Tenants table to find the matching tenant.
    /// </summary>
    public class EmailLookupStrategy : IMultiTenantStrategy
    {
        private readonly IMultiTenantStore<InteractiveTenantInfo> _tenantStore;

        public EmailLookupStrategy(IMultiTenantStore<InteractiveTenantInfo> tenantStore)
        {
            _tenantStore = tenantStore;
        }

        public async Task<string?> GetIdentifierAsync(object context)
        {
            if (context is not HttpContext httpContext)
                return null;

            // Only for login endpoint
            if (!httpContext.Request.Path.Value?.Contains("/token/login", StringComparison.OrdinalIgnoreCase) ?? true)
                return null;

            // Only for POST requests with JSON body
            if (httpContext.Request.Method != HttpMethods.Post)
                return null;

            try
            {
                // Read email from request body
                httpContext.Request.EnableBuffering();
                httpContext.Request.Body.Position = 0;

                using var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                httpContext.Request.Body.Position = 0;

                if (string.IsNullOrWhiteSpace(body))
                    return null;

                // Extract email from JSON
                using var jsonDoc = JsonDocument.Parse(body);
                string? email = null;

                if (jsonDoc.RootElement.TryGetProperty("userName", out var userNameElement))
                    email = userNameElement.GetString();
                else if (jsonDoc.RootElement.TryGetProperty("email", out var emailElement))
                    email = emailElement.GetString();

                if (string.IsNullOrWhiteSpace(email))
                    return null;

                // Find tenant by owner email
                var allTenants = await _tenantStore.GetAllAsync();
                var tenant = allTenants.FirstOrDefault(t => 
                    t.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

                return tenant?.Id;
            }
            catch
            {
                return null;
            }
        }
    }
}

