import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

export interface AppConfig {
  api: {
    baseUrl: string;
    proxy: {
      [key: string]: {
        target: string;
        secure: boolean;
        changeOrigin: boolean;
        logLevel: string;
      };
    };
  };
  app: {
    rtl: boolean;
    darkModeSelector: string;
    theme: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class ConfigService {
  private config!: AppConfig;

  constructor() {}

  public loadConfig(): Observable<AppConfig> {
    const environment = this.getEnvironment();
    const configPath = `config/${environment}.json`;
    
    return new Observable<AppConfig>((observer) => {
      fetch(configPath)
        .then(response => {
          if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
          }
          return response.json();
        })
        .then(config => {
          this.config = config;
          observer.next(config);
          observer.complete();
        })
        .catch(error => {
          console.error('Failed to load config, trying development config:', error);
          // Fallback to development config
          fetch('config/development.json')
            .then(response => response.json())
            .then(config => {
              this.config = config;
              observer.next(config);
              observer.complete();
            })
            .catch(fallbackError => {
              console.error('Failed to load development config:', fallbackError);
              observer.error(fallbackError);
            });
        });
    });
  }

  public getConfig(): AppConfig {
    return this.config;
  }

  public getApiBaseUrl(): string {
    return this.config.api.baseUrl;
  }

  public getProxyConfig(): any {
    return this.config.api.proxy;
  }

  public getAppConfig(): any {
    return this.config.app;
  }

  private getEnvironment(): string {
    // Check for environment variable first
    if ((window as any).__env?.environment) {
      return (window as any).__env.environment;
    }
    
    // Check hostname to determine environment
    const hostname = window.location.hostname;
    
    // Production environments
    if (hostname === 'localhost' && window.location.port === '80') {
      return 'production';
    }
    if (hostname !== 'localhost' && hostname !== '127.0.0.1') {
      return 'production';
    }
    
    // Default to development for local development
    return 'development';
  }
}
