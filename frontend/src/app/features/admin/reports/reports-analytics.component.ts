import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { Subject, takeUntil } from 'rxjs';
import { AdminService, SystemAnalytics } from '../../../core/services/admin.service';
import { LanguageService } from '../../../core/services/language.service';

interface DateRangePreset {
  label: string;
  days: number;
}

@Component({
  selector: 'app-reports-analytics',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './reports-analytics.component.html',
  styleUrls: ['./reports-analytics.component.scss']
})
export class ReportsAnalyticsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  analytics: SystemAnalytics | null = null;
  isLoading = true;
  currentLanguage = 'en';

  // Date range
  startDate = '';
  endDate = '';
  selectedPreset = '30';

  dateRangePresets: DateRangePreset[] = [
    { label: 'Last 7 Days', days: 7 },
    { label: 'Last 30 Days', days: 30 },
    { label: 'Last 90 Days', days: 90 },
    { label: 'Last 365 Days', days: 365 }
  ];

  constructor(
    private adminService: AdminService,
    private languageService: LanguageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentLanguage = this.languageService.getCurrentLanguage();

    this.languageService.currentLanguage$
      .pipe(takeUntil(this.destroy$))
      .subscribe(lang => {
        this.currentLanguage = lang;
      });

    // Set default date range (last 30 days)
    this.setDateRangePreset(30);
    this.loadAnalytics();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadAnalytics(): void {
    this.isLoading = true;

    this.adminService.getSystemAnalytics(this.startDate, this.endDate)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (analytics) => {
          this.analytics = analytics;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading analytics:', error);
          this.isLoading = false;
        }
      });
  }

  setDateRangePreset(days: number): void {
    const end = new Date();
    const start = new Date();
    start.setDate(start.getDate() - days);

    this.endDate = end.toISOString().split('T')[0];
    this.startDate = start.toISOString().split('T')[0];
    this.selectedPreset = days.toString();
  }

  onPresetChange(event: Event): void {
    const days = parseInt((event.target as HTMLSelectElement).value);
    this.setDateRangePreset(days);
    this.loadAnalytics();
  }

  onStartDateChange(event: Event): void {
    this.startDate = (event.target as HTMLInputElement).value;
    this.selectedPreset = 'custom';
    this.loadAnalytics();
  }

  onEndDateChange(event: Event): void {
    this.endDate = (event.target as HTMLInputElement).value;
    this.selectedPreset = 'custom';
    this.loadAnalytics();
  }

  exportReport(format: 'pdf' | 'excel' | 'csv'): void {
    // This would call the backend API to generate and download the report
    console.log(`Exporting report as ${format}...`);
    alert(`Export as ${format.toUpperCase()} will be implemented with backend integration.`);
  }

  backToDashboard(): void {
    this.router.navigate(['/admin/dashboard']);
  }

  formatCurrency(amount: number, currency: string = 'SAR'): string {
    return `${currency} ${amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  formatNumber(num: number): string {
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1) + 'M';
    } else if (num >= 1000) {
      return (num / 1000).toFixed(1) + 'K';
    }
    return num.toString();
  }

  formatPercent(value: number): string {
    return value.toFixed(1) + '%';
  }

  getUserRetentionRate(): number {
    if (!this.analytics || !this.analytics.users.totalUsers) {
      return 0;
    }
    return (this.analytics.users.activeUsers / this.analytics.users.totalUsers) * 100;
  }

  getBookingCompletionRate(): number {
    if (!this.analytics || !this.analytics.bookings.totalBookings) {
      return 0;
    }
    return (this.analytics.bookings.completedBookings / this.analytics.bookings.totalBookings) * 100;
  }

  getBookingCancellationRate(): number {
    if (!this.analytics || !this.analytics.bookings.totalBookings) {
      return 0;
    }
    return (this.analytics.bookings.cancelledBookings / this.analytics.bookings.totalBookings) * 100;
  }

  getProviderToCustomerRatio(): string {
    if (!this.analytics || !this.analytics.users.totalCustomers) {
      return '0:1';
    }
    const ratio = this.analytics.users.totalProviders / this.analytics.users.totalCustomers;
    return `1:${Math.round(1/ratio)}`;
  }

  getAveragePlatformCommission(): number {
    if (!this.analytics || !this.analytics.bookings.totalRevenue) {
      return 0;
    }
    return ((this.analytics.bookings.platformEarnings / this.analytics.bookings.totalRevenue) * 100);
  }

  getServiceApprovalRate(): number {
    if (!this.analytics || !this.analytics.services.totalServices) {
      return 0;
    }
    return ((this.analytics.services.activeServices / this.analytics.services.totalServices) * 100);
  }
}
