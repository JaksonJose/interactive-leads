import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Response } from '@core/responses/response';
import { User, CreateUserRequest, UpdateUserRequest } from '@feature/management/tenants/models';
import { environment } from '@environment/environment';

@Injectable({
  providedIn: 'root'
})
export class TenantUserRepository {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/${environment.apiVersion}/crosstenant`;

  /**
   * Retrieves all users in a specific tenant.
   * Requires Permission.CrossTenantUsers.Read
   */
  getUsersInTenant(tenantId: string): Observable<Response<User[]>> {
    return this.http.get<Response<User[]>>(`${this.baseUrl}/tenants/${tenantId}/users`);
  }

  /**
   * Retrieves a specific user from a tenant.
   * Requires Permission.CrossTenantUsers.Read
   */
  getUserInTenant(tenantId: string, userId: string): Observable<Response<User>> {
    return this.http.get<Response<User>>(`${this.baseUrl}/tenants/${tenantId}/users/${userId}`);
  }

  /**
   * Creates a new user in a specific tenant.
   * Requires Permission.CrossTenantUsers.Create
   */
  createUserInTenant(tenantId: string, request: CreateUserRequest): Observable<Response<User>> {
    return this.http.post<Response<User>>(`${this.baseUrl}/tenants/${tenantId}/users`, request);
  }

  /**
   * Updates an existing user in a specific tenant.
   * Requires Permission.CrossTenantUsers.Update
   */
  updateUserInTenant(tenantId: string, userId: string, request: UpdateUserRequest): Observable<Response<User>> {
    return this.http.put<Response<User>>(`${this.baseUrl}/tenants/${tenantId}/users/${userId}`, request);
  }
}

