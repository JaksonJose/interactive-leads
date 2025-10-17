import { Component, inject } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { SHARED_IMPORTS } from '../../shared/shared-imports';

@Component({
  selector: 'app-appbar',
  templateUrl: './appbar.component.html',
  styleUrls: ['./appbar.component.scss'],
  imports: [...SHARED_IMPORTS]
})
export class AppbarComponent {
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
}
