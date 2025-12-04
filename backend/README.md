# Interactive Leads - Backend

## 📋 Table of Contents
- [Overview](#overview)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Design Patterns & Practices](#design-patterns--practices)
- [Multi-Tenancy Architecture](#multi-tenancy-architecture)
- [Security & Authentication](#security--authentication)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Database Management](#database-management)
- [API Documentation](#api-documentation)

---

## Overview

**Interactive Leads** is a multi-tenant SaaS platform built with **.NET 10** following **Clean Architecture** principles and **Domain-Driven Design (DDD)** patterns. The system provides a scalable, maintainable, and secure foundation for lead management with enterprise-grade features.

### Key Highlights
- 🏗️ **Clean Architecture** with clear separation of concerns
- 🔐 **Hybrid Multi-Tenancy** - Shared database for basic tier, isolated databases for premium tier
- ⚡ **CQRS Pattern** using MediatR for optimal read/write operations
- 🛡️ **Granular Permission-Based Authorization** system
- 🔑 **JWT Authentication** with refresh token support
- 🐘 **PostgreSQL** with pgvector support for AI capabilities
- 💼 **Flexible Subscription Model** - Scalable from startups to enterprise
- ✅ **FluentValidation** for comprehensive input validation
- 📝 **OpenAPI/Swagger** documentation with multi-tenant support

---

## Architecture

### Clean Architecture Layers

The project follows **Clean Architecture** principles with four distinct layers:

```
┌─────────────────────────────────────────────────────┐
│              InteractiveLeads.Api                   │
│         (Presentation Layer)                        │
│  • Controllers                                      │
│  • Middleware                                       │
│  • API Configuration                                │
└─────────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────┐
│          InteractiveLeads.Application               │
│           (Application Layer)                       │
│  • CQRS Commands & Queries                         │
│  • MediatR Handlers                                │
│  • DTOs & Request/Response Models                  │
│  • FluentValidation Validators                     │
│  • Business Logic Orchestration                    │
└─────────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────┐
│            InteractiveLeads.Domain                  │
│             (Domain Layer)                          │
│  • Domain Entities                                 │
│  • Value Objects                                   │
│  • Domain Events                                   │
│  • Domain Interfaces                               │
└─────────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────┐
│         InteractiveLeads.Infrastructure             │
│          (Infrastructure Layer)                     │
│  • EF Core DbContext (Identity only)               │
│  • Dapper Data Access                              │
│  • Multi-Tenant Implementation                     │
│  • Identity & Authentication                       │
│  • External Services Integration                   │
└─────────────────────────────────────────────────────┘
```

### Dependency Flow
```
Api → Infrastructure → Application → Domain
```
- Dependencies point **inward** (Dependency Inversion Principle)
- Domain layer has **zero dependencies**
- Infrastructure implements interfaces defined in Application/Domain

---

## Technology Stack

### Core Framework
- **.NET 10.0** - Latest .NET version with performance improvements
- **C# 13** - Modern C# features with nullable reference types enabled

### Data Access
- **Entity Framework Core 9.0** - ORM for data access
- **PostgreSQL 18** - Primary database with pgvector extension
- **Npgsql** - .NET data provider for PostgreSQL

### Architecture & Patterns
- **MediatR 13.0** - CQRS implementation and request/response pattern
- **FluentValidation 12.0** - Declarative validation rules
- **Mapster 7.4** - High-performance object mapping
- **Finbuckle.MultiTenant 9.4** - Multi-tenancy framework

### Authentication & Authorization
- **ASP.NET Core Identity** - User management and authentication
- **JWT Bearer Authentication** - Stateless token-based authentication
- **Custom Permission System** - Granular role and permission-based authorization

### API Documentation
- **NSwag** - OpenAPI/Swagger documentation with multi-tenant support
- **Swashbuckle** - Additional Swagger tooling

### DevOps & Containerization
- **Docker & Docker Compose** - Containerized development environment
- **pgvector** - Vector similarity search for AI features

---

## Design Patterns & Practices

### 1. CQRS (Command Query Responsibility Segregation)
Commands and queries are separated using **MediatR**:

```csharp
// Command Example
public sealed class CreateTenantCommand : IRequest<IResponseWrapper>
{
    public CreateTenantRequest CreateTenant { get; set; } = new();
}

public sealed class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, IResponseWrapper>
{
    private readonly ITenantService _tenantService;
    
    public async Task<IResponseWrapper> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenantId = await _tenantService.CreateTenantAsync(request.CreateTenant, cancellationToken);
        return await ResponseWrapper<string>.SuccessAsync(data: tenantId, "Tenant created successfully");
    }
}
```

**Benefits:**
- Clear separation between read and write operations
- Optimized query paths for reads
- Write operations can include complex business logic
- Easier to scale reads and writes independently

### 2. Repository Pattern
Data access is abstracted through service interfaces:

```csharp
public interface ITenantService
{
    Task<string> CreateTenantAsync(CreateTenantRequest request, CancellationToken ct);
    Task<List<TenantResponse>> GetTenantsAsync();
    Task<TenantResponse> GetTenantsByIdAsync(string id);
}
```

### 3. Dependency Injection
All services are registered through extension methods for clean configuration:

```csharp
// Application Layer Services
services.AddValidatorsFromAssembly(assembly)
        .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assembly));

// Infrastructure Services
services.AddScoped<ITenantService, TenantService>();
services.AddScoped<ITokenService, TokenService>();
```

### 4. Global Exception Handling
Centralized error handling middleware for consistent API responses:

```csharp
public class ErrorHandlingMiddleware : RequestDelegate
{
    // Maps exceptions to appropriate HTTP status codes
    // Returns consistent error response format
    // Handles: ConflictException, NotFoundException, ForbiddenException, etc.
}
```

### 5. Response Wrapper Pattern
Standardized API responses across all endpoints:

```csharp
public interface IResponseWrapper
{
    bool Succeeded { get; }
    List<string> Messages { get; }
}

public class ResponseWrapper<T> : IResponseWrapper
{
    public T Data { get; set; }
    public bool Succeeded { get; set; }
    public List<string> Messages { get; set; }
}
```

### 6. Strategy Pattern for Multi-Tenancy
Multiple tenant resolution strategies:
- **Header Strategy** - Tenant from `tenant` header
- **Claim Strategy** - Tenant from JWT claims

---

## Multi-Tenancy Architecture

### Hybrid Multi-Tenancy Strategy

The system implements a **flexible hybrid multi-tenancy approach** using **Finbuckle.MultiTenant**, supporting different isolation levels based on subscription tiers:

```
┌────────────────────────────────────────────────────────┐
│            Master Database (TenantDb)                  │
│  • Tenant Registry                                     │
│  • Tenant Metadata                                     │
│  • Subscription Tiers                                  │
│  • Connection Strings                                  │
└────────────────────────────────────────────────────────┘
                         │
         ┌───────────────┼────────────────┐
         ▼                                 ▼
┌─────────────────┐              ┌──────────────────┐
│  Shared Database│              │ Isolated Databases│
│  (Basic Tier)   │              │ (Premium Tier)    │
├─────────────────┤              ├──────────────────┤
│ • Tenant A      │              │  ┌────────────┐  │
│ • Tenant B      │              │  │ Tenant X DB│  │
│ • Tenant C      │              │  └────────────┘  │
│ (Lower cost)    │              │  ┌────────────┐  │
└─────────────────┘              │  │ Tenant Y DB│  │
                                 │  └────────────┘  │
                                 │ (Higher security)│
                                 └──────────────────┘
```

### Multi-Tenancy Tiers

#### **Shared Database (Basic Tier)**
- Multiple tenants share the same database
- Lower infrastructure costs
- Suitable for startups and small businesses
- Row-level tenant isolation via TenantId filtering
- Cost-effective scaling for many small tenants

#### **Isolated Database (Premium/Enterprise Tier)**
- Dedicated database per tenant
- Complete data isolation and security
- Custom performance tuning per tenant
- Independent backup and restore
- Regulatory compliance requirements
- Higher pricing tier justifies dedicated resources

### Why Hybrid Multi-Tenancy?

This architecture provides the **best of both worlds**:

**Business Benefits:**
- 💰 **Cost Optimization** - Shared database reduces infrastructure costs for small tenants
- 📈 **Scalable Pricing** - Clear upgrade path from basic to premium tiers
- 🎯 **Market Segmentation** - Different offerings for different customer segments
- 💼 **Enterprise Ready** - Isolated databases meet compliance and security requirements

**Technical Benefits:**
- ⚡ **Performance Flexibility** - Premium tenants get dedicated resources
- 🔒 **Security Options** - Complete isolation for regulated industries
- 🔧 **Maintenance Efficiency** - Shared database simplifies updates for many small tenants
- 📊 **Resource Optimization** - Pay for isolation only when needed

### Tenant Context Implementation

```csharp
public abstract class BaseDbContext : MultiTenantIdentityDbContext<ApplicationUser, ...>
{
    private InteractiveTenantInfo TenantInfo { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!string.IsNullOrWhiteSpace(TenantInfo?.ConnectionString)) 
        {
            optionsBuilder.UseNpgsql(TenantInfo.ConnectionString, options =>
            {
                options.EnableRetryOnFailure();
            });
        }
    }
}
```

### Tenant Resolution Flow
1. **HTTP Request** arrives with tenant identifier (header or claim)
2. **Middleware** resolves tenant from configured strategies
3. **TenantInfo** loaded from TenantDbContext (includes subscription tier)
4. **Connection String** switched dynamically based on tenant configuration:
   - **Shared**: Points to shared database (TenantId filtering applied)
   - **Isolated**: Points to tenant's dedicated database
5. **All queries** execute against appropriate database with proper isolation

### Multi-Tenant Store
```csharp
services.AddMultiTenant<InteractiveTenantInfo>()
    .WithHeaderStrategy("tenant")        // Resolve from header
    .WithClaimStrategy("tenant")         // Resolve from JWT claim
    .WithEFCoreStore<TenantDbContext, InteractiveTenantInfo>();
```

### Tenant Lifecycle Management
- **Create**: New tenant → assign to shared or isolated database → seed initial data
- **Activate/Deactivate**: Control tenant access
- **Subscription Management**: Track expiration dates and tier levels
- **Tier Upgrades**: Migrate from shared to isolated database when upgrading subscription
- **Schema Evolution**: Migrations managed per database (shared or isolated)

---

## Security & Authentication

### JWT Authentication Flow

```
┌──────┐                                              ┌────────┐
│Client│                                              │  API   │
└──┬───┘                                              └───┬────┘
   │                                                      │
   │  POST /api/token (credentials)                      │
   ├────────────────────────────────────────────────────►│
   │                                                      │
   │  ◄────────────────────────────────────────────────┤ │
   │     { accessToken, refreshToken }                   │
   │                                                      │
   │  GET /api/resource (Bearer accessToken)             │
   ├────────────────────────────────────────────────────►│
   │                                                      │
   │  ◄────────────────────────────────────────────────┤ │
   │     { data }                                         │
   │                                                      │
   │  POST /api/token/refresh (refreshToken)             │
   ├────────────────────────────────────────────────────►│
   │                                                      │
   │  ◄────────────────────────────────────────────────┤ │
   │     { accessToken, refreshToken }                   │
```

### JWT Configuration
```json
{
  "JwtSettings": {
    "SecretKey": "...",
    "Issuer": "interactive_leads",
    "Audience": "interactive_leads_app",
    "TokenExpiresInMinutes": 60,
    "RefreshExpiresInDays": 1
  }
}
```

### Token Validation
- **IssuerSigningKey**: Symmetric key validation
- **ValidateIssuer**: Ensures token from trusted source
- **ValidateAudience**: Ensures token for correct application
- **ValidateLifetime**: Automatic expiration checking
- **ClockSkew**: Zero tolerance for time differences

### Granular Permission System

The system implements a sophisticated **permission-based authorization** beyond simple role-based access:

#### Permission Structure
```csharp
public record InteractivePermission(
    string Action,        // Create, Read, Update, Delete, etc.
    string Feature,       // Tenants, Users, Roles, etc.
    string Description,
    string Group,
    bool IsBasic = false,
    bool IsRoot = false
)
{
    // Permission format: "Permission.{Feature}.{Action}"
    public string Name => $"Permission.{Feature}.{Action}";
}
```

#### Permission Levels
```csharp
// Root Permissions - System-wide tenant management
InteractivePermissions.Root = [
    "Permission.Tenants.Create",
    "Permission.Tenants.Read",
    "Permission.Tenants.Update",
    "Permission.Tenants.UpgradeSubscription"
]

// Admin Permissions - Tenant-level management
InteractivePermissions.Admin = [
    "Permission.Users.Create",
    "Permission.Users.Read",
    "Permission.Users.Update",
    "Permission.Users.Delete",
    "Permission.Roles.Read",
    "Permission.RoleClaims.Update"
    // ... excludes root permissions
]

// Basic Permissions - Standard user operations
InteractivePermissions.Basic = [
    "Permission.Tokens.RefreshToken"
]
```

#### Permission Authorization Handler
```csharp
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        PermissionRequirement requirement)
    {
        var permissions = context.User.Claims
            .Where(claim => claim.Type == ClaimConstants.Permission 
                && claim.Value == requirement.Permission);

        if (permissions.Any()) 
        {
            context.Succeed(requirement);
        }
    }
}
```

#### Dynamic Policy Registration
```csharp
services.AddAuthorization(options =>
{
    // Auto-register policies for all permissions via reflection
    foreach (var permission in InteractivePermissions.All)
    {
        options.AddPolicy(permission.Name, policy => 
            policy.RequireClaim(ClaimConstants.Permission, permission.Name));
    }
});
```

#### Usage in Controllers
```csharp
[HttpPost]
[Authorize(Policy = "Permission.Tenants.Create")]
public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand command)
{
    return Ok(await Sender.Send(command));
}
```

### Security Features
- ✅ **Hashed Passwords** via ASP.NET Core Identity
- ✅ **Token Refresh** mechanism for long-lived sessions
- ✅ **Claims-Based Identity** for flexible authorization
- ✅ **Permission Claims** stored in JWT for stateless auth
- ✅ **Tenant Isolation** enforced at database level
- ✅ **HTTPS Redirection** in production
- ✅ **CORS Configuration** for Angular frontend

---

## Project Structure

### 📁 InteractiveLeads.Api (Presentation Layer)
```
Controllers/
├── Base/
│   └── BaseApiController.cs         # Base controller with MediatR
└── TokenController.cs               # Authentication endpoints

Middleware/
└── ErrorHandlingMiddleware.cs       # Global exception handling

Program.cs                            # Application entry point & DI configuration
```

### 📁 InteractiveLeads.Application (Application Layer)
```
Feature/
├── Identity/
│   └── Tokens/
│       ├── TokenRequest.cs          # Login request model
│       ├── TokenResponse.cs         # JWT response model
│       └── ITokenService.cs         # Token service interface
│
└── Tenancy/
    ├── Commands/
    │   ├── CreateTenantCommand.cs   # Create tenant CQRS command
    │   ├── ActivateTenantCommand.cs
    │   └── DeactivateTenantCommand.cs
    │
    ├── CreateTenantRequest.cs       # DTOs
    ├── TenantResponse.cs
    ├── ITenantService.cs            # Service interface
    └── UpdateTenantSubscriptionRequest.cs

Exceptions/
├── ConflictException.cs             # 409 Conflict
├── NotFoundException.cs             # 404 Not Found
├── ForbiddenException.cs            # 403 Forbidden
├── UnauthorizedException.cs         # 401 Unauthorized
└── IdentityException.cs             # Identity-specific errors

Wrappers/
├── IResponseWrapper.cs              # Response interface
└── ResponseWrapper.cs               # Standardized API response

Startup.cs                            # Application services registration
JwtSettings.cs                        # JWT configuration model
```

### 📁 InteractiveLeads.Domain (Domain Layer)
```
# Currently minimal - ready for domain entities
# Future: Aggregate roots, value objects, domain events
```

### 📁 InteractiveLeads.Infrastructure (Infrastructure Layer)
```
Context/
├── Application/
│   ├── ApplicationDbContext.cs      # Tenant-specific DbContext
│   ├── ApplicationDbSeeder.cs       # Seed data for new tenants
│   ├── BaseDbContext.cs             # Multi-tenant base context
│   └── DbConfigurations.cs          # EF Core entity configurations
│
└── Tenancy/
    ├── TenantDbContext.cs           # Master tenant registry
    ├── TenantDbSeeder.cs            # Initialize tenant database
    └── Interfaces/
        └── ITenantDbSeeder.cs

Identity/
├── Auth/
│   ├── PermissionAuthorizationHandler.cs    # Permission evaluation
│   ├── PermissionPolicyProvider.cs          # Dynamic policy provider
│   ├── PermissionRequirement.cs             # Authorization requirement
│   └── CustomClaimsPrincipalFactory.cs      # Claims factory
│
├── Models/
│   ├── ApplicationUser.cs           # Identity user entity
│   ├── ApplicationRole.cs           # Identity role entity
│   ├── ApplicationRoleClaim.cs      # Role permissions
│   └── RefreshToken.cs              # Refresh token entity
│
├── Tokens/
│   └── TokenService.cs              # JWT generation & validation
│
└── ClaimPrincipalExtensions.cs      # Claims helper methods

Tenancy/
├── Models/
│   └── InteractiveTenantInfo.cs     # Tenant entity
└── TenantService.cs                 # Tenant management logic

Constants/
├── ClaimConstants.cs                # Claim type constants
├── PermissionConstants.cs           # Permission definitions
├── RoleConstants.cs                 # Role name constants
└── TenancyConstants.cs              # Tenant-related constants

OpenApi/
├── SwaggerGlobalAuthProcessor.cs    # Global auth in Swagger
├── SwaggerHeaderAttribute.cs        # Custom header attribute
├── SwaggerHeaderAttributeProcessor.cs
├── SwaggerSettings.cs               # Swagger configuration
└── TenantHeaderAttribute.cs         # Tenant header for Swagger

Migrations/
├── Application/                     # Tenant database migrations
└── Tenant/                          # Master tenant DB migrations

Startup.cs                           # Infrastructure services registration
```

---

## Getting Started

### Prerequisites
- **.NET 10 SDK** ([Download](https://dotnet.microsoft.com/download/dotnet/10.0))
- **Docker Desktop** (for PostgreSQL)
- **IDE**: Visual Studio 2025, JetBrains Rider, or VS Code

### Environment Setup

1. **Clone the repository**
```bash
git clone <repository-url>
cd interactive-leads
```

2. **Start PostgreSQL with Docker**
```bash
cd docker
docker-compose up -d
```

This starts PostgreSQL 18 with pgvector extension:
- **Host**: localhost
- **Port**: 5432
- **Database**: InteractiveLeads
- **User**: postgres
- **Password**: @Inter#123

3. **Update Connection String** (if needed)

Edit `backend/src/InteractiveLeads.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=InteractiveLeads;Username=postgres;Password=@Inter#123"
  }
}
```

4. **Apply Migrations**

The application automatically applies migrations on startup via `AddDatabaseInitializerAsync()`.

Alternatively, apply manually:
```bash
cd backend/src/InteractiveLeads.Api
dotnet ef database update --context TenantDbContext --project ../InteractiveLeads.Infrastructure
dotnet ef database update --context ApplicationDbContext --project ../InteractiveLeads.Infrastructure
```

5. **Run the Application**
```bash
cd backend/src/InteractiveLeads.Api
dotnet run
```

The API will be available at:
- **HTTPS**: https://localhost:7051
- **HTTP**: http://localhost:5051
- **Swagger**: https://localhost:7051/swagger

---

## Database Management

### Migration Strategy

The system uses **two separate DbContexts** with independent migrations:

#### 1. TenantDbContext (Master Database)
Stores tenant registry and metadata.

**Create Migration:**
```bash
dotnet ef migrations add MigrationName --context TenantDbContext --project src/InteractiveLeads.Infrastructure --startup-project src/InteractiveLeads.Api --output-dir Migrations/Tenant
```

**Apply Migration:**
```bash
dotnet ef database update --context TenantDbContext --project src/InteractiveLeads.Infrastructure --startup-project src/InteractiveLeads.Api
```

#### 2. ApplicationDbContext (Tenant Databases)
Each tenant has their own database with Identity tables and application data.

**Create Migration:**
```bash
dotnet ef migrations add MigrationName --context ApplicationDbContext --project src/InteractiveLeads.Infrastructure --startup-project src/InteractiveLeads.Api --output-dir Migrations/Application
```

**Apply to Tenant:**
Migrations are applied automatically when creating a new tenant through `ApplicationDbSeeder`.

### Database Seeding

#### Master Database Seeding (TenantDbSeeder)
- Creates default root admin user
- Seeds initial tenant for development
- Runs on application startup

#### Tenant Database Seeding (ApplicationDbSeeder)
- Creates tenant-specific admin user
- Seeds roles and permissions
- Sets up initial application data
- Runs when creating a new tenant

### Schema Organization

**Master Database:**
```
Multitenancy.Tenants          # Tenant registry with subscription tier info
```

**Shared Tenant Database (Basic Tier):**
```
dbo.AspNetUsers               # Identity users (with TenantId)
dbo.AspNetRoles               # Identity roles (with TenantId)
dbo.AspNetUserRoles           # User-role mapping
dbo.AspNetRoleClaims          # Role permissions
dbo.RefreshTokens             # Refresh tokens (with TenantId)
# ... application tables (all include TenantId for row-level isolation)
```

**Isolated Tenant Database (Premium Tier):**
```
dbo.AspNetUsers               # Identity users (no TenantId needed)
dbo.AspNetRoles               # Identity roles
dbo.AspNetUserRoles           # User-role mapping
dbo.AspNetRoleClaims          # Role permissions
dbo.RefreshTokens             # Refresh tokens
# ... application tables (isolated at database level)
```

---

## API Documentation

### Swagger/OpenAPI

Access interactive API documentation at: `https://localhost:7051/swagger`

#### Features:
- ✅ **JWT Authorization** - Add Bearer token via "Authorize" button
- ✅ **Tenant Header** - Custom `tenant` header support
- ✅ **Try It Out** - Execute requests directly from browser
- ✅ **Schema Documentation** - Complete request/response models
- ✅ **Response Examples** - Sample responses for all endpoints

### Authentication Endpoints

#### POST /api/token
Login and receive JWT tokens.

**Request:**
```json
{
  "email": "admin@example.com",
  "password": "password123"
}
```

**Response:**
```json
{
  "succeeded": true,
  "messages": ["Authentication successful"],
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "f2e8d7c6-5b4a-3d2c...",
    "expiresIn": 3600
  }
}
```

#### POST /api/token/refresh
Refresh expired access token.

**Request:**
```json
{
  "refreshToken": "f2e8d7c6-5b4a-3d2c..."
}
```

### Tenant Management Endpoints

#### POST /api/tenants
Create a new tenant (requires Root permission).

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "identifier": "tenant-slug",
  "name": "Company Name",
  "email": "admin@company.com",
  "firstName": "John",
  "lastName": "Doe",
  "connectionString": "Server=localhost;Port=5432;Database=Tenant_CompanyName;...",
  "isActive": true,
  "expirationDate": "2025-12-31T23:59:59Z"
}
```

#### GET /api/tenants
List all tenants (requires Root permission).

#### GET /api/tenants/{id}
Get tenant by ID (requires Root permission).

#### POST /api/tenants/{id}/activate
Activate a tenant.

#### POST /api/tenants/{id}/deactivate
Deactivate a tenant.

#### PUT /api/tenants/subscription
Update tenant subscription expiration.

### Using Multi-Tenant Endpoints

For tenant-specific operations, include the `tenant` header:

```http
GET /api/resource HTTP/1.1
Host: localhost:7051
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
tenant: tenant-slug
```

---

## Performance Considerations

### Entity Framework Core Strategy
Entity Framework Core is used across the application for:
- ASP.NET Core Identity (users, roles, claims)
- Multi-tenant store management
- Application data access
- Schema migrations and seeding

### Future Optimizations
As the application scales, performance-critical queries may be optimized using:
- **Raw SQL** for complex reporting queries
- **Compiled Queries** for frequently executed operations
- **Dapper** for high-throughput read scenarios
- **Read Replicas** for query load distribution

### Connection Management
- **Npgsql Connection Pooling** - Automatically managed connection pools
- **Per-Tenant Caching** - Connection strings cached for performance
- **Retry on Failure** - Transient error handling for resilience
- **Async Operations** - Non-blocking database calls throughout

### Multi-Tenant Performance
- **Shared Database**: Efficient for many small tenants with lower overhead
- **Isolated Database**: Dedicated resources for premium tenants requiring high performance
- **Flexible Scaling**: Mix of shared and isolated allows cost-effective growth

---

## Future Enhancements

### Planned Features
- [ ] **Domain Events** - Implement domain event system
- [ ] **Outbox Pattern** - Reliable event publishing
- [ ] **CQRS Read Models** - Optimized projections for complex queries
- [ ] **Dapper Integration** - High-performance data access for critical queries
- [ ] **Tenant Migration Tool** - Automated migration between shared and isolated databases
- [ ] **API Rate Limiting** - Per-tenant throttling based on subscription tier
- [ ] **Audit Logging** - Track all data changes
- [ ] **Real-Time Notifications** - SignalR integration
- [ ] **Background Jobs** - Hangfire for scheduled tasks
- [ ] **Distributed Caching** - Redis for performance
- [ ] **Health Checks** - Comprehensive monitoring
- [ ] **Localization** - Multi-language support (i18n prepared)

### AI-Ready Architecture
- **pgvector** extension installed for vector similarity search
- Ready for RAG (Retrieval-Augmented Generation) implementations
- Semantic search capabilities for lead matching

---

## Best Practices Demonstrated

### Code Quality
- ✅ **Nullable Reference Types** enabled across solution
- ✅ **XML Documentation** on public APIs
- ✅ **Async/Await** throughout for scalability
- ✅ **CancellationTokens** for graceful cancellation
- ✅ **Using Declarations** for proper resource disposal
- ✅ **Record Types** for immutable DTOs

### Architecture
- ✅ **Separation of Concerns** - Clear layer boundaries
- ✅ **Dependency Inversion** - Abstractions over implementations
- ✅ **Single Responsibility** - Focused classes and methods
- ✅ **Open/Closed Principle** - Extensible without modification
- ✅ **Interface Segregation** - Granular interfaces
- ✅ **Hybrid Multi-Tenancy** - Flexible isolation based on business needs

### Security
- ✅ **Defense in Depth** - Multiple security layers
- ✅ **Least Privilege** - Granular permissions
- ✅ **Secure Defaults** - Authorization required by default
- ✅ **Flexible Data Isolation** - Row-level (shared) or database-level (isolated) per tier

### Testability
- ✅ **Dependency Injection** - Easy mocking
- ✅ **Interface-Based Design** - Decoupled components
- ✅ **CQRS Handlers** - Testable in isolation
- ✅ **Validators** - Unit testable validation logic

---

## Contributing

### Coding Standards
- Follow **.NET naming conventions**
- Use **PascalCase** for public members
- Use **camelCase** for private fields with `_` prefix
- Add **XML documentation** for public APIs
- Write **descriptive commit messages**

### Development Workflow
1. Create feature branch from `master`
2. Implement feature with tests
3. Update documentation
4. Create pull request
5. Code review and approval
6. Merge to master

---

## License

This project is licensed under the MIT License - see the [LICENSE](../License) file for details.

---

## Contact

For questions or support regarding the backend architecture:
- **Email**: contact@interactiveleads.com
- **Documentation**: [API Swagger](https://localhost:7051/swagger)

---

**Built with ❤️ using .NET 10 and Clean Architecture principles**

