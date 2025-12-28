import { Component, OnInit } from '@angular/core';
import { forkJoin, of } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { PaginatorModule } from 'primeng/paginator';
import { DialogModule } from 'primeng/dialog';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { CheckboxModule } from 'primeng/checkbox';
import { TreeModule } from 'primeng/tree';
import { Client, AssignPermissionDto, CreateRoleDto, UpdateRoleDto } from '../../api/client';
import { AuthService } from '../../services/auth.service';
import { PermissionService } from '../../services/permission.service';
import { PaginatedResultDto, RoleListDto } from '../../models/dto.models';

@Component({
  selector: 'app-role-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    TagModule,
    ToastModule,
    PaginatorModule,
    DialogModule,
    ToggleSwitchModule,
    CheckboxModule,
    TreeModule
  ],
  providers: [MessageService],
  template: `
    <div class="card">
      <div class="flex justify-content-between align-items-center mb-4">
        <h5>إدارة الأدوار</h5>
        <div class="flex gap-2">
          <p-select 
            [options]="statusOptions" 
            optionLabel="label"
            optionValue="value"
            [(ngModel)]="selectedStatus" 
            name="statusFilter"
            placeholder="الحالة"
            (onChange)="loadRoles()"
            styleClass="w-8rem">
          </p-select>
          <input 
            pInputText
            [(ngModel)]="searchTerm" 
            name="roleSearch"
            placeholder="بحث..."
            (keyup.enter)="loadRoles()"
            class="w-12rem" />
          <p-button 
            label="بحث" 
            icon="pi pi-search" 
            (click)="loadRoles()">
          </p-button>
          <p-button 
            label="دور جديد" 
            icon="pi pi-plus" 
            (click)="openCreateRoleDialog()"
            [disabled]="!canCreateRoles()">
          </p-button>
        </div>
      </div>

      <p-table 
        [value]="roles" 
        [loading]="loading"
        [paginator]="true"
        [rows]="pageSize"
        [totalRecords]="totalRecords"
        [first]="first"
        (onPage)="onPageChange($event)"
        [rowsPerPageOptions]="[10, 20, 50]"
        responsiveLayout="scroll">
        
        <ng-template pTemplate="header">
          <tr>
            <th>المعرف</th>
            <th>اسم الدور</th>
            <th>الوصف</th>
            <th>الحالة</th>
            <th>الصلاحيات</th>
            <th>تاريخ الإنشاء</th>
            <th>الإجراءات</th>
          </tr>
        </ng-template>

        <ng-template pTemplate="body" let-role>
          <tr>
            <td>{{ role.id }}</td>
            <td>{{ role.name }}</td>
            <td>{{ role.description }}</td>
            <td>
              <p-tag 
                [value]="role.isActive ? 'نشط' : 'غير نشط'" 
                [severity]="role.isActive ? 'success' : 'danger'">
              </p-tag>
            </td>
            <td>
              <div class="flex flex-wrap gap-1">
                <p-tag 
                  *ngFor="let permission of role.permissions" 
                  [value]="permission" 
                  severity="info"
                  styleClass="text-xs">
                </p-tag>
              </div>
            </td>
            <td>{{ role.createdAt | date:'yyyy/MM/dd' }}</td>
            <td>
              <div class="flex gap-2">
                <p-button 
                  icon="pi pi-key" 
                  size="small"
                  severity="info"
                  label="تعيين صلاحيات"
                  (click)="openAssignPermissionDialog(role)"
                  [disabled]="!canAssignPermissions()">
                </p-button>
                <p-button 
                  icon="pi pi-pencil" 
                  size="small"
                  severity="secondary"
                  (click)="openEditRoleDialog(role)"
                  [disabled]="!canUpdateRoles()">
                </p-button>
              </div>
            </td>
          </tr>
        </ng-template>
      </p-table>
    </div>

    <!-- Create/Edit Role Dialog -->
    <p-dialog
  [(visible)]="roleDialogVisible"
  [modal]="true"
  [draggable]="false"
  [resizable]="false"
  [closable]="true"
  [dismissableMask]="true"
  [style]="{ width: '480px' }"
  [contentStyle]="{ padding: '1.5rem' }"
  [header]="isEditMode ? '✏️ تعديل الدور' : '➕ إنشاء دور جديد'">

  <form class="p-fluid">

    <!-- Role Name -->
    <div class="field mb-4">
      <label for="roleName" class="block font-medium mb-2">
        اسم الدور <span class="text-red-500">*</span>
      </label>

      <input
        pInputText
        id="roleName"
        name="roleName"
        required
        maxlength="50"
        [(ngModel)]="roleForm.name"
        #roleNameModel="ngModel"
        placeholder="مثال: مدير النظام"
        class="w-full" />

      <small class="p-error block mt-1"
        *ngIf="roleNameModel.invalid && (roleNameModel.dirty || roleNameModel.touched)">
        اسم الدور مطلوب
      </small>
    </div>

    <!-- Role Description -->
    <div class="field mb-4">
      <label for="roleDescription" class="block font-medium mb-2">
        وصف الدور
      </label>

      <textarea
        pInputTextarea
        id="roleDescription"
        name="roleDescription"
        rows="3"
        [(ngModel)]="roleForm.description"
        placeholder="اكتب وصفًا مختصرًا للدور (اختياري)"
        class="w-full">
      </textarea>
    </div>

    <!-- Active Switch -->
    <div class="field flex align-items-center gap-3">
      <label for="roleActive" class="font-medium">
        حالة الدور
      </label>

      <p-toggleswitch
        id="roleActive"
        name="roleActive"
        [(ngModel)]="roleForm.isActive">
      </p-toggleswitch>

      <span class="text-sm text-600">
        {{ roleForm.isActive ? 'نشط' : 'غير نشط' }}
      </span>
    </div>

  </form>

  <!-- Footer -->
  <ng-template pTemplate="footer">
    <div class="flex justify-content-end gap-2">
      <p-button
        label="إلغاء"
        icon="pi pi-times"
        severity="secondary"
        (click)="roleDialogVisible = false">
      </p-button>

      <p-button
        [label]="isEditMode ? 'تحديث الدور' : 'إنشاء الدور'"
        icon="pi pi-check"
        (click)="saveRole()"
        [loading]="savingRole">
      </p-button>
    </div>
  </ng-template>

</p-dialog>


    <!-- Assign Permission Dialog -->
    <p-dialog 
      [(visible)]="permissionDialogVisible" 
      header="تعيين صلاحيات للدور" 
      [modal]="true" 
      [style]="{width: '500px'}"
      [closable]="true">
      
      <div class="field">
        <label>اختر الصلاحيات</label>
        <p-tree 
          [value]="permissionTreeNodes" 
          [(selection)]="selectedTreeNodes"
          selectionMode="checkbox"
          [style]="{'height': '300px', 'overflow': 'auto'}">
        </p-tree>
      </div>

      <ng-template pTemplate="footer">
        <p-button 
          label="إلغاء" 
          icon="pi pi-times" 
          (click)="permissionDialogVisible = false" 
          severity="secondary">
        </p-button>
        <p-button 
          label="حفظ الصلاحيات" 
          icon="pi pi-check" 
          (click)="savePermissions()" 
          [loading]="savingPermissions">
        </p-button>
      </ng-template>
    </p-dialog>

    <p-toast></p-toast>
  `
})
export class RoleManagementComponent implements OnInit {
  roles: RoleListDto[] = [];
  loading = false;
  totalRecords = 0;
  first = 0;
  pageSize = 10;
  searchTerm = '';
  selectedStatus = '';

