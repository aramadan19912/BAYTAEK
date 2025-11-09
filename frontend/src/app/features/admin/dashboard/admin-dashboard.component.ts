import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { Subject, takeUntil } from 'rxjs';
import { AdminService, DashboardStats } from '../../../core/services/admin.service';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  stats: DashboardStats | null = null;
  isLoading = true;
  currentLanguage = 'en';

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

    this.loadDashboardStats();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadDashboardStats(): void {
    this.isLoading = true;
    this.adminService.getDashboardStats()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (stats) => {
          this.stats = stats;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading dashboard stats:', error);
          this.isLoading = false;
        }
      });
  }

  navigateToUsers(): void {
    this.router.navigate(['/admin/users']);
  }

  navigateToServices(): void {
    this.router.navigate(['/admin/services']);
  }

  navigateToBookings(): void {
    this.router.navigate(['/admin/bookings']);
  }

  navigateToReports(): void {
    this.router.navigate(['/admin/reports']);
  }

  formatCurrency(amount: number): string {
    return `SAR ${amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  formatNumber(num: number): string {
    return num.toLocaleString('en-US');
  }

  formatPercentage(value: number): string {
    return `${value.toFixed(1)}%`;
  }

  getGrowthClass(growth: number): string {
    if (growth > 0) return 'positive';
    if (growth < 0) return 'negative';
    return 'neutral';
  }

  getGrowthIcon(growth: number): string {
    if (growth > 0) return '↑';
    if (growth < 0) return '↓';
    return '→';
  }
}
