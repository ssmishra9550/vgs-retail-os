import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, of, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'vgs_access_token';
  private readonly REFRESH_TOKEN_KEY = 'vgs_refresh_token';
  private readonly USER_KEY = 'vgs_user';

  // Signals for reactive state
  private currentUserSignal = signal<User | null>(this.getStoredUser());
  private tokenSignal = signal<string | null>(this.getStoredToken());

  public currentUser = this.currentUserSignal.asReadonly();
  public isAuthenticated = computed(() => this.tokenSignal() !== null);

  constructor(private http: HttpClient, private router: Router) {}

  login(email: string, password: string):Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, { email, password }).pipe(
      tap(response => {
        this.setAuthData(response.accessToken, response.refreshToken, response.user);
      })
    );
  }

  logout() {
    this.clearAuthData();
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.tokenSignal();
  }

  getTenantId(): string | null {
    const token = this.tokenSignal();
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.tenant_id || null;
    } catch {
      return null;
    }
  }

  private setAuthData(token: string, refreshToken: string, user: User) {
    localStorage.setItem(this.TOKEN_KEY, token);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    
    this.tokenSignal.set(token);
    this.currentUserSignal.set(user);
  }

  private clearAuthData() {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    
    this.tokenSignal.set(null);
    this.currentUserSignal.set(null);
  }

  private getStoredToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  private getStoredUser(): User | null {
    const userStr = localStorage.getItem(this.USER_KEY);
    if (userStr) {
      try {
        return JSON.parse(userStr);
      } catch {
        return null;
      }
    }
    return null;
  }
}
