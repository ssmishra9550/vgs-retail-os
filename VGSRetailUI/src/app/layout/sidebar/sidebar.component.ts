import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

interface NavItem {
  label: string;
  route: string;
  icon?: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="sidebar">
      <div class="sidebar-brand">
        <span class="brand-text">VGS Retail OS</span>
      </div>
      
      <nav class="sidebar-nav">
        @for (item of navItems; track item.route) {
          <a [routerLink]="item.route" 
             routerLinkActive="active" 
             class="nav-link">
            <span class="nav-label">{{ item.label }}</span>
          </a>
        }
      </nav>
    </div>
  `,
  styles: [`
    .sidebar {
      display: flex;
      flex-direction: column;
      height: 100%;
    }
    
    .sidebar-brand {
      height: var(--header-height);
      display: flex;
      align-items: center;
      padding: 0 var(--spacing-lg);
      border-bottom: 1px solid var(--color-border);
    }
    
    .brand-text {
      font-weight: 700;
      font-size: 1.25rem;
      color: var(--color-primary);
      letter-spacing: -0.02em;
    }
    
    .sidebar-nav {
      flex: 1;
      padding: var(--spacing-md) 0;
      overflow-y: auto;
      display: flex;
      flex-direction: column;
      gap: 4px;
    }
    
    .nav-link {
      display: flex;
      align-items: center;
      padding: var(--spacing-md) var(--spacing-lg);
      color: var(--color-text-secondary);
      text-decoration: none;
      font-weight: 500;
      transition: all var(--transition-fast);
      border-left: 3px solid transparent;
    }
    
    .nav-link:hover {
      background-color: var(--color-background);
      color: var(--color-text);
    }
    
    .nav-link.active {
      background-color: var(--color-primary-light);
      color: var(--color-primary);
      border-left-color: var(--color-primary);
    }
  `]
})
export class SidebarComponent {
  navItems: NavItem[] = [
    { label: 'Dashboard', route: '/dashboard' },
    { label: 'Point of Sale', route: '/pos' },
    { label: 'Sales History', route: '/sales' },
    { label: 'Inventory', route: '/inventory' },
    { label: 'Purchases', route: '/purchases' },
    { label: 'Suppliers', route: '/suppliers' },
    { label: 'Customers', route: '/customers' },
    { label: 'Expenses', route: '/expenses' },
    { label: 'Reports', route: '/reports' },
    { label: 'Master Data', route: '/master-data' },
    { label: 'Settings', route: '/settings' }
  ];
}
