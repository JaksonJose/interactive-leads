import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { TenantUserRepository } from '@feature/management/tenants/repositories';
import { CreateUserRequest, UpdateUserRequest, User } from '@feature/management/tenants/models';
import { PRIME_NG_MODULES } from '@shared/primeng-imports';
import { Response } from '@core/responses/response';

interface UserFormValue {
  firstName: string;
  lastName: string;
  email: string;
  password?: string;
  confirmPassword?: string;
  phoneNumber: string;
  isActive: boolean;
}

@Component({
  selector: 'app-tenant-user-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslatePipe,
    ...PRIME_NG_MODULES
  ],
  templateUrl: './tenant-user-form.component.html',
  styleUrls: ['./tenant-user-form.component.scss']
})
export class TenantUserFormComponent implements OnInit {
  private readonly tenantUserRepository = inject(TenantUserRepository);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly translateService = inject(TranslateService);

  userForm!: FormGroup;
  loading = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  tenantId = signal<string | null>(null);
  userId = signal<string | null>(null);
  currentUser = signal<User | null>(null);

  ngOnInit(): void {
    this.initializeForm();
    this.checkEditMode();
  }

  private checkEditMode(): void {
    const tenantId = this.route.snapshot.paramMap.get('tenantId');
    const userId = this.route.snapshot.paramMap.get('userId');
    
    if (tenantId) {
      this.tenantId.set(tenantId);
    }

    if (userId) {
      this.isEditMode.set(true);
      this.userId.set(userId);
      if (tenantId) {
        this.loadUserForEdit(tenantId, userId);
      }
    }
  }

  private loadUserForEdit(tenantId: string, userId: string): void {
    this.loading.set(true);
    this.tenantUserRepository.getUserInTenant(tenantId, userId).subscribe({
      next: (response: Response<User>) => {
        if (response.data) {
          this.currentUser.set(response.data);
          this.populateFormWithUserData(response.data);
        }
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  private populateFormWithUserData(user: User): void {
    this.userForm.patchValue({
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      phoneNumber: user.phoneNumber || '',
      isActive: user.isActive
    });
    
    // Remove password validators in edit mode
    this.userForm.get('password')?.clearValidators();
    this.userForm.get('confirmPassword')?.clearValidators();
    this.userForm.get('password')?.updateValueAndValidity();
    this.userForm.get('confirmPassword')?.updateValueAndValidity();
  }

  private initializeForm(): void {
    this.userForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
      phoneNumber: [''],
      isActive: [true]
    }, { validators: this.passwordMatchValidator });
  }

  private passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');
    
    if (!password || !confirmPassword) {
      return null;
    }
    
    return password.value === confirmPassword.value ? null : { passwordMismatch: true };
  }

  onSubmit(): void {
    if (this.userForm.valid) {
      this.loading.set(true);

      const formValue = this.userForm.value as UserFormValue;
      
      if (this.isEditMode()) {
        this.updateUser(formValue);
      } else {
        this.createUser(formValue);
      }
    } else {
      this.markFormGroupTouched();
    }
  }

  private createUser(formValue: UserFormValue): void {
    const tenantId = this.tenantId();
    if (!tenantId) return;

    const createRequest: CreateUserRequest = {
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      email: formValue.email,
      password: formValue.password!,
      confirmPassword: formValue.confirmPassword!,
      phoneNumber: formValue.phoneNumber || undefined,
      isActive: formValue.isActive
    };

    this.tenantUserRepository.createUserInTenant(tenantId, createRequest).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/tenants', tenantId, 'users']);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  private updateUser(formValue: UserFormValue): void {
    const tenantId = this.tenantId();
    const userId = this.userId();
    if (!tenantId || !userId) return;

    const updateRequest: UpdateUserRequest = {
      id: userId,
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      phoneNumber: formValue.phoneNumber || undefined
    };

    this.tenantUserRepository.updateUserInTenant(tenantId, userId, updateRequest).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/tenants', tenantId, 'users']);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  onCancel(): void {
    const tenantId = this.tenantId();
    if (tenantId) {
      this.router.navigate(['/tenants', tenantId, 'users']);
    } else {
      this.router.navigate(['/tenants']);
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.userForm.controls).forEach(key => {
      const control = this.userForm.get(key);
      control?.markAsTouched();
    });
  }

  getFieldError(fieldName: string): string {
    const control = this.userForm.get(fieldName);
    
    if (control?.errors && control.touched) {
      const fieldKey = fieldName === 'email' ? 'emailField' : fieldName;
      const fieldDisplayName = this.translateService.instant(`validation.${fieldKey}`);
      
      if (control.errors['required']) {
        return this.translateService.instant('validation.required', { fieldName: fieldDisplayName });
      }
      if (control.errors['email']) {
        return this.translateService.instant('validation.email');
      }
      if (control.errors['minlength']) {
        return this.translateService.instant('validation.minlength', { 
          fieldName: fieldDisplayName, 
          requiredLength: control.errors['minlength'].requiredLength 
        });
      }
      if (control.errors['maxlength']) {
        return this.translateService.instant('validation.maxlength', { 
          fieldName: fieldDisplayName, 
          requiredLength: control.errors['maxlength'].requiredLength 
        });
      }
      if (control.errors['passwordMismatch']) {
        return this.translateService.instant('validation.passwordMismatch');
      }
    }
    
    return '';
  }

  isFieldInvalid(fieldName: string): boolean {
    const control = this.userForm.get(fieldName);
    return !!(control?.errors && control.touched);
  }

  isPasswordFieldRequired(): boolean {
    return !this.isEditMode();
  }
}

