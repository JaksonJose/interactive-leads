import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';

import { TenantUserService } from '@feature/management/tenants/services';
import { TenantUserRepository } from '@feature/management/tenants/repositories';
import { TenantRepository } from '@feature/management/tenants/repositories';
import { User, Tenant } from '@feature/management/tenants/models';
import { Response } from '@core/responses/response';
import { SHARED_IMPORTS } from '@shared/shared-imports';
import { AuthService } from '@authentication/services/auth.service';

@Component({
  selector: 'app-tenant-users-list',
  standalone: true,
  imports: [
    CommonModule,
    TranslatePipe,
    ...SHARED_IMPORTS
  ],
  templateUrl: './tenant-users-list.component.html',
  styleUrls: ['./tenant-users-list.component.scss']
})
export class TenantUsersListComponent implements OnInit {
  private readonly tenantUserService = inject(TenantUserService);
  private readonly tenantUserRepository = inject(TenantUserRepository);
  private readonly tenantRepository = inject(TenantRepository);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly authService = inject(AuthService);

  users = signal<User[]>([]);
  tenant = signal<Tenant | null>(null);
  loading = signal<boolean>(false);
  messages = signal<{ severity: 'error' | 'success' | 'info' | 'warn' | 'secondary' | 'contrast'; content: string }[]>([]);
  tenantId = signal<string | null>(null);

  ngOnInit(): void {
    const tenantId = this.route.snapshot.paramMap.get('tenantId');
    if (tenantId) {
      this.tenantId.set(tenantId);
      this.loadTenant(tenantId);
      this.loadUsers(tenantId);
    } else {
      this.messages.set([{
        severity: 'error',
        content: 'userManagement.tenantIdRequired'
      }]);
    }
  }

  loadTenant(tenantId: string): void {
    this.tenantRepository.getTenantById(tenantId).subscribe({
      next: (response: Response<Tenant>) => {
        if (response.data) {
          this.tenant.set(response.data);
        }
      },
      error: () => {
        // Silently fail - tenant name is not critical
      }
    });
  }

  loadUsers(tenantId: string): void {
    this.loading.set(true);
    this.messages.set([]);

    this.tenantUserRepository.getUsersInTenant(tenantId).subscribe({
      next: (response: Response<User[]>) => {
        if (response.data) {
          this.users.set(response.data);
        }
        this.loading.set(false);
      },
      error: () => {
        this.messages.set([{
          severity: 'error',
          content: 'userManagement.errorLoadingUsers'
        }]);
        this.loading.set(false);
      }
    });
  }

  createUser(): void {
    const tenantId = this.tenantId();
    if (tenantId) {
      this.router.navigate(['/tenants', tenantId, 'users', 'create']);
    }
  }

  editUser(user: User): void {
    const tenantId = this.tenantId();
    if (tenantId) {
      this.router.navigate(['/tenants', tenantId, 'users', user.id, 'edit']);
    }
  }

  viewUserDetails(user: User): void {
    const tenantId = this.tenantId();
    if (tenantId) {
      this.router.navigate(['/tenants', tenantId, 'users', user.id]);
    }
  }

  backToTenantDetails(): void {
    const tenantId = this.tenantId();
    if (tenantId) {
      this.router.navigate(['/tenants', tenantId]);
    }
  }

  getUserFullName(user: User): string {
    return this.tenantUserService.getUserFullName(user);
  }

  getUserStatusDescription(user: User): string {
    return this.tenantUserService.getUserStatusDescription(user);
  }

  getUserStatusSeverity(user: User): 'success' | 'danger' {
    return this.tenantUserService.getUserStatusSeverity(user);
  }

  hasPermission(permissions: string[]): boolean {
    return this.authService.hasAnyPermission(permissions);
  }
}

