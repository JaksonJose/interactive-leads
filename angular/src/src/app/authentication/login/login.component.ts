import { Component, inject, PLATFORM_ID, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateDirective, TranslatePipe, TranslateService } from '@ngx-translate/core';

import { AuthService } from '../services/auth.service';
import { LoginModel, LoginResponseWrapper } from '../models';
import { PRIME_NG_MODULES } from '../../shared/primeng-imports';

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
  //messages: Message[] = [];
  messages = signal<{ severity: 'error' | 'success' | 'info' | 'warn' | 'secondary' | 'contrast'; content: string }[]>([]);
  showRemainingAttempts = false;
  attemptsRemaining = 0;

  authService = inject(AuthService);
  router = inject(Router);
  fb = inject(FormBuilder);
  translate = inject(TranslateService);
  platformId = inject(PLATFORM_ID);

  constructor() {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
    });
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      const login = new LoginModel();

      login.userName = this.loginForm.get('username')?.value;
      login.password = this.loginForm.get('password')?.value;

      this.authService.AuthenticateUser(login).subscribe({
        next: (response: LoginResponseWrapper) => {
          if (response.isSuccessful && response.data) {
            this.authService.storeTokens(response.data);

            this.router.navigate(['/']);
          } else {
            this.messages.set([
              {
                severity: "error",
                content: response.messages.join(', ')
              }
            ]);
          }
        },
        error: (error: { error: LoginResponseWrapper }) => {
          this.messages.set([]);

          const errorResponse: LoginResponseWrapper = error.error;
          
          if (errorResponse && errorResponse.messages) {
            this.messages.set([
              {
                severity: "error",
                content: this.translate.instant(`api.error.${errorResponse.messages[0]}`)
              }
            ]);
          } else {
            this.messages.set([
              {
                severity: "error",
                content: this.translate.instant('login.error.generic')
              }
            ]);
          }
        }
      });
    }
  }
}
