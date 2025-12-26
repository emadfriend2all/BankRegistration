import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { PaginatorModule } from 'primeng/paginator';
import { InputTextModule } from 'primeng/inputtext';
import { InputGroupModule } from 'primeng/inputgroup';
import { InputGroupAddonModule } from 'primeng/inputgroupaddon';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressBarModule } from 'primeng/progressbar';
import { SkeletonModule } from 'primeng/skeleton';
import { MessageModule } from 'primeng/message';
import { RippleModule } from 'primeng/ripple';
import { Router } from '@angular/router';
import { CustomerService, PaginatedResultDto } from '../../services/customer.service';
import { AuthService, User } from '../../services/auth.service';
import { PaginationParameters } from '../../models/pagination.model';
import { CustMastDto, AccountMastDto } from '../../api/client';

@Component({
  selector: 'app-view-all-customers',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    PaginatorModule,
    InputTextModule,
    InputGroupModule,
    InputGroupAddonModule,
    SelectModule,
    ButtonModule,
    CardModule,
    TagModule,
    TooltipModule,
    ProgressBarModule,
    SkeletonModule,
    MessageModule,
    RippleModule
  ],
  templateUrl: './view-all-customers.component.html',
  styleUrls: ['./view-all-customers.component.css']
})
export class ViewAllCustomersComponent implements OnInit {
  customers: CustMastDto[] = [];
  filteredCustomers: CustMastDto[] = [];
  loading: boolean = false;
  totalRecords: number = 0;
  first: number = 0;
  rows: number = 10;
  globalFilter: string = '';
  selectedStatus: string = '';
  selectedReviewStatus: string = '';
  sortField: string = 'custCName';
  sortOrder: number = 1;
  currentUser: User | null = null;
  userRole: string | null = null;

  // Add Math property for template access
  Math = Math;

  // Filter options
  statusOptions = [
    { label: 'اختر الحالة', value: '' },
    { label: 'جديد', value: 'New' },
    { label: 'تحديث', value: 'Update' }
  ];

  reviewStatusOptions: Array<{label: string, value: string}> = [];

  // All available columns for filtering (including hidden ones)
  allColumns = [
    { field: 'custCName', label: 'الاسم', searchable: true },
    { field: 'custCFname', label: 'الاسم الأول', searchable: true },
    { field: 'custCSname', label: 'اسم الأب', searchable: true },
    { field: 'custCTname', label: 'اسم الجد', searchable: true },
    { field: 'branchCCode', label: 'الفرع', searchable: true },
    { field: 'custIId', label: 'رقم الهوية', searchable: true },
    { field: 'mobileCNo', label: 'الجوال', searchable: true },
    { field: 'emailCAdd', label: 'البريد الإلكتروني', searchable: true },
    { field: 'idCType', label: 'نوع الهوية', searchable: true },
    { field: 'idCNo', label: 'رقم الهوية', searchable: true },
    { field: 'idCNo2', label: 'رقم الهوية 2', searchable: true },
    { field: 'custCSex', label: 'الجنس', searchable: true },
    { field: 'custCMaritalsts', label: 'الحالة الاجتماعية', searchable: true },
    { field: 'custCReligion', label: 'الديانة', searchable: true },
    { field: 'custCCaste', label: 'الطائفة', searchable: true },
    { field: 'custCPadd1', label: 'العنوان', searchable: true },
    { field: 'custCPCity', label: 'المدينة', searchable: true },
    { field: 'custCOccupation', label: 'المهنة', searchable: true },
    { field: 'custCNationality', label: 'الجنسية', searchable: true },
    { field: 'status', label: 'الحالة', searchable: true },
    { field: 'custDEntrydt', label: 'تاريخ الإدخال', searchable: true, isDate: true },
    { field: 'custDBdate', label: 'تاريخ الميلاد', searchable: true, isDate: true }
  ];

  sortOptions = [
    { label: 'الاسم', value: 'custCName' },
    { label: 'الفرع', value: 'branchCCode' },
    { label: 'رقم الهوية', value: 'custIId' },
    { label: 'الجوال', value: 'mobileCNo' },
    { label: 'نوع الهوية', value: 'idCType' },
    { label: 'رقم الهوية', value: 'idCNo' },
    { label: 'الحالة', value: 'status' }
  ];

  constructor(private customerService: CustomerService, private router: Router, private authService: AuthService) {}

  ngOnInit(): void {
    this.loadUserData();
    this.loadCustomers();
  }

  loadUserData(): void {
    this.currentUser = this.authService.currentUserValue;
    this.userRole = this.currentUser?.role || null;
    this.updateFilterOptions();
  }

  updateFilterOptions(): void {
    // Update review status options based on user role
    this.reviewStatusOptions = this.getReviewStatusOptionsForRole(this.userRole);
  }

