import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';

import { TenantService } from '../../services';
import { CreateTenantRequest } from '../../models';
import { PRIME_NG_MODULES } from '@shared/primeng-imports';

@Component({
  selector: 'app-tenant-create',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslatePipe,
    ...PRIME_NG_MODULES
  ],
  templateUrl: './tenant-create.component.html',
  styleUrls: ['./tenant-create.component.scss']
})
export class TenantCreateComponent implements OnInit {
  private readonly tenantService = inject(TenantService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  tenantForm!: FormGroup;
  loading = signal<boolean>(false);
  messages = signal<{ severity: 'error' | 'success' | 'info' | 'warn' | 'secondary' | 'contrast'; content: string }[]>([]);
  minDate = new Date();

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm(): void {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);

    this.tenantForm = this.fb.group({
      identifier: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50), Validators.pattern(/^[a-z0-9-_]+$/)]],
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      expirationDate: [tomorrow, [Validators.required]],
      isActive: [true],
      connectionString: ['']
    });
  }

  onSubmit(): void {
    if (this.tenantForm.valid) {
      this.loading.set(true);
      this.messages.set([]);

      const formValue = this.tenantForm.value;
      const createRequest: CreateTenantRequest = {
        identifier: formValue.identifier,
        name: formValue.name,
        email: formValue.email,
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        expirationDate: new Date(formValue.expirationDate),
        isActive: formValue.isActive,
        connectionString: formValue.connectionString || undefined
      };

      this.tenantService.createTenant(createRequest).subscribe({
        next: (tenant) => {
          this.messages.set([{
            severity: 'success',
            content: 'Tenant created successfully'
          }]);
          
          setTimeout(() => {
            this.router.navigate(['/management/tenants']);
          }, 2000);
        },
        error: (error) => {
          this.messages.set([{
            severity: 'error',
            content: 'Error creating tenant'
          }]);
          this.loading.set(false);
        }
      });
    } else {
      this.markFormGroupTouched();
      this.messages.set([{
        severity: 'warn',
        content: 'Please fill in all required fields correctly'
      }]);
    }
  }

  onCancel(): void {
    this.router.navigate(['/management/tenants']);
  }

  private markFormGroupTouched(): void {
    Object.keys(this.tenantForm.controls).forEach(key => {
      const control = this.tenantForm.get(key);
      control?.markAsTouched();
    });
  }

  getFieldError(fieldName: string): string {
    const control = this.tenantForm.get(fieldName);
    
    if (control?.errors && control.touched) {
      if (control.errors['required']) {
        return `${fieldName} is required`;
      }
      if (control.errors['email']) {
        return 'Invalid email format';
      }
      if (control.errors['minlength']) {
        return `${fieldName} must be at least ${control.errors['minlength'].requiredLength} characters`;
      }
      if (control.errors['maxlength']) {
        return `${fieldName} must not exceed ${control.errors['maxlength'].requiredLength} characters`;
      }
      if (control.errors['pattern']) {
        return `${fieldName} contains invalid characters`;
      }
    }
    
    return '';
  }

  isFieldInvalid(fieldName: string): boolean {
    const control = this.tenantForm.get(fieldName);
    return !!(control?.errors && control.touched);
  }
}
