import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="login-container">
      <div class="login-card">
        <h1 class="login-title">VGS Retail OS</h1>
        <p class="login-subtitle">Sign in to your account</p>
        
        <div class="error-banner" *ngIf="errorMessage">
          {{ errorMessage }}
        </div>

        <form (ngSubmit)="onSubmit()" #loginForm="ngForm" class="login-form">
          <div class="form-group">
            <label for="email">Email</label>
            <input type="email" id="email" name="email" [(ngModel)]="email" required class="form-control" placeholder="admin@vgs.com">
          </div>
          
          <div class="form-group">
            <label for="password">Password</label>
            <input type="password" id="password" name="password" [(ngModel)]="password" required class="form-control" placeholder="••••••••">
          </div>
          
          <button type="submit" class="btn btn-primary btn-block" [disabled]="loginForm.invalid || isLoading">
            {{ isLoading ? 'Signing in...' : 'Sign In' }}
          </button>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      display: flex;
      align-items: center;
      justify-content: center;
      height: 100vh;
      width: 100vw;
      background-color: var(--color-background);
    }
    .login-card {
      background-color: var(--color-surface);
      padding: var(--spacing-xl);
      border-radius: var(--radius-lg);
      box-shadow: var(--shadow-md);
      width: 100%;
      max-width: 400px;
      border: 1px solid var(--color-border);
    }
    .login-title {
      font-size: 1.5rem;
      font-weight: 700;
      text-align: center;
      margin-bottom: var(--spacing-xs);
      color: var(--color-primary);
    }
    .login-subtitle {
      text-align: center;
      color: var(--color-text-secondary);
      margin-bottom: var(--spacing-lg);
    }
    .login-form {
      display: flex;
      flex-direction: column;
      gap: var(--spacing-md);
    }
    .error-banner {
      background-color: #fee2e2;
      color: #b91c1c;
      padding: var(--spacing-sm);
      border-radius: var(--radius-sm);
      margin-bottom: var(--spacing-md);
      font-size: 0.875rem;
      text-align: center;
    }
  `]
})
export class LoginComponent {
  email = '';
  password = '';
  isLoading = false;
  errorMessage = '';

  private authService = inject(AuthService);
  private router = inject(Router);

  onSubmit() {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.authService.login(this.email, this.password).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'Invalid email or password';
      }
    });
  }
}
