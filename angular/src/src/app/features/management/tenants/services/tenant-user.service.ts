import { Injectable } from '@angular/core';
import { User } from '@feature/management/tenants/models';

@Injectable({
  providedIn: 'root'
})
export class TenantUserService {

  /**
   * Gets the full name of a user.
   */
  getUserFullName(user: User): string {
    return `${user.firstName} ${user.lastName}`.trim();
  }

  /**
   * Gets the status description for a user.
   */
  getUserStatusDescription(user: User): string {
    return user.isActive 
      ? 'userManagement.statusLabels.active' 
      : 'userManagement.statusLabels.inactive';
  }

  /**
   * Gets the status severity for a user.
   */
  getUserStatusSeverity(user: User): 'success' | 'danger' {
    return user.isActive ? 'success' : 'danger';
  }
}

