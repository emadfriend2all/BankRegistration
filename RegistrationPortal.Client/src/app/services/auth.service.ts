import { Injectable, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { LoginDto, API_BASE_URL } from '../api/client';

export interface User {
  id: string;
  email: string;
  name: string;
  role?: string;
  permissions: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject: BehaviorSubject<User | null>;
  public currentUser: Observable<User | null>;
  private readonly TOKEN_KEY = 'auth_token';
  private readonly USER_KEY = 'current_user';

  constructor(
    private router: Router,
    private http: HttpClient,
    @Inject(API_BASE_URL) private baseUrl: string
  ) {
    this.currentUserSubject = new BehaviorSubject<User | null>(this.getUserFromStorage());
    this.currentUser = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  public get isAuthenticated(): boolean {
    return !!this.getToken() && !!this.currentUserValue;
  }

  public get token(): string | null {
    return this.getToken();
  }

  login(email: string, password: string, rememberMe: boolean = false, returnUrl?: string): Observable<any> {
    const loginDto = new LoginDto({ username: email, password });
    const url = `${this.baseUrl}/api/Auth/login`;

    return this.http.post(url, loginDto).pipe(
      // Handle response and extract token/user info
      // You may need to adjust this based on your actual API response
      switchMap((response: any) => {
        const token = this.extractTokenFromResponse(response);
        const user = this.extractUserFromResponse(response);

        if (token && user) {
          this.setToken(token, rememberMe);
          this.setUser(user, rememberMe);
          this.currentUserSubject.next(user);
          
          // Navigate to returnUrl or default route
          const navigationUrl = returnUrl || '/';
          this.router.navigate([navigationUrl]);
          
          return of({ success: true, user });
        } else {
          return throwError(() => new Error('Invalid login response'));
        }
      })
    );
  }

  logout(): void {
    this.removeToken();
    this.removeUser();
    this.currentUserSubject.next(null);
    this.router.navigate(['/auth/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY) || sessionStorage.getItem(this.TOKEN_KEY);
  }

  private setToken(token: string, rememberMe: boolean): void {
    if (rememberMe) {
      localStorage.setItem(this.TOKEN_KEY, token);
    } else {
      sessionStorage.setItem(this.TOKEN_KEY, token);
    }
  }

  private removeToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    sessionStorage.removeItem(this.TOKEN_KEY);
  }

  private getUserFromStorage(): User | null {
    const userStr = localStorage.getItem(this.USER_KEY) || sessionStorage.getItem(this.USER_KEY);
    return userStr ? JSON.parse(userStr) : null;
  }

  private setUser(user: User, rememberMe: boolean): void {
    const userStr = JSON.stringify(user);
    if (rememberMe) {
      localStorage.setItem(this.USER_KEY, userStr);
    } else {
      sessionStorage.setItem(this.USER_KEY, userStr);
    }
  }

  private removeUser(): void {
    localStorage.removeItem(this.USER_KEY);
    sessionStorage.removeItem(this.USER_KEY);
  }

  private extractTokenFromResponse(response: any): string | null {
    // Adjust this based on your actual API response structure
    return response?.token || response?.accessToken || null;
  }

  private extractUserFromResponse(response: any): User | null {
    const token = this.extractTokenFromResponse(response);
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const permissions = payload?.permission || [];

      return {
        id: payload.nameid || '',
        email: payload.email || '',
        name: payload.unique_name || '',
        role: payload.role || '',
        permissions: Array.isArray(permissions) ? permissions : [permissions]
      };
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }

  private debugTokenClaims(token: string): void {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      console.log('JWT Token Payload:', payload);
      console.log('All Claims:', Object.keys(payload).map(key => `${key}: ${payload[key]}`));
      
      // Check for role claims specifically
      const roleClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      console.log('Role Claim (Microsoft schema):', roleClaim);
      
      const altRoleClaim = payload.role;
      console.log('Role Claim (direct):', altRoleClaim);
      
    } catch (error) {
      console.error('Error decoding token:', error);
    }
  }

  // Method to get authorization headers for API requests
  getAuthHeaders(): HttpHeaders {
    const token = this.getToken();
    if (token) {
      return new HttpHeaders({
        'Authorization': `Bearer ${token}`
      });
    }
    return new HttpHeaders();
  }

  // Check if token is expired (if your API uses expiring tokens)
  isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) return true;

    try {
      // If using JWT tokens, you can decode and check expiration
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp ? payload.exp * 1000 < Date.now() : false;
    } catch {
      return false; // If not JWT or can't decode, assume not expired
    }
  }

  // Auto logout if token is expired
  checkTokenExpiration(): void {
    if (this.isAuthenticated && this.isTokenExpired()) {
      this.logout();
    }
  }
}
