import { Component, OnInit, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { AppMenuitem } from './app.menuitem';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'app-menu',
    standalone: true,
    imports: [CommonModule, AppMenuitem, RouterModule],
    template: `<ul class="layout-menu">
        <ng-container *ngFor="let item of model; let i = index">
            <li app-menuitem *ngIf="!item.separator" [item]="item" [index]="i" [root]="true"></li>
            <li *ngIf="item.separator" class="menu-separator"></li>
        </ng-container>
    </ul> `
})
export class AppMenu implements OnInit, OnChanges {
    model: MenuItem[] = [];
    isAuthenticated = false;
    userRole: string | null = null;

    constructor(private authService: AuthService) {}

    ngOnInit() {
        this.authService.currentUser.subscribe(user => {
            this.userRole = user?.role || null;
            this.isAuthenticated = user !== null; // Check if user exists (authenticated)
            this.updateMenu();
        });
        
        // Initial menu setup
        this.updateMenu();
    }

    ngOnChanges() {
        // Trigger menu update when component changes
        this.updateMenu();
    }

    private updateMenu() {
        this.model = [
            {
                label: 'الصفحات',
                icon: 'pi pi-fw pi-briefcase',
                visible: true,
                items: this.getMenuItems()
            }
        ];
    }

    private getMenuItems(): MenuItem[] {
        const items: MenuItem[] = [];

        // Dashboard - available to all users
        items.push({
            label: 'الرئيسية',
            icon: 'pi pi-home',
            routerLink: ['/'],
            visible: true
        });

        // Customer creation and update - available to all users
        items.push({
            label: 'إنشاء حساب جديد',
            icon: 'pi pi-user-plus',
            routerLink: ['/pages/customer/create'],
            visible: true
        });
        
        items.push({
            label: 'تحديث البيانات',
            icon: 'pi pi-user-edit',
            routerLink: ['/pages/customer/update'],
            visible: true
        });
        
        // Authenticated users - role-based menu items
        if (this.isAuthenticated) {
            // All authenticated users can view customers list
            items.push({
                label: 'العملاء',
                icon: 'pi pi-users',
                routerLink: ['/pages/customers'],
                visible: true
            });
        }

        return items;
    }
}
