import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app.config';
import { AppComponent } from './app.component';

// Import Angular compiler for JIT compilation in development
import '@angular/compiler';

bootstrapApplication(AppComponent, appConfig).catch((err) => console.error(err));
