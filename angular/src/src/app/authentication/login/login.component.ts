import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateDirective, TranslatePipe, TranslateService } from '@ngx-translate/core';

import { AuthService } from '@authentication/services/auth.service';
import { LoginModel, TokenResponse } from '@authentication/models';
import { Response } from '@core/responses/response';
import { PRIME_NG_MODULES } from '@shared/primeng-imports';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateDirective,
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
  carregando = signal<boolean>(false);

  authService = inject(AuthService);
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
      this.carregando.set(true);
      this.messages.set([]);

      const login = new LoginModel();
      login.userName = this.loginForm.get('username')?.value;
      login.password = this.loginForm.get('password')?.value;

      this.authService.AuthenticateUser(login).subscribe({
        next: (response: Response<TokenResponse>) => {
          this.carregando.set(false);
          if (response.isSuccessful && response.data) {
            this.authService.storeTokens(response.data);
            this.router.navigate(['/']);          
          } else {
            const errorMessage = response.messages?.[0]?.text || 'Erro ao fazer login';
            this.messages.set([{
              severity: 'error',
              content: errorMessage
            }]);
          }
        },
        error: (error) => {
          this.carregando.set(false);
          this.messages.set([{
            severity: 'error',
            content: error.error.messages[0].text || 'Erro de conexão'
          }]);
        }
      });
    }
  }
}
