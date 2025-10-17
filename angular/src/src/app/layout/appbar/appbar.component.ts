import { Component, inject } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { Router } from '@angular/router';
import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { AuthService } from '../../authentication/services/auth.service';

@Component({
  selector: 'app-appbar',
  templateUrl: './appbar.component.html',
  styleUrls: ['./appbar.component.scss'],
  imports: [...SHARED_IMPORTS]
})
export class AppbarComponent {
  private router = inject(Router);
  private authService = inject(AuthService);

  userMenuItems: MenuItem[] = [
    {
      label: 'Editar Perfil',
      icon: 'pi pi-user-edit',
      //command: () => this.onEditProfile()
    },
    {
      label: 'Sair',
      icon: 'pi pi-sign-out',
      command: () => this.performLogout()
    }
  ];

  /**
   * Performs user logout
   */
  private performLogout(): void {
    // Clear authentication tokens
    this.authService.clearTokens();
    
    // Redirect to login page
    this.router.navigate(['/login']);
  }
}
