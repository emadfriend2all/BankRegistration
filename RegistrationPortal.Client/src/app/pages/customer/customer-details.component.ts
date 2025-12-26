import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { ImageModule } from 'primeng/image';
import { GalleriaModule } from 'primeng/galleria';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { PanelModule } from 'primeng/panel';
import { FieldsetModule } from 'primeng/fieldset';
import { SkeletonModule } from 'primeng/skeleton';
import { MessageModule } from 'primeng/message';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToolbarModule } from 'primeng/toolbar';
import { TooltipModule } from 'primeng/tooltip';
import { ChipModule } from 'primeng/chip';
import { BadgeModule } from 'primeng/badge';
import { DialogModule } from 'primeng/dialog';
import { SelectButtonModule } from 'primeng/selectbutton';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { CustomerService } from '../../services/customer.service';
import { AuthService } from '../../services/auth.service';
import { GetCustMastByIdDto, AccountMastDto, CustomerDocumentDto, UpdateCustomerReviewDto } from '../../api/client';

@Component({
  selector: 'app-customer-details',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardModule,
    ButtonModule,
    ImageModule,
    GalleriaModule,
    TableModule,
    TagModule,
    DividerModule,
    PanelModule,
    FieldsetModule,
    SkeletonModule,
    MessageModule,
    ProgressSpinnerModule,
    ToolbarModule,
    TooltipModule,
    ChipModule,
    BadgeModule,
    DialogModule,
    SelectButtonModule,
    InputTextModule,
    ToastModule
  ],
  templateUrl: './customer-details.component.html',
  styleUrls: ['./customer-details.component.css']
})
export class CustomerDetailsComponent implements OnInit {
  customer: GetCustMastByIdDto | null = null;
  loading: boolean = false;
  error: string | null = null;
  
  // Document galleries
  identityDocuments: CustomerDocumentDto[] = [];
  otherDocuments: CustomerDocumentDto[] = [];
  identityImages: any[] = [];
  
  // Review functionality
  confirmDialogVisible: boolean = false;
  pendingReviewStatus: string = '';
  reviewLoading: boolean = false;
  
  // Galleria responsive options
  responsiveOptions: any[] = [
    {
      breakpoint: '1024px',
      numVisible: 5
    },
    {
      breakpoint: '768px',
      numVisible: 3
    },
    {
      breakpoint: '560px',
      numVisible: 1
    }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private customerService: CustomerService,
    public authService: AuthService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadCustomerDetails();
  }

