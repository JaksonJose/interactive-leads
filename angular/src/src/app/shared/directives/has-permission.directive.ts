import { Directive, Input, TemplateRef, ViewContainerRef, OnInit, inject } from '@angular/core';
import { AuthService } from '@authentication/services/auth.service';

/**
 * Structural directive that conditionally renders content based on user permissions.
 * Usage: *appHasPermission="['Permission.Tenants.Create']" or *appHasPermission="'Permission.Tenants.Read'"
 */
@Directive({
  selector: '[appHasPermission]'
})
export class HasPermissionDirective implements OnInit {
  @Input() hasPermission!: string | string[];

  private readonly templateRef = inject(TemplateRef<any>);
  private readonly viewContainer = inject(ViewContainerRef);
  private readonly authService = inject(AuthService);

  ngOnInit() {
    if (!this.hasPermission) {
      this.viewContainer.clear();
      return;
    }

    const permissions = Array.isArray(this.hasPermission) 
      ? this.hasPermission 
      : [this.hasPermission];

    const hasPermission = this.authService.hasAnyPermission(permissions);

    if (hasPermission) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainer.clear();
    }
  }
}
