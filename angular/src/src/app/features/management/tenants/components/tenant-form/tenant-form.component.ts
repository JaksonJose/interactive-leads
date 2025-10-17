import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';

import { TenantService } from '@feature/management/tenants/services';
import { TenantRepository } from '@feature/management/tenants/repositories';
import { CreateTenantRequest, UpdateTenantRequest, Tenant } from '@feature/management/tenants/models';
import { Response } from '@core/responses/response';
import { PRIME_NG_MODULES } from '@shared/primeng-imports';

interface TenantFormValue {
  identifier: string;
  name: string;
  email: string;
  firstName: string;
  lastName: string;
  expirationDate: Date;
  isActive: boolean;
  connectionString: string;
}

@Component({
  selector: 'app-tenant-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslatePipe,
    ...PRIME_NG_MODULES
  ],
  templateUrl: './tenant-form.component.html',
  styleUrls: ['./tenant-form.component.scss']
})
export class TenantFormComponent implements OnInit {
  private readonly tenantService = inject(TenantService);
  private readonly tenantRepository = inject(TenantRepository);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);

  tenantForm!: FormGroup;
  loading = signal<boolean>(false);
  messages = signal<{ severity: 'error' | 'success' | 'info' | 'warn' | 'secondary' | 'contrast'; content: string }[]>([]);
  minDate = new Date();
  isEditMode = signal<boolean>(false);
  tenantId = signal<string | null>(null);
  currentTenant = signal<Tenant | null>(null);

  ngOnInit(): void {
    this.initializeForm();
    this.checkEditMode();
  }

  private checkEditMode(): void {
    const tenantId = this.route.snapshot.paramMap.get('tenantId');
    if (tenantId) {
      this.isEditMode.set(true);
      this.tenantId.set(tenantId);
      this.loadTenantForEdit(tenantId);
    }
  }

  private loadTenantForEdit(tenantId: string): void {
    this.loading.set(true);
    this.tenantRepository.getTenantById(tenantId).subscribe({
      next: (response: Response<Tenant>) => {
        if (response.data) {
          this.currentTenant.set(response.data);
          this.populateFormWithTenantData(response.data);
        }
        this.loading.set(false);
      },
      error: () => {
        this.messages.set([{
          severity: 'error',
          content: 'Error loading tenant data'
        }]);
        this.loading.set(false);
      }
    });
  }

  private populateFormWithTenantData(tenant: Tenant): void {
    this.tenantForm.patchValue({
      identifier: tenant.identifier,
      name: tenant.name,
      email: tenant.email,
      firstName: tenant.firstName,
      lastName: tenant.lastName,
      expirationDate: new Date(tenant.expirationDate),
      isActive: tenant.isActive,
      connectionString: tenant.connectionString || ''
    });
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

      const formValue = this.tenantForm.value as TenantFormValue;
      
      if (this.isEditMode()) {
        this.updateTenant(formValue);
      } else {
        this.createTenant(formValue);
      }
    } else {
      this.markFormGroupTouched();
      this.messages.set([{
        severity: 'warn',
        content: 'Please fill in all required fields correctly'
      }]);
    }
  }

  private createTenant(formValue: TenantFormValue): void {
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

    this.tenantRepository.createTenant(createRequest).subscribe({
      next: (response: Response<Tenant>) => {
        this.messages.set([{
          severity: 'success',
          content: 'Tenant created successfully'
        }]);
        
        setTimeout(() => {
          this.router.navigate(['/tenants']);
        }, 2000);
      },
      error: () => {
        this.messages.set([{
          severity: 'error',
          content: 'Error creating tenant'
        }]);
        this.loading.set(false);
      }
    });
  }

  private updateTenant(formValue: TenantFormValue): void {
    const updateRequest: UpdateTenantRequest = {
      identifier: formValue.identifier,
      name: formValue.name,
      email: formValue.email,
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      expirationDate: new Date(formValue.expirationDate),
      isActive: formValue.isActive,
      connectionString: formValue.connectionString || undefined
    };

    this.tenantRepository.updateTenant(this.tenantId()!, updateRequest).subscribe({
      next: (response: Response<Tenant>) => {
        this.messages.set([{
          severity: 'success',
          content: 'Tenant updated successfully'
        }]);
        
        setTimeout(() => {
          this.router.navigate(['/tenants']);
        }, 2000);
      },
      error: (error) => {
        this.messages.set([{
          severity: 'error',
          content: 'Error updating tenant'
        }]);
        this.loading.set(false);
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/tenants']);
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
