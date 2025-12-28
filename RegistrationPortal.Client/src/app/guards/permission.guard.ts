import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PermissionService } from '../services/permission.service';

export const permissionGuard: CanActivateFn = (route, state) => {
  const permissionService = inject(PermissionService);
  const router = inject(Router);
  const requiredPermission = route.data['permission'];

  if (!requiredPermission) {
    return true;
  }

  if (permissionService.hasPermission(requiredPermission)) {
    return true;
  } else {
    router.navigate(['/auth/access-denied']);
    return false;
  }
};
