import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { HeaderComponent } from '../header/header.component';

@Component({
  selector: 'app-app-layout',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, HeaderComponent],
  template: `
    <div class="app-layout">
      <app-sidebar class="app-sidebar"></app-sidebar>
      <div class="app-main">
        <app-header class="app-header"></app-header>
        <div class="app-content">
          <router-outlet></router-outlet>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .app-layout {
      display: flex;
      height: 100vh;
      width: 100vw;
      overflow: hidden;
      background-color: var(--color-background);
    }
    .app-sidebar {
      width: var(--sidebar-width);
      flex-shrink: 0;
      border-right: 1px solid var(--color-border);
      background-color: var(--color-surface);
      z-index: 10;
    }
    .app-main {
      flex: 1;
      display: flex;
      flex-direction: column;
      overflow: hidden;
    }
    .app-header {
      height: var(--header-height);
      flex-shrink: 0;
      border-bottom: 1px solid var(--color-border);
      background-color: var(--color-surface);
      z-index: 5;
    }
    .app-content {
      flex: 1;
      overflow-y: auto;
      padding: var(--spacing-lg);
    }
  `]
})
export class AppLayoutComponent {

}
