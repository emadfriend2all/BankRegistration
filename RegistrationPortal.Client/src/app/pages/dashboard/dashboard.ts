import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Client } from '../../api/client';
import { DashboardStatisticsComponent } from './components/dashboard-statistics.component';
import { DailyRequestsGraphComponent } from './components/daily-requests-graph.component';

@Component({
    selector: 'app-dashboard',
    imports: [CommonModule, ButtonModule, RouterModule, FormsModule, DashboardStatisticsComponent, DailyRequestsGraphComponent],
    template: `
        <!-- Anonymous users - Full screen layout without grid constraint -->
        <ng-container *ngIf="!isAuthenticated">
            <!-- Professional Background -->
            <div class="fixed inset-0 bg-gradient-to-br from-blue-50 via-white to-indigo-50 -z-10"></div>
            
            <!-- Main Content - Full Width -->
            <div class="relative flex">
                <div class="w-full max-w-7xl mx-auto">
                    <!-- Glass Card Container -->
                    <div class="backdrop-blur-sm bg-white/90 border border-white/20 shadow-2xl rounded-3xl p-8 md:p-12">
                        
                        <!-- Header Section -->
                        <div class="text-center mb-12">
                            <div class="inline-flex items-center justify-center w-24 h-24 mb-6">
                                <img src="/global/logo.png" alt="SSDB Logo" class="w-24 h-24 object-contain" />
                            </div>
                            <h1 class="text-4xl font-bold text-gray-900 mb-4 bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent">
                                مرحباً بكم في بوابة المصرف
                            </h1>
                            <p class="text-xl text-gray-600 leading-relaxed mx-auto">
                                نظام تسجيل عملاء احترافي وفعال لإدارة حسابات المصرفية
                            </p>
                        </div>

                        <!-- Action Cards Grid -->
                        <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-12">
                            <!-- Create Customer Card -->
                            <div class="group relative overflow-hidden rounded-2xl border border-gray-200 bg-gradient-to-br from-blue-50 to-indigo-50 p-8 hover:shadow-xl transition-all duration-300 hover:scale-105">
                                <div class="relative z-10">
                                    <div class="flex items-center justify-center w-16 h-16 rounded-xl bg-gradient-to-br from-blue-500 to-blue-600 shadow-md mb-6">
                                        <i class="pi pi-user-plus text-2xl text-white"></i>
                                    </div>
                                    <h3 class="text-2xl font-bold text-gray-900 mb-3">تسجيل عميل جديد</h3>
                                    <p class="text-gray-600 mb-6 leading-relaxed">
                                        إنشاء حساب جديد للعملاء مع جميع البيانات المطلوبة
                                    </p>
                                    <p-button 
                                        label="إنشاء حساب" 
                                        icon="pi pi-arrow-left" 
                                        iconPos="right"
                                        routerLink="pages/customer/create" 
                                        class="w-full py-4 text-lg font-semibold from-blue-500 to-indigo-600 border-0 hover:from-blue-600 hover:to-indigo-700"
                                        size="large" />
                                </div>
                                <!-- Decorative Element -->
                                <div class="absolute top-0 right-0 w-32 h-32 bg-gradient-to-br from-blue-400/20 to-transparent rounded-full -translate-y-16 translate-x-16"></div>
                            </div>

                            <!-- Update Customer Card -->
                            <div class="group relative overflow-hidden rounded-2xl border border-gray-200 bg-gradient-to-br from-green-50 to-emerald-50 p-8 hover:shadow-xl transition-all duration-300 hover:scale-105">
                                <div class="relative z-10">
                                    <div class="flex items-center justify-center w-16 h-16 rounded-xl bg-gradient-to-br from-green-500 to-emerald-600 shadow-md mb-6">
                                        <i class="pi pi-user-edit text-2xl text-white"></i>
                                    </div>
                                    <h3 class="text-2xl font-bold text-gray-900 mb-3">تحديث بيانات عميل</h3>
                                    <p class="text-gray-600 mb-6 leading-relaxed">
                                        تعديل وتحديث معلومات العملاء المسجلين مسبقاً
                                    </p>
                                    <p-button 
                                        label="تحديث البيانات" 
                                        icon="pi pi-arrow-left" 
                                        iconPos="right"
                                        routerLink="pages/customer/update" 
                                        class="w-full py-4 text-lg font-semibold  from-green-500 to-emerald-600 border-0 hover:from-green-600 hover:to-emerald-700 p-button-outlined"
                                        size="large" />
                                </div>
                                <!-- Decorative Element -->
                                <div class="absolute top-0 right-0 w-32 h-32 bg-gradient-to-br from-green-400/20 to-transparent rounded-full -translate-y-16 translate-x-16"></div>
                            </div>
                        </div>

                        <!-- Login Prompt -->
                        <div class="bg-gradient-to-r from-gray-50 to-blue-50 rounded-2xl p-6 border border-gray-200">
                            <div class="flex items-center justify-center">
                                <div class="flex items-center justify-center w-10 h-10 rounded-lg bg-blue-100 mr-3">
                                    <i class="pi pi-info-circle text-blue-600"></i>
                                </div>
                                <span class="text-gray-700">
                                    <strong>يرجى التأكد من إدخال جميع البيانات المطلوبة بشكل صحيح وتوفير المستندات اللازمة لتجنب تأخير طلبك</strong> 
                                    
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </ng-container>

        <!-- Authenticated users - Grid layout with widgets -->
        <ng-container *ngIf="isAuthenticated">
            <div class="space-y-8">
                <!-- Dashboard Statistics -->
                <app-dashboard-statistics />
                
                <!-- Daily Requests Graph -->
                <app-daily-requests-graph />
            </div>
        </ng-container>
    `
})
export class Dashboard implements OnInit {
    isAuthenticated = false;
    userRole: string | null = null;

    constructor(
        private authService: AuthService,
        private router: Router
    ) {}

    ngOnInit() {
        this.authService.currentUser.subscribe(user => {
            this.userRole = user?.role || null;
            this.isAuthenticated = this.authService.isAuthenticated;
        });
    }
}
