import { Component, inject, OnInit } from '@angular/core';

import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { MenuItem } from 'primeng/api';
import { AuthService } from '../../authentication/services/auth.service';

@Component({
  selector: 'app-bar',
  templateUrl: './app-bar.component.html',
  styleUrls: ['./app-bar.component.scss'],
  imports: [...SHARED_IMPORTS]
})
export class AppBarComponent implements OnInit {
  // TODO: Move to a file
  allMenuItems = [
    {
      label: 'menu.dashboard',
      icon: 'pi pi-th-large',
      route: '/dashboard',
      expanded: false,
      permissions: [], // Dashboard accessible to all authenticated users
      children: []
    },
    {
      label: 'menu.leads',
      icon: 'pi pi-users',
      route: '/leads',
      expanded: false,
      permissions: [], // Leads accessible to all authenticated users
      children: []
    },
    {
      label: 'menu.salespipelines',
      icon: 'pi pi-filter',
      route: '/salespipelines',
      expanded: false,
      permissions: [], // Sales pipelines accessible to all authenticated users
      children: []
    },
    {
      label: 'menu.schedule',
      icon: 'pi pi-calendar',
      route: '/calendar',
      expanded: false,
      permissions: [], // Schedule accessible to all authenticated users
      children: []
    },
    {
      label: 'menu.report',
      icon: 'pi pi-file',
      expanded: false,
      permissions: [], // Reports accessible to all authenticated users
      children: [
        {
          label: 'menu.analytics',
          icon: 'pi pi-chart-line',
          route: '/analytics',
          permissions: []
        },
        {
          label: 'menu.graphics',
          icon: 'pi pi-chart-bar',
          route: '/graphics',
          permissions: []
        },
      ]
    },
    {
      label: 'menu.admin',
      icon: 'pi-cog',
      expanded: false,
      permissions: ['Permission.Users.Read'], // Admin section requires user management permission
      children: [
        {
          label: 'menu.company',
          icon: 'pi pi-building',
          route: '/admin/companies',
          permissions: ['Permission.Users.Read']
        },
        {
          label: 'menu.consultants',
          icon: 'pi pi-users',
          route: '/admin/consultants',
          permissions: ['Permission.Users.Read']
        },
        {
          label: 'menu.tenants',
          icon: 'pi pi-sitemap',
          route: '/tenants',
          permissions: ['Permission.Tenants.Read']
        }
      ]
    }
  ];

  menuItems: any[] = [];

  userMenuItems: MenuItem[] = [
    {
      label: 'Edit Profile',
      icon: 'pi pi-user-edit',
      //command: () => this.onEditProfile()
    },
    {
      label: 'Logout',
      icon: 'pi pi-sign-out',
      //command: () => this.onLogout()
    }
  ];

  authService = inject(AuthService);

  ngOnInit(): void {
    // Filter menu items based on user permissions
    this.filterMenuItemsByPermissions();
  }

  private filterMenuItemsByPermissions(): void {
    this.menuItems = this.allMenuItems.filter(item => {
      // If no permissions required, show the item
      if (!item.permissions || item.permissions.length === 0) {
        // Filter children if they exist
        if (item.children && item.children.length > 0) {
          item.children = item.children.filter(child => {
            return !child.permissions || child.permissions.length === 0 || 
                   this.authService.hasAnyPermission(child.permissions);
          });
          // Only show parent if it has visible children or no children required
          return item.children.length > 0;
        }
        return true;
      }

      // Check if user has required permissions
      const hasPermission = this.authService.hasAnyPermission(item.permissions);
      
      if (hasPermission && item.children && item.children.length > 0) {
        // Filter children based on their permissions
        item.children = item.children.filter(child => {
          return !child.permissions || child.permissions.length === 0 || 
                 this.authService.hasAnyPermission(child.permissions);
        });
        // Only show parent if it has visible children
        return item.children.length > 0;
      }

      return hasPermission;
    });
  }
}
