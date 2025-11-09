import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { AdminService, BookingListItem } from '../../../core/services/admin.service';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-bookings-management',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './bookings-management.component.html',
  styleUrls: ['./bookings-management.component.scss']
})
export class BookingsManagementComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private searchSubject$ = new Subject<string>();

  bookings: BookingListItem[] = [];
  isLoading = true;
  currentLanguage = 'en';

  // Filters
  searchTerm = '';
  selectedStatus = '';
  selectedPaymentStatus = '';
  startDate = '';
  endDate = '';

  // Pagination
  pageNumber = 1;
  pageSize = 20;
  totalCount = 0;
  hasNextPage = false;
  hasPreviousPage = false;

  // Statistics
  stats = {
    totalBookings: 0,
    pendingBookings: 0,
    confirmedBookings: 0,
    completedBookings: 0,
    cancelledBookings: 0,
    totalRevenue: 0
  };

  // Status options
  statusOptions = [
    { value: 'Pending', label: 'Pending' },
    { value: 'Confirmed', label: 'Confirmed' },
    { value: 'InProgress', label: 'In Progress' },
    { value: 'Completed', label: 'Completed' },
    { value: 'Cancelled', label: 'Cancelled' },
    { value: 'Disputed', label: 'Disputed' }
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

    // Setup search debouncing
    this.searchSubject$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(searchTerm => {
        this.searchTerm = searchTerm;
        this.pageNumber = 1;
        this.loadBookings();
      });

    this.loadBookings();
    this.loadStatistics();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadBookings(): void {
    this.isLoading = true;

    const params: any = {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    };

    if (this.searchTerm) {
      params.searchTerm = this.searchTerm;
    }

    if (this.selectedStatus) {
      params.status = this.selectedStatus;
    }

    if (this.selectedPaymentStatus) {
      params.paymentStatus = this.selectedPaymentStatus;
    }

    if (this.startDate) {
      params.startDate = this.startDate;
    }

    if (this.endDate) {
      params.endDate = this.endDate;
    }

    this.adminService.getBookings(params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.bookings = result.bookings;
          this.totalCount = result.totalCount;
          this.hasNextPage = result.hasNextPage;
          this.hasPreviousPage = result.hasPreviousPage;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading bookings:', error);
          this.isLoading = false;
        }
      });
  }

  loadStatistics(): void {
    this.adminService.getBookingStatistics()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (stats) => {
          this.stats = stats;
        },
        error: (error) => {
          console.error('Error loading statistics:', error);
        }
      });
  }

  onSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchSubject$.next(value);
  }

  onStatusChange(event: Event): void {
    this.selectedStatus = (event.target as HTMLSelectElement).value;
    this.pageNumber = 1;
    this.loadBookings();
  }

  onPaymentStatusChange(event: Event): void {
    this.selectedPaymentStatus = (event.target as HTMLSelectElement).value;
    this.pageNumber = 1;
    this.loadBookings();
  }

  onStartDateChange(event: Event): void {
    this.startDate = (event.target as HTMLInputElement).value;
    this.pageNumber = 1;
    this.loadBookings();
  }

  onEndDateChange(event: Event): void {
    this.endDate = (event.target as HTMLInputElement).value;
    this.pageNumber = 1;
    this.loadBookings();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedStatus = '';
    this.selectedPaymentStatus = '';
    this.startDate = '';
    this.endDate = '';
    this.pageNumber = 1;
    this.loadBookings();
  }

  nextPage(): void {
    if (this.hasNextPage) {
      this.pageNumber++;
      this.loadBookings();
    }
  }

  previousPage(): void {
    if (this.hasPreviousPage) {
      this.pageNumber--;
      this.loadBookings();
    }
  }

  viewBookingDetails(booking: BookingListItem): void {
    this.router.navigate(['/admin/bookings', booking.bookingId]);
  }

  cancelBooking(booking: BookingListItem, event: Event): void {
    event.stopPropagation();

    const reason = prompt(`Please provide a reason for cancelling booking #${booking.bookingNumber}:`);

    if (!reason) {
      return;
    }

    this.adminService.cancelBooking(booking.bookingId, { reason, notifyCustomer: true, refundAmount: booking.totalAmount })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadBookings();
          this.loadStatistics();
        },
        error: (error) => {
          console.error('Error cancelling booking:', error);
          alert('Failed to cancel booking. Please try again.');
        }
      });
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'pending':
        return 'status-pending';
      case 'confirmed':
        return 'status-confirmed';
      case 'inprogress':
        return 'status-inprogress';
      case 'completed':
        return 'status-completed';
      case 'cancelled':
        return 'status-cancelled';
      case 'disputed':
        return 'status-disputed';
      default:
        return 'status-default';
    }
  }

  getPaymentStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'paid':
        return 'payment-paid';
      case 'pending':
        return 'payment-pending';
      case 'refunded':
        return 'payment-refunded';
      case 'failed':
        return 'payment-failed';
      default:
        return 'payment-default';
    }
  }

  backToDashboard(): void {
    this.router.navigate(['/admin/dashboard']);
  }

  formatCurrency(amount: number, currency: string = 'SAR'): string {
    return `${currency} ${amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString(this.currentLanguage === 'ar' ? 'ar-SA' : 'en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  formatDateTime(date: string): string {
    return new Date(date).toLocaleString(this.currentLanguage === 'ar' ? 'ar-SA' : 'en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getUserInitial(name: string): string {
    return name ? name.charAt(0).toUpperCase() : '?';
  }
}
