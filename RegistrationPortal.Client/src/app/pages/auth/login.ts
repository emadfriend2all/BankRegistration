import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { RippleModule } from 'primeng/ripple';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [ButtonModule, CardModule, CheckboxModule, InputTextModule, PasswordModule, FormsModule, RouterModule, RippleModule, CommonModule],
    templateUrl: './login.html'
})
export class Login implements OnInit {
    email: string = '';
    password: string = '';
    checked: boolean = false;
    isLoading: boolean = false;
    messages: any[] = [];
    returnUrl: string | null = null;

    constructor(
        private authService: AuthService,
        private router: Router,
        private route: ActivatedRoute,
        private messageService: MessageService
    ) {}

    ngOnInit(): void {
        // Get return URL from route parameters or default to '/'
        this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    }

    get hasError(): boolean {
        return this.messages.some(m => m.severity === 'error');
    }

    onLogin(): void {
        if (!this.email || !this.password) {
            this.messages = [{ severity: 'error', detail: 'يرجى إدخال البريد الإلكتروني وكلمة المرور' }];
            return;
        }

        this.isLoading = true;
        this.messages = [];

        this.authService.login(this.email, this.password, this.checked, this.returnUrl || undefined).subscribe({
            next: (response) => {
                this.isLoading = false;
                this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Login successful!' });
                // Navigation is handled by AuthService
            },
            error: (error) => {
                this.isLoading = false;
                console.error('Login error:', error);
                this.messages = [{
                    severity: 'error',
                    detail: error?.message || 'Login failed. Please check your credentials and try again.'
                }];
            }
        });
    }
}