  getReviewStatusOptionsForRole(role: string | null): Array<{label: string, value: string}> {
    const baseOptions = [
      { label: 'اختر حالة المراجعة', value: '' }
    ];

    if (!role) {
      // If no role, show all options
      return [
        ...baseOptions,
        { label: 'Pending', value: 'Pending' },
          { label: 'Approved', value: 'Approved' },
          { label: 'Rejected', value: 'Rejected' }
      ];
    }

    switch (role) {
      case 'Reviewer':
        // Reviewer can only see Pending
        return [
          ...baseOptions,
          { label: 'Pending', value: 'Pending' }
        ];
      case 'Manager':
        // Manager can see Approved and Rejected only
        return [
          ...baseOptions,
          { label: 'Approved', value: 'Approved' },
          { label: 'Rejected', value: 'Rejected' }
        ];
      case 'Admin':
        // Admin can see all
        return [
          ...baseOptions,
          { label: 'Pending', value: 'Pending' },
          { label: 'Approved', value: 'Approved' },
          { label: 'Rejected', value: 'Rejected' }
        ];
      default:
        // Default to all options for unknown roles
        return [
          ...baseOptions,
          { label: 'Pending', value: 'Pending' },
          { label: 'Approved', value: 'Approved' },
          { label: 'Rejected', value: 'Rejected' }
        ];
    }
  }

  loadCustomers(): void {
    this.loading = true;
    const params: PaginationParameters = {
      pageNumber: Math.floor(this.first / this.rows) + 1,
      pageSize: this.rows,
      searchTerm: this.globalFilter || undefined,
      sortBy: this.sortField,
      sortDescending: this.sortOrder === -1,
      status: this.selectedStatus || undefined,
      reviewStatus: this.selectedReviewStatus || undefined
    };

    this.customerService.getAllCustomers(params).subscribe({
      next: (response: PaginatedResultDto<CustMastDto>) => {
        this.customers = response.data || [];
        this.filteredCustomers = [...this.customers];
        this.totalRecords = this.filteredCustomers.length;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading customers:', error);
        this.customers = [];
        this.filteredCustomers = [];
        this.totalRecords = 0;
        this.loading = false;
      }
    });
  }

  onPageChange(event: any): void {
    this.first = event.first;
    this.rows = event.rows;
    this.loadCustomers();
  }

  onSortChange(event: any): void {
    this.sortField = event.value;
    this.loadCustomers();
  }

  onFilter(): void {
    this.first = 0;
    this.applySmartFilter();
  }

  applySmartFilter(): void {
    if (!this.globalFilter || this.globalFilter.trim() === '') {
      this.filteredCustomers = [...this.customers];
      this.totalRecords = this.filteredCustomers.length;
      return;
    }

    const searchTerm = this.globalFilter.toLowerCase().trim();
    
    // Smart filtering across all columns
    this.filteredCustomers = this.customers.filter(customer => {
      return this.allColumns.some(column => {
        if (!column.searchable) return false;
        
        const value = (customer as any)[column.field];
        if (value === null || value === undefined) return false;
        
        // Handle date fields
        if (column.isDate && value instanceof Date) {
          return value.toLocaleDateString('ar-SA').includes(searchTerm);
        }
        
        // Handle numeric fields
        if (typeof value === 'number') {
          return value.toString().includes(searchTerm);
        }
        
        // Handle string fields
        return value.toString().toLowerCase().includes(searchTerm);
      });
    });
    
    this.totalRecords = this.filteredCustomers.length;
  }

  getFilterSummary(): string {
    if (!this.globalFilter || this.globalFilter.trim() === '') {
      return '';
    }
    
    const matchCount = this.filteredCustomers.length;
    const totalCount = this.customers.length;
    
    return `تم العثور على ${matchCount} من ${totalCount} عميل`;
  }

  clearFilter(): void {
    this.globalFilter = '';
    this.first = 0;
    this.applySmartFilter();
  }

  onStatusFilterChange(): void {
    this.first = 0;
    this.loadCustomers();
  }

  onReviewStatusFilterChange(): void {
    this.first = 0;
    this.loadCustomers();
  }

  clearAllFilters(): void {
    this.globalFilter = '';
    this.selectedStatus = '';
    this.selectedReviewStatus = '';
    this.first = 0;
    this.loadCustomers();
  }

  hasActiveFilters(): boolean {
    return !!(this.globalFilter && this.globalFilter.trim()) || 
           !!(this.selectedStatus) || 
           !!(this.selectedReviewStatus);
  }

  getCustomerStatus(customer: CustMastDto): string {
    // Use the actual status field from CustMast TypeScript interface
    return customer.status || 'Unknown';
  }

  getReviewStatus(customer: CustMastDto): string {
    return customer.reviewStatus || 'Unknown';
  }

  getStatusSeverity(status: string): string {
    switch (status.toLowerCase()) {
      case 'active':
        return 'success';
      case 'inactive':
        return 'danger';
      default:
        return 'info';
    }
  }

  getReviewStatusSeverity(reviewStatus: string): string {
    switch (reviewStatus.toLowerCase()) {
      case 'pending':
        return 'warning';
      case 'approved':
        return 'success';
      case 'rejected':
        return 'danger';
      default:
        return 'info';
    }
  }

  formatPhoneNumber(phone: string | undefined): string {
    if (!phone) return 'N/A';
    // Format phone number for better display
    if (phone.length === 10) {
      return `(${phone.slice(0, 3)}) ${phone.slice(3, 6)}-${phone.slice(6)}`;
    }
    return phone;
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .map(word => word.charAt(0).toUpperCase())
      .join('')
      .slice(0, 2);
  }

  exportCustomers(): void {
    // Implementation for export functionality
    console.log('Exporting customers...');
  }

  viewCustomerDetails(customer: CustMastDto): void {
    console.log('Navigating to customer details for:', customer);
    console.log('Customer ID:', customer.id);
    
    if (!customer.id) {
      console.error('Customer ID is undefined');
      return;
    }
    
    this.router.navigate(['/pages/customer', customer.id]);
  }
}