  loadCustomerDetails(): void {
    this.loading = true;
    this.error = null;
    
    const customerId = this.route.snapshot.paramMap.get('id');
    console.log('Customer ID from route:', customerId);
    
    if (!customerId) {
      this.error = 'رقم العميل مطلوب';
      this.loading = false;
      return;
    }

    // Use the new GetCustMastById endpoint
    this.customerService.getCustomerByIdFromApi(customerId).subscribe({
      next: (customer) => {
        if (customer) {
          this.customer = customer;
          this.organizeDocuments();
        } else {
          this.error = 'لم يتم العثور على العميل';
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'حدث خطأ أثناء تحميل بيانات العميل';
        this.loading = false;
        console.error('Error loading customer details:', err);
      }
    });
  }

  organizeDocuments(): void {
    if (!this.customer || !this.customer.customerDocuments) return;
    
    // Separate identity documents (Passport, National ID) from other documents
    this.identityDocuments = this.customer.customerDocuments.filter(doc => 
      doc.documentType === 'Identification' || 
      doc.documentType === 'NationalId' ||
      doc.documentType === 'PersonalImage' ||
      doc.documentType === 'ImageFortheRequesterHoldingTheID'
    );
    
    // Transform identity documents for galleria
    this.identityImages = this.identityDocuments.map(doc => ({
      itemImageSrc: doc.fileUrl,
      thumbnailImageSrc: doc.fileUrl,
      alt: doc.originalFileName,
      title: this.getDocumentTypeLabel(doc.documentType)
    }));
    
    this.otherDocuments = this.customer.customerDocuments.filter(doc => 
      !this.identityDocuments.includes(doc)
    );
  }

  getPersonalImage(): CustomerDocumentDto | null {
    return this.customer?.customerDocuments?.find(doc => 
      doc.documentType === 'PersonalImage'
    ) ?? null;
  }

  getPassportDocument(): CustomerDocumentDto | null {
    return this.customer?.customerDocuments?.find(doc => 
      doc.documentType === 'Identification'
    ) ?? null;
  }

  getNationalIdDocument(): CustomerDocumentDto | null {
    return this.customer?.customerDocuments?.find(doc => 
      doc.documentType === 'NationalId'
    ) ?? null;
  }

  getDocumentTypeLabel(documentType: string | undefined): string {
    if (!documentType) return 'غير محدد';
    const labels: { [key: string]: string } = {
      'Identification': 'جواز السفر',
      'NationalId': 'بطاقة الهوية الوطنية',
      'PersonalImage': 'الصورة الشخصية',
      'ImageFortheRequesterHoldingTheID': 'صورة طالب الهوية',
      'SignitureTemplate': 'نموذج التوقيع',
      'HandwrittenRequest': 'الطلب المكتوب بخط اليد',
      'EmploymentCertificate': 'شهادة التوظيف'
    };
    return labels[documentType] || documentType;
  }

  formatDate(dateString: string | Date | undefined): string {
    if (!dateString) return 'غير محدد';
    const date = typeof dateString === 'string' ? new Date(dateString) : dateString;
    return date.toLocaleDateString('ar-SA', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  formatFileSize(bytes: number | undefined): string {
    if (!bytes || bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  getStatusSeverity(status: string | undefined): string {
    if (!status) return 'info';
    switch (status.toLowerCase()) {
      case 'active':
      case 'نشط':
        return 'success';
      case 'inactive':
      case 'غير نشط':
        return 'danger';
      case 'new':
      case 'جديد':
        return 'info';
      default:
        return 'warning';
    }
  }

  getStatusLabel(status: string | undefined): string {
    if (!status) return 'غير محدد';
    const labels: { [key: string]: string } = {
      'active': 'نشط',
      'inactive': 'غير نشط',
      'new': 'جديد',
      'pending': 'قيد الانتظار',
      'suspended': 'معلق'
    };
    return labels[status.toLowerCase()] || status;
  }

  getGenderLabel(gender: string | undefined): string {
    if (!gender) return 'غير محدد';
    return gender.toLowerCase() === 'm' ? 'ذكر' : 'أنثى';
  }

  getMaritalStatusLabel(status: string | undefined): string {
    if (!status) return 'غير محدد';
    const labels: { [key: string]: string } = {
      'married': 'متزوج',
      'single': 'أعزب',
      'divorced': 'مطلق',
      'widowed': 'أرمل'
    };
    return labels[status.toLowerCase()] || status;
  }

  goBack(): void {
    this.router.navigate(['/customers']);
  }

  downloadDocument(document: CustomerDocumentDto): void {
    // In a real implementation, you would create a download endpoint
    console.log('Downloading document:', document.originalFileName);
    // window.open(document.fileUrl, '_blank');
  }

  viewDocument(document: CustomerDocumentDto): void {
    // In a real implementation, you would open the document in a new tab
    console.log('Viewing document:', document.originalFileName);
    // window.open(document.fileUrl, '_blank');
  }

  // Permission check for review functionality
  get canReviewCustomer(): boolean {
    const user = this.authService.currentUserValue;
    if (!user || !this.customer) return false;
    
    // Check if customer has Pending status
    if (this.customer.reviewStatus !== 'Pending') {
      return false;
    }
    
    // Check if user has review permission - temporarily allow all authenticated users for testing
    // TODO: Fix role extraction from API response
    const hasReviewRole = user.role === 'Admin' || user.role === 'Reviewer' || !user.role;
    
    console.log('Review permission check:', {
      hasReviewRole: hasReviewRole,
      customerStatus: this.customer.reviewStatus,
      userRole: user.role,
      userEmail: user.email,
      fullUser: user
    });
    
    return hasReviewRole;
  }

  // Review functionality methods
  approveCustomer(): void {
    if (!this.customer) return;
    
    this.pendingReviewStatus = 'Approved';
    console.log('approveCustomer called, setting pendingReviewStatus to:', this.pendingReviewStatus);
    this.confirmDialogVisible = true;
  }

  rejectCustomer(): void {
    if (!this.customer) return;
    
    this.pendingReviewStatus = 'Rejected';
    console.log('rejectCustomer called, setting pendingReviewStatus to:', this.pendingReviewStatus);
    this.confirmDialogVisible = true;
  }

  confirmReview(): void {
    this.updateReviewStatus(this.pendingReviewStatus);
  }

  cancelReview(): void {
    this.confirmDialogVisible = false;
    this.pendingReviewStatus = '';
  }

  getConfirmMessage(): string {
    console.log('getConfirmMessage called, pendingReviewStatus:', this.pendingReviewStatus);
    if (this.pendingReviewStatus === 'Approved') {
      return 'هل أنت متأكد من أنك تريد الموافقة على هذا العميل؟ سيتم تحديث حالة المراجعة إلى "موافق عليه".';
    } else if (this.pendingReviewStatus === 'Rejected') {
      return 'هل أنت متأكد من أنك تريد رفض هذا العميل؟ سيتم تحديث حالة المراجعة إلى "مرفوض".';
    }
    return 'هل أنت متأكد من إتمام هذه المراجعة؟';
  }

  getConfirmHeader(): string {
    if (this.pendingReviewStatus === 'Approved') {
      return 'تأكيد الموافقة';
    } else if (this.pendingReviewStatus === 'Rejected') {
      return 'تأكيد الرفض';
    }
    return 'تأكيد المراجعة';
  }

  updateReviewStatus(status: string): void {
    if (!this.customer) return;

    this.reviewLoading = true;

    const reviewDto = new UpdateCustomerReviewDto({
      customerId: this.customer.id,
      reviewStatus: status
    });

    // Call the review endpoint from the API client
    this.customerService.reviewCustomer(reviewDto).subscribe({
      next: (success) => {
        this.reviewLoading = false;
        
        if (success) {
          // Update local customer data
          if (this.customer) {
            this.customer.reviewStatus = status;
            this.customer.reviewedBy = this.authService.currentUserValue?.name || 'Unknown';
          }
          
          this.messageService.add({
            severity: 'success',
            summary: 'نجاح',
            detail: `تم ${status === 'Approved' ? 'الموافقة على' : 'رفض'} العميل بنجاح`
          });
          
          this.confirmDialogVisible = false;
          this.pendingReviewStatus = '';
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: 'فشل تحديث حالة المراجعة'
          });
        }
      },
      error: (err) => {
        this.reviewLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'حدث خطأ أثناء تحديث حالة المراجعة'
        });
        console.error('Review update error:', err);
      }
    });
  }

  getReviewStatusLabel(status: string | undefined): string {
    if (!status) return 'غير محدد';
    const labels: { [key: string]: string } = {
      'Approved': 'Approved',
      'Rejected': 'Rejected',
    };
    return labels[status] || status;
  }

  getReviewStatusSeverity(status: string | undefined): string {
    if (!status) return 'info';
    switch (status) {
      case 'Approved':
        return 'success';
      case 'Rejected':
        return 'danger';
      case 'Pending':
        return 'warning';
      case 'NeedsInfo':
        return 'info';
      default:
        return 'secondary';
    }
  }
}
