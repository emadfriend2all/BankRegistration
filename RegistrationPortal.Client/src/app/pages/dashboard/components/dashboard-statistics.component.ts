import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService, DashboardStatisticsDto } from '../../../services/dashboard.service';

@Component({
  selector: 'app-dashboard-statistics',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4">
      <!-- طلبات إنشاء حساب -->
      <div class="col-span-1">
        <div class="card mb-0">
          <div class="flex justify-between mb-4">
            <div>
              <span class="block text-muted-color font-medium mb-4">طلبات إنشاء حساب</span>
              <div class="text-primary font-medium text-xl">{{ statistics?.newCustomersCount || 0 }}</div>
            </div>
            <div class="flex items-center justify-center bg-blue-100 dark:bg-blue-400/10 rounded-border" style="width: 2.5rem; height: 2.5rem">
              <i class="pi pi-user-plus text-blue-500 text-xl!"></i>
            </div>
          </div>
          <span class="text-primary font-medium">{{ statistics?.newCustomersPercentage?.toFixed(1) || '0.0' }}% </span>
          <span class="text-muted-color">من الإجمالي</span>
        </div>
      </div>

      <!-- طلبات التعديل -->
      <div class="col-span-1">
        <div class="card mb-0">
          <div class="flex justify-between mb-4">
            <div>
              <span class="block text-muted-color font-medium mb-4">طلبات التعديل</span>
              <div class="text-primary font-medium text-xl">{{ statistics?.updateCustomersCount || 0 }}</div>
            </div>
            <div class="flex items-center justify-center bg-green-100 dark:bg-green-400/10 rounded-border" style="width: 2.5rem; height: 2.5rem">
              <i class="pi pi-user-edit text-green-500 text-xl!"></i>
            </div>
          </div>
          <span class="text-primary font-medium">{{ statistics?.updateCustomersPercentage?.toFixed(1) || '0.0' }}% </span>
          <span class="text-muted-color">من الإجمالي</span>
        </div>
      </div>

      <!-- مجموع الطلبات -->
      <div class="col-span-1">
        <div class="card mb-0">
          <div class="flex justify-between mb-4">
            <div>
              <span class="block text-muted-color font-medium mb-4">مجموع الطلبات</span>
              <div class="text-primary font-medium text-xl">{{ statistics?.totalCustomersCount || 0 }}</div>
            </div>
            <div class="flex items-center justify-center bg-purple-100 dark:bg-purple-400/10 rounded-border" style="width: 2.5rem; height: 2.5rem">
              <i class="pi pi-users text-purple-500 text-xl!"></i>
            </div>
          </div>
          <span class="text-muted-color">إجمالي العملاء</span>
        </div>
      </div>

      <!-- الطلبات في الإنتظار -->
      <div class="col-span-1">
        <div class="card mb-0">
          <div class="flex justify-between mb-4">
            <div>
              <span class="block text-muted-color font-medium mb-4">الطلبات في الإنتظار</span>
              <div class="text-primary font-medium text-xl">{{ statistics?.pendingReviewsCount || 0 }}</div>
            </div>
            <div class="flex items-center justify-center bg-orange-100 dark:bg-orange-400/10 rounded-border" style="width: 2.5rem; height: 2.5rem">
              <i class="pi pi-clock text-orange-500 text-xl!"></i>
            </div>
          </div>
          <span class="text-primary font-medium">{{ statistics?.pendingReviewsPercentage?.toFixed(1) || '0.0' }}% </span>
          <span class="text-muted-color">من الإجمالي</span>
        </div>
      </div>

      <!-- الطلبات المصدقة -->
      <div class="col-span-1">
        <div class="card mb-0">
          <div class="flex justify-between mb-4">
            <div>
              <span class="block text-muted-color font-medium mb-4">الطلبات المصدقة</span>
              <div class="text-primary font-medium text-xl">{{ statistics?.approvedReviewsCount || 0 }}</div>
            </div>
            <div class="flex items-center justify-center bg-teal-100 dark:bg-teal-400/10 rounded-border" style="width: 2.5rem; height: 2.5rem">
              <i class="pi pi-check-circle text-teal-500 text-xl!"></i>
            </div>
          </div>
          <span class="text-primary font-medium">{{ statistics?.approvedReviewsPercentage?.toFixed(1) || '0.0' }}% </span>
          <span class="text-muted-color">من الإجمالي</span>
        </div>
      </div>

      <!-- الطلبات المرفوضة -->
      <div class="col-span-1">
        <div class="card mb-0">
          <div class="flex justify-between mb-4">
            <div>
              <span class="block text-muted-color font-medium mb-4">الطلبات المرفوضة</span>
              <div class="text-primary font-medium text-xl">{{ statistics?.rejectedReviewsCount || 0 }}</div>
            </div>
            <div class="flex items-center justify-center bg-red-100 dark:bg-red-400/10 rounded-border" style="width: 2.5rem; height: 2.5rem">
              <i class="pi pi-times-circle text-red-500 text-xl!"></i>
            </div>
          </div>
          <span class="text-primary font-medium">{{ statistics?.rejectedReviewsPercentage?.toFixed(1) || '0.0' }}% </span>
          <span class="text-muted-color">من الإجمالي</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
  `]
})
export class DashboardStatisticsComponent implements OnInit {
  statistics: DashboardStatisticsDto | null = null;
  loading = true;
  error: string | null = null;

  constructor(private dashboardService: DashboardService) {}

  ngOnInit() {
    this.loadStatistics();
  }

  private loadStatistics() {
    this.loading = true;
    this.error = null;

    this.dashboardService.getDashboardStatistics().subscribe({
      next: (data) => {
        this.statistics = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading dashboard statistics:', err);
        this.error = 'فشل في تحميل الإحصائيات';
        this.loading = false;
      }
    });
  }
}
