import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import {
  ProviderService as ProviderApiService,
  ProviderDashboardStats,
  ProviderProfile,
  ProviderBooking,
  ProviderService as ProviderServiceDto
} from '../../../core/services/provider.service';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-provider-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './provider-dashboard.component.html',
  styleUrls: ['./provider-dashboard.component.scss']
})
export class ProviderDashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Dashboard data
  stats: ProviderDashboardStats | null = null;
  profile: ProviderProfile | null = null;
  recentBookings: ProviderBooking[] = [];
  activeServices: ProviderServiceDto[] = [];

  // Loading states
  isLoadingStats = false;
  isLoadingProfile = false;
  isLoadingBookings = false;
  isLoadingServices = false;

  // Quick stats
  todayBookings = 0;
  weekEarnings = 0;
  pendingBookings = 0;
  totalCompleted = 0;
  activeServicesCount = 0;

  constructor(
    private providerService: ProviderApiService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadProviderProfile();
    this.loadDashboardStats();
    this.loadRecentBookings();
    this.loadActiveServices();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Load provider profile
  loadProviderProfile(): void {
    this.isLoadingProfile = true;
    this.providerService.getProviderProfile().pipe(takeUntil(this.destroy$)).subscribe({
      next: (profile) => {
        this.profile = profile;
        this.isLoadingProfile = false;
      },
      error: (error) => {
        console.error('Error loading provider profile:', error);
        this.isLoadingProfile = false;
      }
    });
  }

  // Load dashboard statistics
  loadDashboardStats(): void {
    this.isLoadingStats = true;
    this.providerService.getDashboardStats().pipe(takeUntil(this.destroy$)).subscribe({
      next: (stats) => {
        this.stats = stats;
        this.calculateQuickStats();
        this.isLoadingStats = false;
      },
      error: (error) => {
        console.error('Error loading dashboard stats:', error);
        this.isLoadingStats = false;
      }
    });
  }

  // Load recent bookings
  loadRecentBookings(): void {
    this.isLoadingBookings = true;
    this.providerService.getProviderBookings({
      pageNumber: 1,
      pageSize: 5
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: (result) => {
        this.recentBookings = result.bookings;
        this.isLoadingBookings = false;
      },
      error: (error) => {
        console.error('Error loading recent bookings:', error);
        this.isLoadingBookings = false;
      }
    });
  }

  // Load active services
  loadActiveServices(): void {
    this.isLoadingServices = true;
    this.providerService.getProviderServices({
      status: 'Active',
      pageNumber: 1,
      pageSize: 6
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: (result) => {
        this.activeServices = result.services;
        this.activeServicesCount = result.totalCount;
        this.isLoadingServices = false;
      },
      error: (error) => {
        console.error('Error loading active services:', error);
        this.isLoadingServices = false;
      }
    });
  }

  // Calculate quick stats
  calculateQuickStats(): void {
    if (!this.stats) return;

    this.todayBookings = this.stats.todayBookings ?? 0;
    this.weekEarnings = this.stats.monthlyNetEarnings ?? this.stats.monthlyRevenue ?? 0;
    this.pendingBookings = this.stats.pendingBookingsCount ?? 0;
    this.totalCompleted = this.stats.monthlyCompleted ?? 0;
  }

  // Navigation methods
  navigateToServices(): void {
    this.router.navigate(['/provider/services']);
  }

  navigateToBookings(): void {
    this.router.navigate(['/provider/bookings']);
  }

  navigateToEarnings(): void {
    this.router.navigate(['/provider/earnings']);
  }

  navigateToReviews(): void {
    this.router.navigate(['/provider/reviews']);
  }

  navigateToAddService(): void {
    this.router.navigate(['/provider/services/add']);
  }

  viewBookingDetail(bookingId: string): void {
    this.router.navigate(['/provider/bookings', bookingId]);
  }

  viewServiceDetail(serviceId: string): void {
    this.router.navigate(['/provider/services', serviceId]);
  }

  // Utility methods
  getBookingStatusClass(status: string): string {
    const statusMap: Record<string, string> = {
      'Pending': 'status-pending',
      'Confirmed': 'status-confirmed',
      'InProgress': 'status-inprogress',
      'Completed': 'status-completed',
      'Cancelled': 'status-cancelled',
      'Rejected': 'status-rejected'
    };
    return statusMap[status] || '';
  }

  getBookingStatusLabel(status: string): string {
    const statusMap: Record<string, string> = {
      'Pending': 'Pending',
      'Confirmed': 'Confirmed',
      'InProgress': 'In Progress',
      'Completed': 'Completed',
      'Cancelled': 'Cancelled',
      'Rejected': 'Rejected'
    };
    return statusMap[status] || status;
  }

  formatDate(date: string): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  }

  formatTime(date: string): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'SAR',
      minimumFractionDigits: 2
    }).format(amount);
  }

  getVerificationStatusClass(status: string): string {
    const statusMap: Record<string, string> = {
      'Pending': 'status-pending',
      'UnderReview': 'status-review',
      'Verified': 'status-verified',
      'Rejected': 'status-rejected'
    };
    return statusMap[status] || '';
  }

  getVerificationStatusLabel(status: string): string {
    const statusMap: Record<string, string> = {
      'Pending': 'Pending Verification',
      'UnderReview': 'Under Review',
      'Verified': 'Verified',
      'Rejected': 'Rejected'
    };
    return statusMap[status] || status;
  }

  // Check if profile needs attention
  needsAttention(): boolean {
    if (!this.profile) return false;
    return this.profile.verificationStatus === 'Pending' ||
           this.profile.verificationStatus === 'Rejected';
  }

  // Get completion percentage
  getProfileCompletion(): number {
    if (!this.profile) return 0;

    let completed = 0;
    const total = 10;

    if (this.profile.businessName) completed++;
    if (this.profile.businessType) completed++;
    if (this.profile.businessDescription) completed++;
    if (this.profile.commercialRegistrationNumber) completed++;
    if (this.profile.taxNumber) completed++;
    if (this.profile.experienceYears && this.profile.experienceYears > 0) completed++;
    if (this.profile.serviceRegions && this.profile.serviceRegions.length > 0) completed++;
    if (this.profile.verificationDocuments && this.profile.verificationDocuments.length > 0) completed++;
    if (this.profile.bankAccountInfo) completed++;
    if (this.profile.verificationStatus === 'Verified') completed++;

    return Math.round((completed / total) * 100);
  }
}
