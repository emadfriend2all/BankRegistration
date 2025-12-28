import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { PaginatorModule } from 'primeng/paginator';
import { DialogModule } from 'primeng/dialog';
import { Client, AssignRoleDto, RegisterDto } from '../../api/client';
import { FormOptionsService } from '../../shared/form-options.service';
import { AuthService } from '../../services/auth.service';
import { PermissionService } from '../../services/permission.service';
import { PaginatedResultDto, UserListDto } from '../../models/dto.models';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    TagModule,
    ToastModule,
    PaginatorModule,
    DialogModule
  ],
  providers: [MessageService],
  templateUrl: `./user-management.component.html`
})
export class UserManagementComponent implements OnInit {
  users: UserListDto[] = [];
  loading = false;
  totalRecords = 0;
  first = 0;
  pageSize = 10;
  searchTerm = '';
  selectedStatus = '';
  showPassword = false;

  createUserDialogVisible = false;
  savingUser = false;
  userForm = {
    username: '',
    email: '',
    password: '',
    branch: '',
    firstName: '',
    lastName: ''
  };

  assignRoleDialogVisible = false;
  selectedUser: UserListDto | null = null;
  availableRoles: any[] = [];
  selectedRole: any = null;
  assigningRole = false;

  statusOptions = [
    { label: 'الكل', value: '' },
    { label: 'نشط', value: 'active' },
    { label: 'غير نشط', value: 'inactive' }
  ];

  branchOptions: any[] = [];

  constructor(
    private client: Client,
    private messageService: MessageService,
    private authService: AuthService,
    private permissionService: PermissionService,
    private formOptions: FormOptionsService
  ) {}

  ngOnInit() {
    this.loadUsers();
    this.loadRoles();
    this.branchOptions = this.formOptions.branches;
  }

  loadUsers() {
    this.loading = true;
    const params = {
      pageNumber: Math.floor(this.first / this.pageSize) + 1,
      pageSize: this.pageSize,
      searchTerm: this.searchTerm || undefined,
      status: this.selectedStatus || undefined,
      sortBy: 'username',
      sortDescending: false
    };

    this.client.paginated2(
      params.pageNumber,
      params.pageSize,
      params.searchTerm,
      params.sortBy,
      params.sortDescending,
      params.status,
      undefined // reviewStatus
    ).subscribe({
      next: (res: any) => {
        // Support both direct DTO and HttpResponse-wrapped bodies
        const payload = res?.body ?? res;
        const paginatedResult: PaginatedResultDto<UserListDto> | null = payload ?? null;
        this.users = paginatedResult?.data ?? [];
        this.totalRecords = paginatedResult?.totalCount ?? 0;
        this.loading = false;
      },
      error: (error: any) => {
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل المستخدمين'
        });
        this.loading = false;
      }
    });
  }

  loadRoles() {
    this.client.paginated(1, 100, undefined, 'name', false, 'active', undefined).subscribe({
      next: (res: any) => {
        const payload = res?.body ?? res;
        this.availableRoles = payload?.data ?? [];
      },
      error: (error: any) => {
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل الأدوار'
        });
      }
    });
  }

  openCreateUserDialog() {
    this.userForm = { username: '', email: '', password: '', branch: '', firstName: '', lastName: '' };
    this.createUserDialogVisible = true;
  }

  saveUser() {
    if (!this.userForm.username.trim() || !this.userForm.email.trim() || !this.userForm.password.trim()) {
      this.messageService.add({ severity: 'warn', summary: 'تحذير', detail: 'الرجاء إدخال اسم المستخدم والبريد وكلمة المرور' });
      return;
    }
    this.savingUser = true;
    const dto = new RegisterDto({
      username: this.userForm.username,
      email: this.userForm.email,
      password: this.userForm.password,
      branch: this.userForm.branch || undefined,
      firstName: this.userForm.firstName || undefined,
      lastName: this.userForm.lastName || undefined
    });

    this.client.userPOST(dto).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم إنشاء المستخدم بنجاح' });
        this.createUserDialogVisible = false;
        this.loadUsers();
        this.savingUser = false;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل إنشاء المستخدم' });
        this.savingUser = false;
      }
    });
  }

  onPageChange(event: any) {
    this.first = event.first;
    this.pageSize = event.rows;
    this.loadUsers();
  }

  openAssignRoleDialog(user: UserListDto) {
    this.selectedUser = user;
    this.selectedRole = null;
    this.assignRoleDialogVisible = true;
  }

  assignRole() {
    if (!this.selectedUser || !this.selectedRole) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تحذير',
        detail: 'يرجى اختيار دور'
      });
      return;
    }

    this.assigningRole = true;
    const assignRoleDto = new AssignRoleDto({
      userId: this.selectedUser.id,
      roleId: this.selectedRole
    });

    this.client.assignRole(assignRoleDto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'نجاح',
          detail: 'تم تعيين الدور بنجاح'
        });
        this.assignRoleDialogVisible = false;
        this.loadUsers(); // Refresh the list
        this.assigningRole = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تعيين الدور'
        });
        this.assigningRole = false;
      }
    });
  }

  canAssignRoles(): boolean {
    return this.permissionService.canAssignRoles();
  }
}
