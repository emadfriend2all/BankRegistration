import { Component, OnInit, OnDestroy } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { StyleClassModule } from 'primeng/styleclass';
import { ButtonModule } from 'primeng/button';
import { LayoutService } from '../service/layout.service';
import { AuthService } from '../../services/auth.service';
import { Subscription } from 'rxjs';

@Component({
    selector: 'app-topbar',
    standalone: true,
    imports: [RouterModule, CommonModule, StyleClassModule, ButtonModule],
    template: ` <div class="layout-topbar">
        <div class="layout-topbar-logo-container">
            <button class="layout-menu-button layout-topbar-action" (click)="layoutService.onMenuToggle()">
                <i class="pi pi-bars"></i>
            </button>
            <a class="layout-topbar-logo" routerLink="/">
                <img src="/global/logo.png" alt="SSDB Logo" class="w-8 h-8 mr-2" />
                <span>مصرف الإدخار والتنمية المصرفية</span>
            </a>
        </div>

        <div class="layout-topbar-actions">
            <div class="layout-config-menu">
                <button type="button" class="layout-topbar-action" (click)="toggleDarkMode()">
                    <i [ngClass]="{ 'pi ': true, 'pi-moon': layoutService.isDarkTheme(), 'pi-sun': !layoutService.isDarkTheme() }"></i>
                </button>
                
            </div>

            <button class="layout-topbar-menu-button layout-topbar-action" pStyleClass="@next" enterFromClass="hidden" enterActiveClass="animate-scalein" leaveToClass="hidden" leaveActiveClass="animate-fadeout" [hideOnOutsideClick]="true">
                <i class="pi pi-ellipsis-v"></i>
            </button>

            <div class="layout-topbar-menu hidden lg:block">
                <div class="layout-topbar-menu-content">
                    <ng-container *ngIf="isAuthenticated; else notLoggedIn">
                        <button pButton label="Logout" icon="pi pi-sign-out" class="p-button-outlined" (click)="onLogout()"></button>
                    </ng-container>
                    <ng-template #notLoggedIn>
                        <button pButton label="Login" icon="pi pi-sign-in" class="p-button-outlined" routerLink="/auth/login"></button>
                    </ng-template>
                </div>
            </div>
        </div>
    </div>`
})
export class AppTopbar implements OnInit, OnDestroy {
    items!: MenuItem[];
    isAuthenticated = false;
    private authSubscription!: Subscription;

    constructor(
        public layoutService: LayoutService,
        private authService: AuthService
    ) {}

    ngOnInit(): void {
        this.authSubscription = this.authService.currentUser.subscribe(user => {
            this.isAuthenticated = !!user;
        });
        
        // Initial check
        this.isAuthenticated = this.authService.isAuthenticated;
    }

    ngOnDestroy(): void {
        if (this.authSubscription) {
            this.authSubscription.unsubscribe();
        }
    }

    toggleDarkMode() {
        this.layoutService.layoutConfig.update((state) => ({ ...state, darkTheme: !state.darkTheme }));
    }

    onLogout(): void {
        this.authService.logout();
    }
}
