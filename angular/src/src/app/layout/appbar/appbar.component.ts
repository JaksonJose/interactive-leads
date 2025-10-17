import { Component, inject } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { Router } from '@angular/router';
import { SHARED_IMPORTS } from '@shared/shared-imports';
import { AuthService } from '@authentication/services/auth.service';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-appbar',
  templateUrl: './appbar.component.html',
  styleUrls: ['./appbar.component.scss'],
  imports: [...SHARED_IMPORTS]
})
export class AppbarComponent {
  private router = inject(Router);
  private authService = inject(AuthService);
  private messageService = inject(MessageService);

  userMenuItems: MenuItem[] = [
    {
      label: 'Edit Profile',
      icon: 'pi pi-user-edit',
      //command: () => this.onEditProfile()
    },
    {
      separator: true
    },
    {
      label: 'Logout from this device',
      icon: 'pi pi-sign-out',
      command: () => this.performDeviceLogout()
    },
    {
      label: 'Logout from all devices',
      icon: 'pi pi-power-off',
      command: () => this.performLogoutFromAllDevices()
    }
  ];

  /**
   * Performs logout from current device only
   */
  private async performDeviceLogout(): Promise<void> {
    try {
      const success = await this.authService.logoutFromCurrentDevice();
      
      if (success) {
        this.messageService.add({
          severity: 'success',
          summary: 'Logout successful',
          detail: 'You have been logged out from this device'
        });
      } else {
        this.messageService.add({
          severity: 'warn',
          summary: 'Logout completed',
          detail: 'You have been logged out (some devices may not have been notified)'
        });
      }
      
      // Redirect to login page
      this.router.navigate(['/login']);
    } catch (error) {
      console.error('Device logout error:', error);
      this.messageService.add({
        severity: 'error',
        summary: 'Logout error',
        detail: 'An error occurred while logging out'
      });
      
      // Clear tokens anyway and redirect
      this.authService.clearTokens();
      this.router.navigate(['/login']);
    }
  }

  /**
   * Performs logout from all devices
   */
  private async performLogoutFromAllDevices(): Promise<void> {
    try {
      const success = await this.authService.logoutFromAllDevices();
      
      if (success) {
        this.messageService.add({
          severity: 'success',
          summary: 'Logout successful',
          detail: 'You have been logged out from all devices'
        });
      } else {
        this.messageService.add({
          severity: 'warn',
          summary: 'Logout completed',
          detail: 'You have been logged out (some devices may not have been notified)'
        });
      }
      
      // Redirect to login page
      this.router.navigate(['/login']);
    } catch (error) {
      console.error('Logout from all devices error:', error);
      this.messageService.add({
        severity: 'error',
        summary: 'Logout error',
        detail: 'An error occurred while logging out from all devices'
      });
      
      // Clear tokens anyway and redirect
      this.authService.clearTokens();
      this.router.navigate(['/login']);
    }
  }
}
