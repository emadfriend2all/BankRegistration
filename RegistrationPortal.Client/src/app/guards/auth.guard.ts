import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean {
    // Check if user is authenticated
    if (this.authService.isAuthenticated) {
      // Check if token is expired
      if (this.authService.isTokenExpired()) {
        this.authService.logout();
        return false;
      }
      return true;
    }

    // Not authenticated, redirect to login page
    this.router.navigate(['/auth/login'], { 
      queryParams: { returnUrl: state.url } 
    });
    return false;
  }
}
