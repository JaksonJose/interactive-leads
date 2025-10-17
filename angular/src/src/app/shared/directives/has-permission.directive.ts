import { Directive, Input, TemplateRef, ViewContainerRef,OnInit } from '@angular/core';
import { AuthService } from '@authentication/services/auth.service';

/**
 * Structural directive that conditionally renders content based on user permissions.
 * Usage: *hasPermission="['Permission.Tenants.Create']" or *hasPermission="'Permission.Tenants.Read'"
 */
@Directive({
  selector: '[hasPermission]'
})
export class HasPermissionDirective implements OnInit {
  @Input() hasPermission!: string | string[];

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private authService: AuthService
  ) {}

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
