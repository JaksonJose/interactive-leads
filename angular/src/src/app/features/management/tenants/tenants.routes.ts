import { Routes } from '@angular/router';
import { permissionGuard } from '@shared/guards/permission.guard';
import { TenantListComponent } from './components/tenant-list/tenant-list.component';
import { TenantCreateComponent } from './components/tenant-create/tenant-create.component';
import { TenantDetailsComponent } from './components/tenant-details/tenant-details.component';

export const tenantRoutes: Routes = [
  {
    path: 'tenants',
    canActivate: [permissionGuard],
    data: { permissions: ['Permission.Tenants.Read'] },
    children: [
      {
        path: '',
        component: TenantListComponent,
        title: 'Tenant Management'
      },
      {
        path: 'create',
        component: TenantCreateComponent,
        data: { permissions: ['Permission.Tenants.Create'] },
        title: 'Create Tenant'
      },
      {
        path: ':tenantId',
        component: TenantDetailsComponent,
        title: 'Tenant Details'
      },
      {
        path: ':tenantId/edit',
        component: TenantCreateComponent, // Reuse create component for editing
        data: { permissions: ['Permission.Tenants.Update'] },
        title: 'Edit Tenant'
      }
    ]
  }
];
