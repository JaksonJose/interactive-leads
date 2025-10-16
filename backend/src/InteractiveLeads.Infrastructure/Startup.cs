﻿using Finbuckle.MultiTenant;
using InteractiveLeads.Application;
using InteractiveLeads.Application.Interfaces;
using InteractiveLeads.Application.Responses;
using InteractiveLeads.Infrastructure.Constants;
using InteractiveLeads.Infrastructure.Context.Application;
using InteractiveLeads.Infrastructure.Context.Tenancy;
using InteractiveLeads.Infrastructure.Context.Tenancy.Interfaces;
using InteractiveLeads.Infrastructure.Identity.Auth;
using InteractiveLeads.Infrastructure.Identity.Models;
using InteractiveLeads.Infrastructure.Identity.Roles;
using InteractiveLeads.Infrastructure.Identity.Tokens;
using InteractiveLeads.Infrastructure.Identity.Users;
using InteractiveLeads.Infrastructure.OpenApi;
using InteractiveLeads.Infrastructure.Tenancy;
using InteractiveLeads.Infrastructure.Tenancy.Models;
using InteractiveLeads.Infrastructure.Tenancy.Strategies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace InteractiveLeads.Infrastructure
{
    public static class Startup
    {
        public static async Task AddDatabaseInitializerAsync(this IServiceProvider serviceProvider, CancellationToken ct = default)
        {
            using var scope = serviceProvider.CreateScope();

            await scope.ServiceProvider.GetRequiredService<ITenantDbSeeder>().InitializeDatabaseAsync(ct);
        }

        public static IServiceCollection AddInfraestructureServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<TenantDbContext>(options => options.UseNpgsql(config.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));
            
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(config.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));

            services.AddMultiTenant<InteractiveTenantInfo>()
                .WithStrategy<UserMappingLookupStrategy>(ServiceLifetime.Scoped)
                .WithHeaderStrategy("tenant")
                .WithClaimStrategy("tenant")
                .WithEFCoreStore<TenantDbContext, InteractiveTenantInfo>();

            services.AddIdentityService();
            services.AddPermissions();

            services.AddTransient<ITenantDbSeeder, TenantDbSeeder>();
            services.AddTransient<ApplicationDbSeeder>();

            // Register application services
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserService, UserService>();

            services.AddOpenApiDocumentation(config);

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, JwtSettings jwtSettings)
        {
            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(bearer =>
            {
                bearer.RequireHttpsMetadata = false;
                bearer.SaveToken = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                };

                bearer.Events = new()
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenException)
                        {
                            if (!context.Response.HasStarted)
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                context.Response.ContentType = "application/json";

                                var response = new ResultResponse().AddErrorMessage("Token has expired", "auth.token_expired");
                                var jsonOptions = new JsonSerializerOptions
                                {
                                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                };
                                var result = JsonSerializer.Serialize(response, jsonOptions);
                                return context.Response.WriteAsync(result);
                            }

                            return Task.CompletedTask;
                        }
                        else
                        {
                            if (!context.Response.HasStarted)
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                context.Response.ContentType = "application/json";

                                var response = new ResultResponse().AddErrorMessage("An unhandled error has occurred", "general.something_went_wrong");
                                var jsonOptions = new JsonSerializerOptions
                                {
                                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                };
                                var result = JsonSerializer.Serialize(response, jsonOptions);
                                return context.Response.WriteAsync(result);
                            }

                            return Task.CompletedTask;
                        }
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        if (!context.Response.HasStarted)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.Response.ContentType = "application/json";

                            var response = new ResultResponse().AddErrorMessage("You are not authorized", "general.unauthorized");
                            var jsonOptions = new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            };
                            var result = JsonSerializer.Serialize(response, jsonOptions);
                            return context.Response.WriteAsync(result);
                        }

                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        context.Response.ContentType = "application/json";

                        var response = new ResultResponse().AddErrorMessage("You are not authorized to access this resource", "general.access_denied");
                        var jsonOptions = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };
                        var result = JsonSerializer.Serialize(response, jsonOptions);
                        return context.Response.WriteAsync(result);
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                foreach (var prop in typeof(InteractivePermissions).GetNestedTypes()
                    .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)))
                {
                    var propertyValue = prop.GetValue(null);
                    if (propertyValue is not null)
                    {
                        options.AddPolicy(propertyValue.ToString()!, policy => policy
                            .RequireClaim(ClaimConstants.Permission, propertyValue.ToString()!));
                    }
                }
            });

            return services;
        }

        public static JwtSettings GetJwtSettings(this IServiceCollection services, IConfiguration config)
        {
            var jwtSettings = config.GetSection(nameof(JwtSettings));
            services.Configure<JwtSettings>(jwtSettings);

            return jwtSettings.Get<JwtSettings>() ?? new();
        }

        public static IApplicationBuilder UseInfraestructure(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseMultiTenant();
            app.UseAuthorization();
            app.UseOpenApiDocumentation();
            
            return app;
        }

        internal static IServiceCollection AddIdentityService(this IServiceCollection services)
        {
            return services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            }).AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .Services;
        }

        internal static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services, IConfiguration configuration)
        {
            var swaggerSettings = configuration.GetSection(nameof(SwaggerSettings)).Get<SwaggerSettings>();

            services.AddEndpointsApiExplorer();
            _ = services.AddOpenApiDocument((document, serviceProvider) =>
            {
                document.PostProcess = doc =>
                {
                    doc.Info.Title = swaggerSettings?.Title ?? string.Empty;
                    doc.Info.Description = swaggerSettings?.Description ?? string.Empty;
                    doc.Info.Contact = new OpenApiContact
                    {
                        Name = swaggerSettings?.ContactName ?? string.Empty,
                        Email = swaggerSettings?.ContactEmail ?? string.Empty,
                        Url = swaggerSettings?.ContactUrl ?? string.Empty,
                    };
                    doc.Info.License = new OpenApiLicense
                    {
                        Name = swaggerSettings?.LicenseName ?? string.Empty,
                        Url = swaggerSettings?.LicenseUrl ?? string.Empty,
                    };
                };

                document.AddSecurity(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter your Bearer token to attach it as a header on your requests.",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Type = OpenApiSecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT",
                });

                document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor());
                document.OperationProcessors.Add(new SwaggerGlobalAuthProcessor());
                document.OperationProcessors.Add(new SwaggerHeaderAttributeProcessor());
            });

            return services;
        }

        internal static IServiceCollection AddPermissions(this IServiceCollection services)
        {
            return services
                .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
                .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        }

        internal static IApplicationBuilder UseOpenApiDocumentation(this IApplicationBuilder app)
        {
            app.UseOpenApi();
            app.UseSwaggerUi(options =>
            {
                options.DefaultModelExpandDepth = -1;
                options.DocExpansion = "none";
                options.TagsSorter = "alpha";
            });

            return app;
        }
    }
}
