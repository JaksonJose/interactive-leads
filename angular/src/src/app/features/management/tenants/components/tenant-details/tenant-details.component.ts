import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';

import { TenantService } from '@feature/management/tenants/services';
import { TenantRepository } from '@feature/management/tenants/repositories';
import { Tenant } from '@feature/management/tenants/models';
import { Response } from '@core/responses/response';
import { SHARED_IMPORTS } from '@shared/shared-imports';
import { AuthService } from '@authentication/services/auth.service';

@Component({
  selector: 'app-tenant-details',
  standalone: true,
  imports: [
    CommonModule,
    TranslatePipe,
    ...SHARED_IMPORTS
  ],
  templateUrl: './tenant-details.component.html',
  styleUrls: ['./tenant-details.component.scss']
})
export class TenantDetailsComponent implements OnInit {
  private readonly tenantService = inject(TenantService);
  private readonly tenantRepository = inject(TenantRepository);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  tenant = signal<Tenant | null>(null);
  loading = signal<boolean>(false);
  messages = signal<{ severity: 'error' | 'success' | 'info' | 'warn' | 'secondary' | 'contrast'; content: string }[]>([]);

  ngOnInit(): void {
    this.loadTenant();
  }

  private loadTenant(): void {
    const tenantId = this.route.snapshot.paramMap.get('tenantId');
    
    if (!tenantId) {
      this.messages.set([{
        severity: 'error',
        content: 'tenantManagement.tenantIdRequired'
      }]);
      return;
    }

    this.loading.set(true);
    this.messages.set([]);

    this.tenantRepository.getTenantById(tenantId).subscribe({
      next: (response: Response<Tenant>) => {
        this.tenant.set(response.data!);
        this.loading.set(false);
      },
      error: () => {
        this.messages.set([{
          severity: 'error',
          content: 'tenantManagement.errorLoadingTenantDetails'
        }]);
        this.loading.set(false);
      }
    });
  }

  editTenant(): void {
    const tenant = this.tenant();
    if (tenant) {
      this.router.navigate(['/tenants', tenant.identifier, 'edit']);
    }
  }

  activateTenant(): void {
    const tenant = this.tenant();
    if (!tenant) return;

    this.tenantRepository.activateTenant(tenant.identifier).subscribe({
      next: () => {
        tenant.isActive = true;
        this.messages.set([{
          severity: 'success',
          content: 'tenantManagement.tenantActivatedSuccessfully'
        }]);
      },
      error: () => {
        this.messages.set([{
          severity: 'error',
          content: 'tenantManagement.errorActivatingTenant'
        }]);
      }
    });
  }

  deactivateTenant(): void {
    const tenant = this.tenant();
    if (!tenant) return;

    this.tenantRepository.deactivateTenant(tenant.identifier).subscribe({
      next: () => {
        tenant.isActive = false;
        this.messages.set([{
          severity: 'success',
          content: 'tenantManagement.tenantDeactivatedSuccessfully'
        }]);
      },
      error: () => {
        this.messages.set([{
          severity: 'error',
          content: 'tenantManagement.errorDeactivatingTenant'
        }]);
      }
    });
  }

  backToList(): void {
    this.router.navigate(['/tenants']);
  }

  getTenantAdminFullName(): string {
    const tenant = this.tenant();
    return tenant ? this.tenantService.getTenantAdminFullName(tenant) : '';
  }

  getTenantStatusDescription(): string {
    const tenant = this.tenant();
    return tenant ? this.tenantService.getTenantStatusDescription(tenant) : '';
  }

  isTenantExpired(): boolean {
    const tenant = this.tenant();
    return tenant ? this.tenantService.isTenantExpired(tenant) : false;
  }

  getStatusSeverity(): 'success' | 'info' | 'warn' | 'danger' {
    const tenant = this.tenant();
    if (!tenant) return 'danger';

    if (!tenant.isActive) {
      return 'danger';
    }
    
    if (this.isTenantExpired()) {
      return 'warn';
    }
    
    return 'success';
  }

  copyConnectionString(): void {
    const tenant = this.tenant();
    if (tenant?.connectionString) {
      navigator.clipboard.writeText(tenant.connectionString).then(() => {
        this.messages.set([{
          severity: 'success',
          content: 'tenantManagement.connectionStringCopied'
        }]);
      }).catch(() => {
        this.messages.set([{
          severity: 'error',
          content: 'tenantManagement.errorCopyingConnectionString'
        }]);
      });
    }
  }

  hasPermission(permissions: string[]): boolean {
    return this.authService.hasAnyPermission(permissions);
  }

  navigateToUsers(): void {
    const tenant = this.tenant();
    if (tenant) {
      this.router.navigate(['/tenants', tenant.identifier, 'users']);
    }
  }
}
