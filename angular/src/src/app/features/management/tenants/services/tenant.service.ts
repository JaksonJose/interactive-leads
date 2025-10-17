import { Injectable } from '@angular/core';
import { Tenant } from '@feature/management/tenants/models';

@Injectable({
  providedIn: 'root'
})
export class TenantService {

  /**
   * Gets the full name of a tenant administrator.
   */
  getTenantAdminFullName(tenant: Tenant): string {
    return `${tenant.firstName} ${tenant.lastName}`.trim();
  }

  /**
   * Checks if a tenant's subscription is expired.
   */
  isTenantExpired(tenant: Tenant): boolean {
    return new Date(tenant.expirationDate) < new Date();
  }

  /**
   * Gets the status description for a tenant.
   */
  getTenantStatusDescription(tenant: Tenant): string {
    if (!tenant.isActive) {
      return 'Inactive';
    }
    
    if (this.isTenantExpired(tenant)) {
      return 'Expired';
    }
    
    return 'Active';
  }

  /**
   * Validates if a tenant can be activated.
   */
  canActivateTenant(tenant: Tenant): boolean {
    return !tenant.isActive && !this.isTenantExpired(tenant);
  }

  /**
   * Validates if a tenant can be deactivated.
   */
  canDeactivateTenant(tenant: Tenant): boolean {
    return tenant.isActive;
  }

  /**
   * Gets the days remaining until tenant expiration.
   */
  getDaysUntilExpiration(tenant: Tenant): number {
    const today = new Date();
    const expirationDate = new Date(tenant.expirationDate);
    const diffTime = expirationDate.getTime() - today.getTime();
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  }

  /**
   * Checks if a tenant is approaching expiration (within 30 days).
   */
  isApproachingExpiration(tenant: Tenant): boolean {
    return this.getDaysUntilExpiration(tenant) <= 30 && this.getDaysUntilExpiration(tenant) > 0;
  }
}
