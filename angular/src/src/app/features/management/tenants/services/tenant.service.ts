import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { TenantRepository } from '../repositories';
import { Tenant, CreateTenantRequest, UpdateSubscriptionRequest } from '../models';

@Injectable({
  providedIn: 'root'
})
export class TenantService {
  private readonly tenantRepository = inject(TenantRepository);

  /**
   * Retrieves all tenants in the system.
   */
  getAllTenants(): Observable<Tenant[]> {
    return this.tenantRepository.getAllTenants().pipe(
      map(response => response.data || [])
    );
  }

  /**
   * Retrieves a specific tenant by its identifier.
   */
  getTenantById(tenantId: string): Observable<Tenant> {
    return this.tenantRepository.getTenantById(tenantId).pipe(
      map(response => response.data!)
    );
  }

  /**
   * Creates a new tenant in the system.
   */
  createTenant(request: CreateTenantRequest): Observable<Tenant> {
    return this.tenantRepository.createTenant(request).pipe(
      map(response => response.data!)
    );
  }

  /**
   * Activates an existing tenant.
   */
  activateTenant(tenantId: string): Observable<void> {
    return this.tenantRepository.activateTenant(tenantId).pipe(
      map(() => void 0)
    );
  }

  /**
   * Deactivates an existing tenant.
   */
  deactivateTenant(tenantId: string): Observable<void> {
    return this.tenantRepository.deactivateTenant(tenantId).pipe(
      map(() => void 0)
    );
  }

  /**
   * Updates a tenant's subscription plan.
   */
  updateSubscription(request: UpdateSubscriptionRequest): Observable<void> {
    return this.tenantRepository.updateSubscription(request).pipe(
      map(() => void 0)
    );
  }

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
}