  roleDialogVisible = false;
  permissionDialogVisible = false;
  isEditMode = false;
  selectedRole: RoleListDto | null = null;
  savingRole = false;
  savingPermissions = false;

  roleForm = {
    name: '',
    description: '',
    isActive: true
  };

  availablePermissions: any[] = [];
  selectedPermissions: { [key: string]: boolean } = {};
  permissionTreeNodes: any[] = [];
  selectedTreeNodes: any[] = [];

  statusOptions = [
    { label: 'الكل', value: '' },
    { label: 'نشط', value: 'active' },
    { label: 'غير نشط', value: 'inactive' }
  ];

  constructor(
    private client: Client,
    private messageService: MessageService,
    private authService: AuthService,
    private permissionService: PermissionService
  ) {}

  ngOnInit() {
    this.loadRoles();
    this.loadPermissions();
  }

  loadRoles() {
    this.loading = true;
    const params = {
      pageNumber: Math.floor(this.first / this.pageSize) + 1,
      pageSize: this.pageSize,
      searchTerm: this.searchTerm || undefined,
      status: this.selectedStatus || undefined,
      sortBy: 'name',
      sortDescending: false
    };

    this.client.paginated(
      params.pageNumber,
      params.pageSize,
      params.searchTerm,
      params.sortBy,
      params.sortDescending,
      params.status,
      undefined // reviewStatus
    ).subscribe({
      next: (result: any) => {
        // Parse the response to get the paginated result
        const paginatedResult: PaginatedResultDto<RoleListDto> = result;
        this.roles = paginatedResult.data || [];
        this.totalRecords = paginatedResult.totalCount;
        this.loading = false;
      },
      error: (error: any) => {
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل الأدوار'
        });
        this.loading = false;
      }
    });
  }

  loadPermissions() {
    // Load permissions from API
    this.client.permissionsAll().subscribe({
      next: (permissions) => {
        this.availablePermissions = permissions;
        this.buildPermissionTree();
      },
      error: (error) => {
        console.error('Failed to load permissions:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل الصلاحيات'
        });
      }
    });
  }

  buildPermissionTree() {
    const groupedPermissions: { [key: string]: any[] } = {};
    
    // Group permissions by their module (before the dot)
    this.availablePermissions.forEach(permission => {
      const [module, action] = permission.name.split('.');
      if (!groupedPermissions[module]) {
        groupedPermissions[module] = [];
      }
      groupedPermissions[module].push({
        key: permission.name,
        label: `${action} - ${permission.description}`,
        data: permission.name,
        selectable: true,
        permissionId: permission.id
      });
    });
    
    // Build tree nodes
    this.permissionTreeNodes = Object.keys(groupedPermissions).map(module => ({
      key: module,
      label: this.getModuleDisplayName(module),
      data: module,
      selectable: false,
      children: groupedPermissions[module]
    }));
  }
  
  getModuleDisplayName(module: string): string {
    const moduleNames: { [key: string]: string } = {
      'users': 'المستخدمين',
      'roles': 'الأدوار',
      'customers': 'العملاء',
      'accounts': 'الحسابات'
    };
    return moduleNames[module] || module;
  }

  onPageChange(event: any) {
    this.first = event.first;
    this.pageSize = event.rows;
    this.loadRoles();
  }

  openCreateRoleDialog() {
    this.isEditMode = false;
    this.roleForm = {
      name: '',
      description: '',
      isActive: true
    };
    this.roleDialogVisible = true;
  }

  openEditRoleDialog(role: RoleListDto) {
    this.isEditMode = true;
    this.selectedRole = role;
    this.roleForm = {
      name: role.name,
      description: role.description,
      isActive: role.isActive
    };
    this.roleDialogVisible = true;
  }

  saveRole() {
    if (!this.roleForm.name.trim()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تحذير',
        detail: 'يرجى إدخال اسم الدور'
      });
      return;
    }

    this.savingRole = true;

    if (this.isEditMode && this.selectedRole) {
      const updateRoleDto = new UpdateRoleDto({
        name: this.roleForm.name,
        description: this.roleForm.description,
        isActive: this.roleForm.isActive
      });

      this.client.rolePUT(this.selectedRole.id, updateRoleDto).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'نجاح',
            detail: 'تم تحديث الدور بنجاح'
          });
          this.roleDialogVisible = false;
          this.loadRoles();
          this.savingRole = false;
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: 'فشل تحديث الدور'
          });
          this.savingRole = false;
        }
      });
    } else {
      const createRoleDto = new CreateRoleDto({
        name: this.roleForm.name,
        description: this.roleForm.description
      });

      this.client.rolePOST(createRoleDto).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'نجاح',
            detail: 'تم إنشاء الدور بنجاح'
          });
          this.roleDialogVisible = false;
          this.loadRoles();
          this.savingRole = false;
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: 'فشل إنشاء الدور'
          });
          this.savingRole = false;
        }
      });
    }
  }

  openAssignPermissionDialog(role: RoleListDto) {
    this.selectedRole = role;
    
    // Build tree nodes first
    this.buildPermissionTree();
    
    // Initialize selected tree nodes based on current role permissions
    this.selectedTreeNodes = [];
    role.permissions.forEach(permissionName => {
      const node = this.findTreeNode(this.permissionTreeNodes, permissionName);
      if (node) {
        this.selectedTreeNodes.push(node);
      }
    });

    this.permissionDialogVisible = true;
  }
  
  findTreeNode(nodes: any[], permissionName: string): any {
    for (const node of nodes) {
      if (node.data === permissionName) {
        return node;
      }
      if (node.children) {
        const found = this.findTreeNode(node.children, permissionName);
        if (found) return found;
      }
    }
    return null;
  }

  savePermissions() {
    if (!this.selectedRole) return;

    this.savingPermissions = true;
    
    // Build lookup map from available permissions: name -> id
    const permissionIdByName: Record<string, number> = {} as any;
    this.availablePermissions.forEach((p: any) => {
      if (p && typeof p.name === 'string' && typeof p.id === 'number') {
        permissionIdByName[p.name] = p.id;
      }
    });

    // Current permissions on the role (ids via name lookup)
    const currentIds = new Set(
      (this.selectedRole.permissions || [])
        .map(name => permissionIdByName[name])
        .filter((id): id is number => typeof id === 'number')
    );

    // Selected permissions from tree (ids)
    const selectedIds = new Set(
      this.selectedTreeNodes
        .filter((node: any) => node.selectable)
        .map((node: any) => node.permissionId)
        .filter((id: any): id is number => typeof id === 'number')
    );

    // Compute differences
    const toAdd: number[] = [];
    const toRemove: number[] = [];
    selectedIds.forEach(id => { if (!currentIds.has(id)) toAdd.push(id); });
    currentIds.forEach(id => { if (!selectedIds.has(id)) toRemove.push(id); });

    // Build requests
    const addRequests = toAdd.map(id => {
      const dto = new AssignPermissionDto({ roleId: this.selectedRole!.id, permissionId: id });
      return this.client.assignPermission(dto);
    });
    const removeRequests = toRemove.map(id => {
      const dto = new AssignPermissionDto({ roleId: this.selectedRole!.id, permissionId: id });
      return this.client.removePermission(dto);
    });

    const requests = [...addRequests, ...removeRequests];

    // If nothing to change, finish gracefully
    const exec$: any = requests.length ? forkJoin(requests) : of(null);

    exec$.subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'نجاح',
          detail: 'تم حفظ الصلاحيات بنجاح'
        });
        this.permissionDialogVisible = false;
        this.loadRoles();
        this.savingPermissions = false;
      },
      error: (error: any) => {
        console.error('Failed to save permissions', { error, toAdd, toRemove });
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل حفظ الصلاحيات'
        });
        this.savingPermissions = false;
      }
    });
  }

  canCreateRoles(): boolean {
    return this.permissionService.canCreateRoles();
  }

  canUpdateRoles(): boolean {
    return this.permissionService.canUpdateRoles();
  }

  canAssignPermissions(): boolean {
    return this.permissionService.canAssignPermissions();
  }
}
