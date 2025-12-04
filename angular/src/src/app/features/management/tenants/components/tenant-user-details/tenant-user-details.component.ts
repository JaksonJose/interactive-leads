import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
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
  selector: 'app-tenant-user-details',
  standalone: true,
  imports: [
    CommonModule,
    TranslatePipe,
    ...SHARED_IMPORTS
  ],
  templateUrl: './tenant-user-details.component.html',
  styleUrls: ['./tenant-user-details.component.scss']
})
export class TenantUserDetailsComponent implements OnInit {
  private readonly tenantUserService = inject(TenantUserService);
  private readonly tenantUserRepository = inject(TenantUserRepository);
  private readonly tenantRepository = inject(TenantRepository);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  user = signal<User | null>(null);
  tenant = signal<Tenant | null>(null);
  loading = signal<boolean>(false);
  messages = signal<{ severity: 'error' | 'success' | 'info' | 'warn' | 'secondary' | 'contrast'; content: string }[]>([]);
  tenantId = signal<string | null>(null);
  userId = signal<string | null>(null);

  ngOnInit(): void {
    const tenantId = this.route.snapshot.paramMap.get('tenantId');
    const userId = this.route.snapshot.paramMap.get('userId');
    
    if (tenantId) {
      this.tenantId.set(tenantId);
      this.loadTenant(tenantId);
    }
    
    if (userId) {
      this.userId.set(userId);
      if (tenantId) {
        this.loadUser(tenantId, userId);
      }
    } else {
      this.messages.set([{
        severity: 'error',
        content: 'userManagement.userIdRequired'
      }]);
    }
  }

  private loadTenant(tenantId: string): void {
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

  private loadUser(tenantId: string, userId: string): void {
    this.loading.set(true);
    this.messages.set([]);

    this.tenantUserRepository.getUserInTenant(tenantId, userId).subscribe({
      next: (response: Response<User>) => {
        if (response.data) {
          this.user.set(response.data);
        }
        this.loading.set(false);
      },
      error: () => {
        this.messages.set([{
          severity: 'error',
          content: 'userManagement.errorLoadingUserDetails'
        }]);
        this.loading.set(false);
      }
    });
  }

  editUser(): void {
    const tenantId = this.tenantId();
    const userId = this.userId();
    if (tenantId && userId) {
      this.router.navigate(['/tenants', tenantId, 'users', userId, 'edit']);
    }
  }

  backToList(): void {
    const tenantId = this.tenantId();
    if (tenantId) {
      this.router.navigate(['/tenants', tenantId, 'users']);
    } else {
      this.router.navigate(['/tenants']);
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

