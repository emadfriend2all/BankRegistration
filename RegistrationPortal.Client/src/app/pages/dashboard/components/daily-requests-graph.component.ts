import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DashboardService, DashboardGraphDataDto, DailyRequestDataDto } from '../../../services/dashboard.service';
import { Chart, ChartConfiguration, ChartType, registerables } from 'chart.js';

@Component({
  selector: 'app-daily-requests-graph',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="bg-white rounded-xl shadow-lg p-6">
      <div class="flex items-center justify-between mb-6">
        <h3 class="text-xl font-bold text-gray-800">إجمالي الطلبات اليومية</h3>
        <div class="flex items-center space-x-2 space-x-reverse">
          <label for="daysSelect" class="text-sm text-gray-600">الفترة:</label>
          <select 
            id="daysSelect"
            [(ngModel)]="selectedDays" 
            (change)="onDaysChange()"
            class="px-3 py-1 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
            <option [value]="7">7 أيام</option>
            <option [value]="30">30 يوم</option>
            <option [value]="90">90 يوم</option>
          </select>
        </div>
      </div>
      
      <div class="relative" style="height: 300px;">
        <canvas #chartCanvas></canvas>
      </div>
      
      <div *ngIf="loading" class="absolute inset-0 flex items-center justify-center bg-white/80 rounded-xl">
        <div class="text-center">
          <i class="pi pi-spin pi-spinner text-2xl text-blue-500"></i>
          <p class="mt-2 text-gray-600">جاري تحميل البيانات...</p>
        </div>
      </div>
      
      <div *ngIf="error" class="absolute inset-0 flex items-center justify-center bg-white/80 rounded-xl">
        <div class="text-center">
          <i class="pi pi-exclamation-triangle text-2xl text-red-500"></i>
          <p class="mt-2 text-gray-600">{{ error }}</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      position: relative;
    }
  `]
})
export class DailyRequestsGraphComponent implements OnInit, AfterViewInit {
  @ViewChild('chartCanvas') chartCanvas!: ElementRef<HTMLCanvasElement>;
  
  chart: Chart | null = null;
  graphData: DailyRequestDataDto[] = [];
  loading = true;
  error: string | null = null;
  selectedDays = 30;

  constructor(private dashboardService: DashboardService) {
    Chart.register(...registerables);
  }

  ngOnInit() {
    this.loadGraphData();
  }

  ngAfterViewInit() {
    // Chart will be initialized after data is loaded
  }

  onDaysChange() {
    this.loadGraphData();
  }

  private loadGraphData() {
    this.loading = true;
    this.error = null;

    this.dashboardService.getDailyRequestsData(this.selectedDays).subscribe({
      next: (data: DashboardGraphDataDto) => {
        this.graphData = data.dailyRequests || [];
        this.loading = false;
        this.createChart();
      },
      error: (err) => {
        console.error('Error loading daily requests data:', err);
        this.error = 'فشل في تحميل بيانات الرسم البياني';
        this.loading = false;
      }
    });
  }

  private createChart() {
    if (!this.chartCanvas) return;

    // Destroy existing chart if it exists
    if (this.chart) {
      this.chart.destroy();
    }

    const ctx = this.chartCanvas.nativeElement.getContext('2d');
    if (!ctx) return;

    const chartConfig: ChartConfiguration = {
      type: 'line' as ChartType,
      data: {
        labels: this.graphData.map(d => this.formatDate(d.date || '')),
        datasets: [{
          label: 'عدد الطلبات',
          data: this.graphData.map(d => d.count || 0),
          borderColor: 'rgb(59, 130, 246)',
          backgroundColor: 'rgba(59, 130, 246, 0.1)',
          borderWidth: 2,
          fill: true,
          tension: 0.4,
          pointBackgroundColor: 'rgb(59, 130, 246)',
          pointBorderColor: '#fff',
          pointBorderWidth: 2,
          pointRadius: 4,
          pointHoverRadius: 6
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            backgroundColor: 'rgba(0, 0, 0, 0.8)',
            titleColor: '#fff',
            bodyColor: '#fff',
            borderColor: 'rgb(59, 130, 246)',
            borderWidth: 1,
            displayColors: false,
            callbacks: {
              title: (context) => {
                return this.formatFullDate(context[0].label);
              },
              label: (context) => {
                return `عدد الطلبات: ${context.parsed.y}`;
              }
            }
          }
        },
        scales: {
          x: {
            grid: {
              display: false
            },
            ticks: {
              color: '#6b7280',
              font: {
                size: 11
              }
            }
          },
          y: {
            beginAtZero: true,
            grid: {
              color: '#e5e7eb'
            },
            ticks: {
              color: '#6b7280',
              font: {
                size: 11
              },
              stepSize: 1
            }
          }
        },
        interaction: {
          intersect: false,
          mode: 'index'
        }
      }
    };

    this.chart = new Chart(ctx, chartConfig);
  }

  private formatDate(dateString: string | undefined): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('ar-EG', { month: 'short', day: 'numeric' });
  }

  private formatFullDate(label: string): string {
    // Find original date string from data
    const index = this.graphData.findIndex(d => this.formatDate(d.date || '') === label);
    if (index !== -1 && this.graphData[index].date) {
      const date = new Date(this.graphData[index].date!);
      return date.toLocaleDateString('ar-EG', { 
        year: 'numeric', 
        month: 'long', 
        day: 'numeric' 
      });
    }
    return label;
  }
}
