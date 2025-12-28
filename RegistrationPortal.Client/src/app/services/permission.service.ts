import { Injectable } from '@angular/core';
import { AuthService, User } from '../services/auth.service';

export interface Permission {
  name: string;
  description: string;
  category: string;
}

@Injectable({
  providedIn: 'root'
})
export class PermissionService {
  private permissions: Permission[] = [
    // User Management Permissions
    { name: 'users.create', description: 'إنشاء مستخدمين', category: 'المستخدمين' },
    { name: 'users.read', description: 'عرض المستخدمين', category: 'المستخدمين' },
    { name: 'users.update', description: 'تحديث المستخدمين', category: 'المستخدمين' },
    { name: 'users.delete', description: 'حذف المستخدمين', category: 'المستخدمين' },
    { name: 'users.list', description: 'قائمة المستخدمين', category: 'المستخدمين' },
    { name: 'users.view_details', description: 'عرض تفاصيل المستخدم', category: 'المستخدمين' },
    { name: 'users.activate', description: 'تفعيل المستخدمين', category: 'المستخدمين' },
    { name: 'users.deactivate', description: 'إلغاء تفعيل المستخدمين', category: 'المستخدمين' },
    { name: 'users.reset_password', description: 'إعادة تعيين كلمة المرور', category: 'المستخدمين' },
    { name: 'users.assign_roles', description: 'تعيين أدوار للمستخدمين', category: 'المستخدمين' },

    // Role Management Permissions
    { name: 'roles.create', description: 'إنشاء أدوار', category: 'الأدوار' },
    { name: 'roles.read', description: 'عرض الأدوار', category: 'الأدوار' },
    { name: 'roles.update', description: 'تحديث الأدوار', category: 'الأدوار' },
    { name: 'roles.delete', description: 'حذف الأدوار', category: 'الأدوار' },
    { name: 'roles.list', description: 'قائمة الأدوار', category: 'الأدوار' },
    { name: 'roles.view_details', description: 'عرض تفاصيل الدور', category: 'الأدوار' },
    { name: 'roles.assign_permissions', description: 'تعيين صلاحيات للأدوار', category: 'الأدوار' },

    // Customer Management Permissions
    { name: 'customers.create', description: 'إنشاء عملاء', category: 'العملاء' },
    { name: 'customers.read', description: 'عرض العملاء', category: 'العملاء' },
    { name: 'customers.update', description: 'تحديث العملاء', category: 'العملاء' },
    { name: 'customers.delete', description: 'حذف العملاء', category: 'العملاء' },
    { name: 'customers.list', description: 'قائمة العملاء', category: 'العملاء' },
    { name: 'customers.view_details', description: 'عرض تفاصيل العميل', category: 'العملاء' },
    { name: 'customers.approve', description: 'موافقة على العميل', category: 'العملاء' },
    { name: 'customers.reject', description: 'رفض العميل', category: 'العملاء' },
    { name: 'customers.review', description: 'مراجعة العميل', category: 'العملاء' }
  ];

  constructor(private authService: AuthService) {}

  hasPermission(permission: string): boolean {
    const user = this.authService.currentUserValue;
    if (!user || !user.permissions) return false;

    return user.permissions.includes(permission);
  }

  hasAnyPermission(permissions: string[]): boolean {
    return permissions.some(permission => this.hasPermission(permission));
  }

  hasAllPermissions(permissions: string[]): boolean {
    return permissions.every(permission => this.hasPermission(permission));
  }

  getPermissionsByCategory(category: string): Permission[] {
    return this.permissions.filter(p => p.category === category);
  }

  getAllPermissions(): Permission[] {
    return this.permissions;
  }

  canAccessUserManagement(): boolean {
    return this.hasPermission('users.list');
  }

  canAccessRoleManagement(): boolean {
    return this.hasPermission('roles.list');
  }

  canAssignRoles(): boolean {
    return this.hasPermission('users.assign_roles');
  }

  canCreateRoles(): boolean {
    return this.hasPermission('roles.create');
  }

  canUpdateRoles(): boolean {
    return this.hasPermission('roles.update');
  }

  canAssignPermissions(): boolean {
    return this.hasPermission('roles.assign_permissions');
  }
}
