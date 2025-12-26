import { Routes, RouterModule, CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { Documentation } from './documentation/documentation';
import { Crud } from './crud/crud';
import { Empty } from './empty/empty';
import { CreateCustomerComponent } from './customer/create-customer.component';
import { UpdateCustomerComponent } from './customer/update/update-customer.component';
import { ViewAllCustomersComponent } from './customer/view-all-customers.component';
import { CustomerDetailsComponent } from './customer/customer-details.component';
import { AuthService } from '../services/auth.service';

// Create a simple guard function inline to avoid import issues
const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router: Router = inject(Router);
  
  if (authService.isAuthenticated) {
    // Check if token is expired
    if (authService.isTokenExpired()) {
      authService.logout();
      return false;
    }
    return true;
  }

  // Not authenticated, redirect to login page
  router.navigate(['/auth/login'], { 
    queryParams: { returnUrl: state.url } 
  });
  return false;
};

export default [
    { path: 'documentation', component: Documentation },
    { path: 'crud', component: Crud },
    { path: 'empty', component: Empty },
    { path: 'customer/create', component: CreateCustomerComponent },
    { path: 'customer/update', component: UpdateCustomerComponent },
    { path: 'customers', component: ViewAllCustomersComponent, canActivate: [authGuard] },
    { path: 'customer/:id', component: CustomerDetailsComponent, canActivate: [authGuard] },
    { path: '**', redirectTo: '/notfound' }
] as Routes;
