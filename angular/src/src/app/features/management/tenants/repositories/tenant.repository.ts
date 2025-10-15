import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Response } from '@core/responses/response';
import { Tenant, CreateTenantRequest, UpdateSubscriptionRequest } from '../models';
import { environment } from '@environment/environment';

@Injectable({
  providedIn: 'root'
})
export class TenantRepository {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/${environment.apiVersion}/tenants`;

  /**
   * Retrieves all tenants in the system.
   * Requires Permission.Tenants.Read
   */
  getAllTenants(): Observable<Response<Tenant[]>> {
    return this.http.get<Response<Tenant[]>>(`${this.baseUrl}/all`);
  }

  /**
   * Retrieves a specific tenant by its identifier.
   * Requires Permission.Tenants.Read
   */
  getTenantById(tenantId: string): Observable<Response<Tenant>> {
    return this.http.get<Response<Tenant>>(`${this.baseUrl}/${tenantId}`);
  }

  /**
   * Creates a new tenant in the system.
   * Requires Permission.Tenants.Create
   */
  createTenant(request: CreateTenantRequest): Observable<Response<Tenant>> {
    return this.http.post<Response<Tenant>>(`${this.baseUrl}/add`, request);
  }

  /**
   * Activates an existing tenant.
   * Requires Permission.Tenants.Update
   */
  activateTenant(tenantId: string): Observable<Response<void>> {
    return this.http.put<Response<void>>(`${this.baseUrl}/${tenantId}/activate`, {});
  }

  /**
   * Deactivates an existing tenant.
   * Requires Permission.Tenants.Update
   */
  deactivateTenant(tenantId: string): Observable<Response<void>> {
    return this.http.put<Response<void>>(`${this.baseUrl}/${tenantId}/deactivate`, {});
  }

  /**
   * Updates a tenant's subscription plan.
   * Requires Permission.Tenants.UpgradeSubscription
   */
  updateSubscription(request: UpdateSubscriptionRequest): Observable<Response<void>> {
    return this.http.put<Response<void>>(`${this.baseUrl}/upgrade`, request);
  }
}
