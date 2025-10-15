import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';

import { TenantService } from '../../services';
import { Tenant } from '../../models';
import { PRIME_NG_MODULES } from '@shared/primeng-imports';
import { HasPermissionDirective } from '@shared/directives';

@Component({
  selector: 'app-tenant-details',
  standalone: true,
  imports: [
    CommonModule,
    TranslatePipe,
    HasPermissionDirective,
    ...PRIME_NG_MODULES
  ],
  templateUrl: './tenant-details.component.html',
  styleUrls: ['./tenant-details.component.scss']
})
export class TenantDetailsComponent implements OnInit {
  private readonly tenantService = inject(TenantService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

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
        content: 'Tenant ID not provided'
      }]);
      return;
    }

    this.loading.set(true);
    this.messages.set([]);

    this.tenantService.getTenantById(tenantId).subscribe({
      next: (tenant) => {
        this.tenant.set(tenant);
        this.loading.set(false);
      },
      error: (error) => {
        this.messages.set([{
          severity: 'error',
          content: 'Error loading tenant details'
        }]);
        this.loading.set(false);
      }
    });
  }

  editTenant(): void {
    const tenant = this.tenant();
    if (tenant) {
      this.router.navigate(['/management/tenants', tenant.identifier, 'edit']);
    }
  }

  activateTenant(): void {
    const tenant = this.tenant();
    if (!tenant) return;

    this.tenantService.activateTenant(tenant.identifier).subscribe({
      next: () => {
        tenant.isActive = true;
        this.messages.set([{
          severity: 'success',
          content: 'Tenant activated successfully'
        }]);
      },
      error: () => {
        this.messages.set([{
          severity: 'error',
          content: 'Error activating tenant'
        }]);
      }
    });
  }

  deactivateTenant(): void {
    const tenant = this.tenant();
    if (!tenant) return;

    this.tenantService.deactivateTenant(tenant.identifier).subscribe({
      next: () => {
        tenant.isActive = false;
        this.messages.set([{
          severity: 'success',
          content: 'Tenant deactivated successfully'
        }]);
      },
      error: () => {
        this.messages.set([{
          severity: 'error',
          content: 'Error deactivating tenant'
        }]);
      }
    });
  }

  backToList(): void {
    this.router.navigate(['/management/tenants']);
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
}
