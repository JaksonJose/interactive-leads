import { Routes } from '@angular/router';
import { LayoutComponent } from './layout/layout/layout.component';
import { LoginComponent } from './authentication/login/login.component';
import { authGuard } from './authentication/shared/guard/auth.guard';
import { tenantRoutes } from './features/management/tenants/tenants.routes';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      // Tenant Management Routes
      ...tenantRoutes,
      // Add other feature routes here as children
    ]
  },
  {
    path: '**',
    redirectTo: '/login'
  }
];
