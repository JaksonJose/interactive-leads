import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { SHARED_IMPORTS } from '@shared/shared-imports';
import { AppbarComponent } from '@layout/appbar/appbar.component';
import { SidebarComponent } from '@layout/sidebar/sidebar.component';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss'],
  imports: [
    ...SHARED_IMPORTS,
    RouterOutlet,
    AppbarComponent,
    SidebarComponent,
  ]
})
export class LayoutComponent {
  collapsed = true;

  onSidebarCollapsedChange(collapsed: boolean) {
    this.collapsed = collapsed;
  }
}
