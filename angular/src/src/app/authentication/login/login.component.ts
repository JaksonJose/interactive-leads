import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { AuthService } from '@authentication/services/auth.service';
import { AuthRepository } from '@authentication/repositories/auth.repository';
import { LoginModel, TokenResponse } from '@authentication/models';
import { Response } from '@core/responses/response';
import { PRIME_NG_MODULES } from '@shared/primeng-imports';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    ...PRIME_NG_MODULES 
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {

  loginForm: FormGroup;
  messages = signal<{ severity: 'error' | 'success' | 'info' | 'warn' | 'secondary' | 'contrast'; content: string }[]>([]);
  showRemainingAttempts = false;
  attemptsRemaining = 0;
  loading = signal<boolean>(false);

  authService = inject(AuthService);
  authRepository = inject(AuthRepository);
  router = inject(Router);
  fb = inject(FormBuilder);
  translate = inject(TranslateService);

  constructor() {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
    });
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.loading.set(true);
      this.messages.set([]);

      const login = new LoginModel();
      login.userName = this.loginForm.get('username')?.value;
      login.password = this.loginForm.get('password')?.value;

      this.authRepository.authenticateUser(login).subscribe({
        next: (response: Response<TokenResponse>) => {
          this.loading.set(false);
          
          if (response.data) {
            this.authService.storeTokens(response.data);
            this.router.navigate(['/']);          
          } else {
            const errorCode = response.messages?.[0]?.code || 'auth.login_error';
            const errorMessage = this.translate.instant(errorCode) || 'Login error';
            this.messages.set([{
              severity: 'error',
              content: errorMessage
            }]);
          }
        },
        error: (error) => {
          this.loading.set(false);
          const errorCode = error.error?.messages?.[0]?.code || 'general.connection_error';
            const errorMessage = this.translate.instant(errorCode) || 'Connection error';
          this.messages.set([{
            severity: 'error',
            content: errorMessage
          }]);
        }
      });
    }
  }
}
