import { Component } from '@angular/core';

import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'app-bar',
  templateUrl: './app-bar.component.html',
  styleUrls: ['./app-bar.component.scss'],
  imports: [...SHARED_IMPORTS]
})
export class AppBarComponent {
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
      label: 'Editar Perfil',
      icon: 'pi pi-user-edit',
      //command: () => this.onEditProfile()
    },
    {
      label: 'Logout',
      icon: 'pi pi-sign-out',
      //command: () => this.onLogout()
    }
  ];
}
