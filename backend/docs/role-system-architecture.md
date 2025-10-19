# Role and Permission System - Interactive Leads

## Overview

The system implements a complete role hierarchy with granular permissions to support cross-tenant and tenant-specific operations.

## Role Structure

### Cross-Tenant Roles (System)

#### 1. SysAdmin (Level 5)
- **Access**: All tenants
- **Permissions**: 
  - Complete tenant management
  - Cross-tenant operations (CRUD)
  - System configuration
  - Logs and monitoring
- **Usage**: System administrators

#### 2. Support (Level 4)
- **Access**: All tenants (limited)
- **Permissions**:
  - Cross-tenant reading
  - User updates
  - Log viewing
- **Usage**: Customer support

### Tenant-Specific Roles

#### 3. Owner (Level 3)
- **Access**: Only their tenant
- **Permissions**:
  - Complete user management
  - Role management
  - All CRUD operations
- **Usage**: Tenant owner

#### 4. Manager (Level 2)
- **Access**: Only their tenant
- **Permissions**:
  - User reading and updating
  - User role management
  - Limited operations
- **Usage**: Tenant manager

#### 5. Agent (Level 1)
- **Access**: Only their tenant
- **Permissions**:
  - Basic reading
  - Standard operations
- **Usage**: Agent/end user

## Permission Hierarchy

```
SysAdmin (5) > Support (4) > Owner (3) > Manager (2) > Agent (1)
```

## Permission Types

### 1. System Permissions
- Tenant management
- System configuration
- Logs and monitoring

### 2. Cross-Tenant Permissions
- Access to multiple tenants
- Support operations

### 3. Tenant Permissions
- User management
- Role management
- Tenant-specific operations

## Implementation

### Main Files

1. **RoleConstants.cs**: Defines all roles and their properties
2. **PermissionConstants.cs**: Defines permissions and role mapping
3. **RoleSeeder.cs**: Automatically populates roles and permissions
4. **CrossTenantAuthorizationService.cs**: Authorization logic
5. **ApplicationDbSeeder.cs**: Database initialization integration

### Features

- ✅ Automatic role creation
- ✅ Automatic permission assignment
- ✅ Role hierarchy
- ✅ Permission validation
- ✅ Cross-tenant support
- ✅ Backward compatibility

## Usage

### Admin User Creation
```csharp
// Root tenant - creates SysAdmin
// Other tenants - creates Owner
```

### Permission Verification
```csharp
// Check if user can access tenant
await authService.CanAccessTenantAsync(userId, tenantId);

// Check specific permission
await authService.CanPerformActionInTenantAsync(userId, tenantId, permission);

// Check role level
var level = await authService.GetUserRoleLevelAsync(userId);
```

### Cross-Tenant Operations
```csharp
// Execute operation in specific tenant
await crossTenantService.ExecuteInTenantContextAsync(tenantId, operation);
```

## Migration

The system maintains compatibility with legacy roles:
- `Admin` → `Owner` (deprecated)
- `Basic` → `Agent` (deprecated)

## Security

- System roles cannot be deleted
- System permissions can only be modified by SysAdmin
- Tenant validation in all operations
- Cross-tenant operation auditing