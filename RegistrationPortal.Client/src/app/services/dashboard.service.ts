import { Injectable } from '@angular/core';
import { Client, DashboardStatisticsDto, DashboardGraphDataDto, DailyRequestDataDto } from '../api/client';
import { Observable } from 'rxjs';

// Re-export DTOs for components
export { DashboardStatisticsDto, DashboardGraphDataDto, DailyRequestDataDto } from '../api/client';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  constructor(private apiClient: Client) {}

  getDashboardStatistics(): Observable<DashboardStatisticsDto> {
    return this.apiClient.statistics();
  }

  getDailyRequestsData(days: number = 30): Observable<DashboardGraphDataDto> {
    return this.apiClient.dailyRequests(days);
  }
}
