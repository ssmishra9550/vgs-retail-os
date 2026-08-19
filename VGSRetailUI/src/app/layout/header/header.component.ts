import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule],
  template: `
    <header class="header">
      <div class="header-left">
        <!-- Optional Breadcrumbs or Page Title could go here -->
      </div>
      
      <div class="header-right">
        <div class="user-profile" *ngIf="auth.currentUser() as user">
          <div class="user-info">
            <span class="user-name">{{ user.firstName }} {{ user.lastName }}</span>
            <span class="user-role">Administrator</span>
          </div>
          <div class="user-avatar">
            {{ user.firstName[0] }}{{ user.lastName[0] }}
          </div>
        </div>
        
        <button class="btn btn-outline" (click)="logout()">Logout</button>
      </div>
    </header>
  `,
  styles: [`
    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      height: 100%;
      padding: 0 var(--spacing-lg);
    }
    
    .header-right {
      display: flex;
      align-items: center;
      gap: var(--spacing-lg);
    }
    
    .user-profile {
      display: flex;
      align-items: center;
      gap: var(--spacing-md);
    }
    
    .user-info {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
    }
    
    .user-name {
      font-weight: 600;
      font-size: 0.875rem;
      color: var(--color-text);
    }
    
    .user-role {
      font-size: 0.75rem;
      color: var(--color-text-secondary);
    }
    
    .user-avatar {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      background-color: var(--color-primary);
      color: white;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 600;
      font-size: 0.875rem;
    }
  `]
})
export class HeaderComponent {
  auth = inject(AuthService);

  logout() {
    this.auth.logout();
  }
}
