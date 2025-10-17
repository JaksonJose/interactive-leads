import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';

import { TenantService } from '../../services';
import { TenantRepository } from '../../repositories';
import { Tenant } from '../../models';
import { Response } from '@core/responses/response';
import { SHARED_IMPORTS } from '@shared/shared-imports';
import { AuthService } from '@authentication/services/auth.service';

@Component({
  selector: 'app-tenant-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslatePipe,
    ...SHARED_IMPORTS
  ],
  templateUrl: './tenant-list.component.html',
  styleUrls: ['./tenant-list.component.scss']
})
export class TenantListComponent implements OnInit {
  private readonly tenantService = inject(TenantService);
  private readonly tenantRepository = inject(TenantRepository);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  tenants = signal<Tenant[]>([]);
  loading = signal<boolean>(false);
  messages = signal<{ severity: 'error' | 'success' | 'info' | 'warn' | 'secondary' | 'contrast'; content: string }[]>([]);

  ngOnInit(): void {
    this.loadTenants();
  }

  loadTenants(): void {
    this.loading.set(true);
    this.messages.set([]);

    this.tenantRepository.getAllTenants().subscribe({
      next: (response: Response<Tenant[]>) => {
        this.tenants.set(response.data || []);
        this.loading.set(false);
      },
      error: () => {
        this.messages.set([{
          severity: 'error',
          content: 'Error loading tenants'
        }]);
        this.loading.set(false);
      }
    });
  }

  createTenant(): void {
    this.router.navigate(['/management/tenants/create']);
  }

  viewTenantDetails(tenant: Tenant): void {
    this.router.navigate(['/management/tenants', tenant.identifier]);
  }

  editTenant(tenant: Tenant): void {
    this.router.navigate(['/management/tenants', tenant.identifier, 'edit']);
  }

  activateTenant(tenant: Tenant): void {
    this.tenantRepository.activateTenant(tenant.identifier).subscribe({
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

  deactivateTenant(tenant: Tenant): void {
    this.tenantRepository.deactivateTenant(tenant.identifier).subscribe({
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

  getTenantAdminFullName(tenant: Tenant): string {
    return this.tenantService.getTenantAdminFullName(tenant);
  }

  getTenantStatusDescription(tenant: Tenant): string {
    return this.tenantService.getTenantStatusDescription(tenant);
  }

  isTenantExpired(tenant: Tenant): boolean {
    return this.tenantService.isTenantExpired(tenant);
  }

  getStatusSeverity(tenant: Tenant): 'success' | 'info' | 'warn' | 'danger' {
    if (!tenant.isActive) {
      return 'danger';
    }
    
    if (this.isTenantExpired(tenant)) {
      return 'warn';
    }
    
    return 'success';
  }

  hasPermission(permissions: string[]): boolean {
    return this.authService.hasAnyPermission(permissions);
  }
}
