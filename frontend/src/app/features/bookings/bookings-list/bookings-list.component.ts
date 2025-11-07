import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { BookingService, Booking, BookingHistory } from '../../../core/services/booking.service';
import { AuthService } from '../../../core/services/auth.service';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-bookings-list',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, FormsModule],
  templateUrl: './bookings-list.component.html',
  styleUrls: ['./bookings-list.component.scss']
})
export class BookingsListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  bookings: Booking[] = [];
  loading = false;
  currentLanguage = 'en';

  // Filter states
  selectedStatus: string | null = null;
  startDate: string = '';
  endDate: string = '';
  searchTerm: string = '';

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  // Statistics
  totalBookings = 0;
  activeBookings = 0;
  completedBookings = 0;
  cancelledBookings = 0;
  totalSpent = 0;

  // Available statuses
  statuses = [
    { value: 'Pending', label: 'Pending', count: 0 },
    { value: 'Confirmed', label: 'Confirmed', count: 0 },
    { value: 'InProgress', label: 'In Progress', count: 0 },
    { value: 'Completed', label: 'Completed', count: 0 },
    { value: 'Cancelled', label: 'Cancelled', count: 0 }
  ];

  constructor(
    private bookingService: BookingService,
    private authService: AuthService,
    private languageService: LanguageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth/login'], {
        queryParams: { returnUrl: '/bookings' }
      });
      return;
    }

    this.currentLanguage = this.languageService.getCurrentLanguage();

    // Subscribe to language changes
    this.languageService.currentLanguage$
      .pipe(takeUntil(this.destroy$))
      .subscribe(lang => {
        this.currentLanguage = lang;
      });

    this.loadBookings();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadBookings(): void {
    this.loading = true;

    this.bookingService.getBookingHistory({
      status: this.selectedStatus || undefined,
      startDate: this.startDate || undefined,
      endDate: this.endDate || undefined,
      searchTerm: this.searchTerm || undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: BookingHistory) => {
          this.bookings = result.bookings;
          this.totalCount = result.totalCount;
          this.totalPages = result.totalPages;
          this.totalBookings = result.totalBookings;
          this.activeBookings = result.activeBookings;
          this.completedBookings = result.completedBookings;
          this.cancelledBookings = result.cancelledBookings;
          this.totalSpent = result.totalSpent;
          this.loading = false;

          // Update status counts
          this.updateStatusCounts(result);
        },
        error: (error) => {
          console.error('Error loading bookings:', error);
          this.loading = false;
        }
      });
  }

  updateStatusCounts(result: BookingHistory): void {
    // Reset counts
    this.statuses.forEach(s => s.count = 0);

    // Count bookings by status
    this.bookings.forEach(booking => {
      const status = this.statuses.find(s => s.value === booking.status);
      if (status) {
        status.count++;
      }
    });
  }

  filterByStatus(status: string | null): void {
    this.selectedStatus = this.selectedStatus === status ? null : status;
    this.currentPage = 1;
    this.loadBookings();
  }

  onDateFilterChange(): void {
    this.currentPage = 1;
    this.loadBookings();
  }

  onSearchChange(): void {
    this.currentPage = 1;
    this.loadBookings();
  }

  clearFilters(): void {
    this.selectedStatus = null;
    this.startDate = '';
    this.endDate = '';
    this.searchTerm = '';
    this.currentPage = 1;
    this.loadBookings();
  }

  // Pagination
  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadBookings();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.goToPage(this.currentPage - 1);
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.goToPage(this.currentPage + 1);
    }
  }

  get paginationPages(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);

    if (end - start < maxVisible - 1) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    return pages;
  }

  // Actions
  viewBookingDetails(bookingId: string): void {
    this.router.navigate(['/bookings', bookingId]);
  }

  cancelBooking(booking: Booking): void {
    if (!confirm(`Are you sure you want to cancel the booking for ${booking.serviceName}?`)) {
      return;
    }

    const reason = prompt('Please provide a cancellation reason (optional):') || 'Customer requested cancellation';

    this.bookingService.cancelBooking(booking.bookingId, reason)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          // Reload bookings to show updated status
          this.loadBookings();
        },
        error: (error) => {
          console.error('Error cancelling booking:', error);
          alert('Failed to cancel booking. Please try again.');
        }
      });
  }

  bookAgain(booking: Booking): void {
    // Navigate to service detail to book again
    this.router.navigate(['/services', booking.serviceId]);
  }

  // Helper methods
  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString(this.currentLanguage === 'ar' ? 'ar-SA' : 'en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatPrice(price: number): string {
    return `SAR ${price.toFixed(2)}`;
  }

  getBookingStatusClass(status: string): string {
    const statusMap: Record<string, string> = {
      'Pending': 'status-pending',
      'Confirmed': 'status-confirmed',
      'InProgress': 'status-inprogress',
      'Completed': 'status-completed',
      'Cancelled': 'status-cancelled',
      'Rejected': 'status-rejected'
    };
    return statusMap[status] || 'status-default';
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

  canCancelBooking(booking: Booking): boolean {
    return booking.status === 'Pending' || booking.status === 'Confirmed';
  }

  canReviewBooking(booking: Booking): boolean {
    return booking.status === 'Completed';
  }

  trackByBookingId(index: number, booking: Booking): string {
    return booking.bookingId;
  }

  hasActiveFilters(): boolean {
    return !!(this.selectedStatus || this.startDate || this.endDate || this.searchTerm);
  }

  getActiveFilterCount(): number {
    let count = 0;
    if (this.selectedStatus) count++;
    if (this.startDate || this.endDate) count++;
    if (this.searchTerm) count++;
    return count;
  }
}
