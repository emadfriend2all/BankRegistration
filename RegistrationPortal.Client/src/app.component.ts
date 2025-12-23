import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Client, API_BASE_URL } from './app/api/client';

@Component({
    selector: 'app-root',
    standalone: true,
    imports: [RouterModule],
    providers: [
        { provide: API_BASE_URL, useValue: 'https://localhost:7232' }
    ],
    template: `<router-outlet></router-outlet>`,
    styles: [`
        :host {
            font-family: 'Tajawal', sans-serif;
        }
        
        * {
            font-family: 'Tajawal', sans-serif;
        }
        
        body {
            font-family: 'Tajawal', sans-serif;
            margin: 0;
            padding: 0;
        }
        
        html, body {
            font-family: 'Tajawal', sans-serif;
        }
    `]
})
export class AppComponent {}
