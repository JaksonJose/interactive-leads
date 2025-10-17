import { Component, inject, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { NgClass } from '@angular/common';

import { SHARED_IMPORTS } from '@shared/shared-imports';
import { MenuItemInterface } from '@shared/interfaces/menu-item.interface';
import { AuthService } from '@authentication/services/auth.service';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
  imports: [
    ...SHARED_IMPORTS,
    NgClass,
    TranslatePipe,
    RouterLink,
    RouterLinkActive,
  ]
})
export class SidebarComponent implements OnInit {
  @Input() collapsed = true;
  @Output() collapsedChange = new EventEmitter<boolean>();

  currentRoute = '';

  // TODO: Move to a file
  allMenuItems: MenuItemInterface[] = [
    // Root Admin Menu - Only visible to users with tenant management permissions
    {
      label: 'menu.tenants',
      icon: 'pi pi-sitemap',
      route: '/tenants',
      expanded: false,
      permissions: ['Permission.Tenants.Read'], // Only root admins can manage tenants
      children: []
    },
    // Tenant Admin Menu - Only visible to users with user management permissions within tenant
    {
      label: 'menu.admin',
      icon: 'pi pi-cog',
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
        }
      ]
    }
    // Note: CRM features (Dashboard, Leads, Sales Pipelines, Schedule, Reports) 
    // are not yet implemented and should be added when the corresponding 
    // components and routes are created
  ];

  menuItems: MenuItemInterface[] = [];

  router = inject(Router);
  authService = inject(AuthService);

  ngOnInit(): void {
    // It always updates the current route when navigation changes
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.currentRoute = event.urlAfterRedirects;
      }
    });

    // Filter menu items based on user permissions
    this.filterMenuItemsByPermissions();
  }

  private filterMenuItemsByPermissions(): void {
    this.menuItems = this.allMenuItems.filter(item => {
      // Check if user has required permissions for the main item
      const hasPermission = this.authService.hasAnyPermission(item.permissions);
      
      if (!hasPermission) {
        return false;
      }

      // If item has children, filter them based on their permissions
      if (item.children && item.children.length > 0) {
        const originalChildren = [...item.children]; // Keep original for reference
        item.children = item.children.filter(child => {
          return this.authService.hasAnyPermission(child.permissions);
        });
        
        // Only show parent if it has visible children
        return item.children.length > 0;
      }

      return true;
    });
  }

  isActive(route: string | undefined): boolean {
    return this.currentRoute === route;
  }

  toggleSubMenu(item: MenuItemInterface) {
    if (item.expanded !== undefined) {
      item.expanded = !item.expanded;
    }
  }

  onMouseEnter() {
    this.collapsed = false;
    this.collapsedChange.emit(false);
  }

  onMouseLeave() {
    this.collapsed = true;
    this.collapsedChange.emit(true);
  }
}
