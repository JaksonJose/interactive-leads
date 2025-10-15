import { Component, inject, OnInit } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { MenuItem } from 'primeng/api';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss'],
  imports: [
    ...SHARED_IMPORTS,
    NgClass,
    TranslatePipe,
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
]
})
export class LayoutComponent implements OnInit {
  collapsed = true;
  currentRoute = '';

  // TODO: Move to a file
  menuItems = [
    {
      label: 'menu.dashboard',
      icon: 'pi pi-th-large',
      route: '/dashboard',
      expanded: false,
      roles: ['SysAdmin', 'Owner', 'Support', 'Manager', 'Consultant'],
      children: []
    },
    {
      label: 'menu.leads',
      icon: 'pi pi-users',
      route: '/leads',
      expanded: false,
      roles: ['SysAdmin', 'Owner', 'Support', 'Manager', 'Consultant'],
      children: []
    },
    {
      label: 'menu.salespipelines',
      icon: 'pi pi-filter',
      route: '/salespipelines',
      expanded: false,
      roles: ['SysAdmin', 'Owner', 'Support', 'Manager', 'Consultant'],
      children: []
    },
    {
      label: 'menu.schedule',
      icon: 'pi pi-calendar',
      route: '/calendar',
      expanded: false,
      roles: ['SysAdmin', 'Owner', 'Support', 'Manager', 'Consultant'],
      children: []
    },
    {
      label: 'menu.report',
      icon: 'pi pi-file',
      expanded: false,
      roles: ['SysAdmin', 'Owner', 'Support', 'Manager', 'Consultant'],
      children: [
        {
          label: 'menu.analytics',
          icon: 'pi pi-chart-line',
          route: '/analytics'
        },
        {
          label: 'menu.graphics',
          icon: 'pi pi-chart-bar',
          route: '/graphics'
        },
      ]
    },
    {
      label: 'menu.admin',
      icon: 'pi-cog',
      expanded: false,
      roles: ['SysAdmin', 'Owner', 'Support', 'Manager'],
      children: [
        {
          label: 'menu.company',
          icon: 'pi pi-building',
          route: '/admin/companies'
        },
        {
          label: 'menu.consultants',
          icon: 'pi pi-users',
          route: '/admin/consultants'
        }
      ]
    }
  ];

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

  router = inject(Router);

  ngOnInit(): void {
    // It always updates the current route when navigation changes
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.currentRoute = event.urlAfterRedirects;
      }
    });

    //const userRoles = this.authService.getUserRoles();
    // this.menuItems = this.filterMenuItemsByRoles(this.menuItems, userRoles);
  }

   isActive(route: string | undefined): boolean {
    return this.currentRoute === route;
  }

  // filterMenuItemsByRoles(menu: any[], roles: string[]): any[] {
  //   return menu
  //     .filter(item => !item.roles || item.roles.some((role: string) => roles.includes(role)))
  //     .map(item => ({
  //       ...item,
  //       children: item.children ? this.filterMenuItemsByRoles(item.children, roles) : [],
  //     }));
  // }

  toggleSubMenu(item: { expanded: boolean }) {
    item.expanded = !item.expanded;
  }

  onMouseEnter() {
    this.collapsed = false;
  }

  onMouseLeave() {
    this.collapsed = true;
  }
}
