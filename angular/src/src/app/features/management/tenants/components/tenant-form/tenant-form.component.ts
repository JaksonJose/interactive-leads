import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { TenantRepository } from '@feature/management/tenants/repositories';
import { CreateTenantRequest, UpdateTenantRequest, Tenant } from '@feature/management/tenants/models';
import { PRIME_NG_MODULES } from '@shared/primeng-imports';

interface TenantFormValue {
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
  private readonly tenantRepository = inject(TenantRepository);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly translateService = inject(TranslateService);

  tenantForm!: FormGroup;
  loading = signal<boolean>(false);
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
    this.tenantRepository.getTenantById(tenantId).subscribe(response => {
      if (response.data) {
        this.currentTenant.set(response.data);
        this.populateFormWithTenantData(response.data);
      }

      this.loading.set(false);
    });
  }

  private populateFormWithTenantData(tenant: Tenant): void {
    this.tenantForm.patchValue({
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

      const formValue = this.tenantForm.value as TenantFormValue;
      
      if (this.isEditMode()) {
        this.updateTenant(formValue);
      } else {
        this.createTenant(formValue);
      }
    } else {
      this.markFormGroupTouched();
    }
  }

  private createTenant(formValue: TenantFormValue): void {
    const createRequest: CreateTenantRequest = {
      name: formValue.name,
      email: formValue.email,
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      expirationDate: new Date(formValue.expirationDate),
      isActive: formValue.isActive,
      connectionString: formValue.connectionString || undefined
    };

    this.tenantRepository.createTenant(createRequest).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/tenants']);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  private updateTenant(formValue: TenantFormValue): void {
    const updateRequest: UpdateTenantRequest = {
      identifier: this.currentTenant()!.identifier,
      name: formValue.name,
      email: formValue.email,
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      expirationDate: new Date(formValue.expirationDate),
      isActive: formValue.isActive,
      connectionString: formValue.connectionString || undefined
    };

    this.tenantRepository.updateTenant(this.tenantId()!, updateRequest).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/tenants']);
      },
      error: () => {
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
      if (control.errors['pattern']) {
        return this.translateService.instant('validation.pattern', { fieldName: fieldDisplayName });
      }
    }
    
    return '';
  }

  isFieldInvalid(fieldName: string): boolean {
    const control = this.tenantForm.get(fieldName);
    return !!(control?.errors && control.touched);
  }
}
