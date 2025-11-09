import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, debounceTime } from 'rxjs/operators';

import { ProviderService } from '../../../core/services/provider.service';
import { Booking } from '../../../core/services/booking.service';

@Component({
  selector: 'app-provider-bookings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './provider-bookings.component.html',
  styleUrls: ['./provider-bookings.component.scss']
})
export class ProviderBookingsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private searchSubject$ = new Subject<string>();

  // Bookings data
  bookings: Booking[] = [];

  // Filters
  searchTerm = '';
  selectedStatus = '';
  startDate = '';
  endDate = '';
  sortBy = 'scheduledDate';
  sortOrder: 'asc' | 'desc' = 'desc';

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  // Statistics
  todayBookings = 0;
  pendingBookings = 0;
  confirmedBookings = 0;
  completedBookings = 0;
  cancelledBookings = 0;
  totalRevenue = 0;

  // Loading states
  isLoading = false;
  isProcessing = false;

  // Selected booking for actions
  selectedBooking: Booking | null = null;
  showAcceptModal = false;
  showRejectModal = false;
  showCancelModal = false;
  showCompleteModal = false;
  rejectReason = '';
  cancellationReason = '';

  constructor(
    private providerService: ProviderService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadBookings();
    this.loadStatistics();

    // Setup debounced search
    this.searchSubject$.pipe(
      debounceTime(500),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.currentPage = 1;
      this.loadBookings();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Load bookings
  loadBookings(): void {
    this.isLoading = true;

    const params: any = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortBy: this.sortBy,
      sortOrder: this.sortOrder
    };

    if (this.searchTerm) params.searchTerm = this.searchTerm;
    if (this.selectedStatus) params.status = this.selectedStatus;
    if (this.startDate) params.startDate = this.startDate;
    if (this.endDate) params.endDate = this.endDate;

    this.providerService.getProviderBookings(params).pipe(takeUntil(this.destroy$)).subscribe({
      next: (result) => {
        this.bookings = result.bookings;
        this.totalCount = result.totalCount;
        this.totalPages = Math.ceil(this.totalCount / this.pageSize);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading bookings:', error);
        this.isLoading = false;
      }
    });
  }

  // Load statistics
  loadStatistics(): void {
    this.providerService.getDashboardStats().pipe(takeUntil(this.destroy$)).subscribe({
      next: (stats) => {
        this.todayBookings = stats.todayBookings || 0;
        this.pendingBookings = stats.pendingBookings || 0;
        this.confirmedBookings = stats.confirmedBookings || 0;
        this.completedBookings = stats.completedBookings || 0;
        this.cancelledBookings = stats.cancelledBookings || 0;
        this.totalRevenue = stats.totalRevenue || 0;
      },
      error: (error) => {
        console.error('Error loading statistics:', error);
      }
    });
  }

  // Search and filter handlers
  onSearchChange(): void {
    this.searchSubject$.next(this.searchTerm);
  }

  onStatusChange(): void {
    this.currentPage = 1;
    this.loadBookings();
  }

  onDateFilterChange(): void {
    this.currentPage = 1;
    this.loadBookings();
  }

  onSortChange(): void {
    this.loadBookings();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedStatus = '';
    this.startDate = '';
    this.endDate = '';
    this.sortBy = 'scheduledDate';
    this.sortOrder = 'desc';
    this.currentPage = 1;
    this.loadBookings();
  }

  // Pagination
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadBookings();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  get paginationPages(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);

    if (end - start + 1 < maxVisible) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  // Navigation
  viewBookingDetail(bookingId: string): void {
    this.router.navigate(['/provider/bookings', bookingId]);
  }

  // Booking actions
  openAcceptModal(booking: Booking): void {
    this.selectedBooking = booking;
    this.showAcceptModal = true;
  }

  closeAcceptModal(): void {
    this.showAcceptModal = false;
    this.selectedBooking = null;
  }

  confirmAccept(): void {
    if (!this.selectedBooking) return;

    this.isProcessing = true;
    this.providerService.acceptBooking(this.selectedBooking.bookingId).subscribe({
      next: () => {
        this.loadBookings();
        this.loadStatistics();
        this.closeAcceptModal();
        this.isProcessing = false;
        alert('Booking accepted successfully');
      },
      error: (error) => {
        console.error('Error accepting booking:', error);
        alert('Failed to accept booking. Please try again.');
        this.isProcessing = false;
      }
    });
  }

  openRejectModal(booking: Booking): void {
    this.selectedBooking = booking;
    this.rejectReason = '';
    this.showRejectModal = true;
  }

  closeRejectModal(): void {
    this.showRejectModal = false;
    this.selectedBooking = null;
    this.rejectReason = '';
  }

  confirmReject(): void {
    if (!this.selectedBooking || !this.rejectReason) {
      alert('Please provide a reason for rejection');
      return;
    }

    this.isProcessing = true;
    this.providerService.rejectBooking(this.selectedBooking.bookingId, this.rejectReason).subscribe({
      next: () => {
        this.loadBookings();
        this.loadStatistics();
        this.closeRejectModal();
        this.isProcessing = false;
        alert('Booking rejected');
      },
      error: (error) => {
        console.error('Error rejecting booking:', error);
        alert('Failed to reject booking. Please try again.');
        this.isProcessing = false;
      }
    });
  }

  openCompleteModal(booking: Booking): void {
    this.selectedBooking = booking;
    this.showCompleteModal = true;
  }

  closeCompleteModal(): void {
    this.showCompleteModal = false;
    this.selectedBooking = null;
  }

  confirmComplete(): void {
    if (!this.selectedBooking) return;

    this.isProcessing = true;
    this.providerService.completeBooking(this.selectedBooking.bookingId).subscribe({
      next: () => {
        this.loadBookings();
        this.loadStatistics();
        this.closeCompleteModal();
        this.isProcessing = false;
        alert('Booking marked as completed');
      },
      error: (error) => {
        console.error('Error completing booking:', error);
        alert('Failed to complete booking. Please try again.');
        this.isProcessing = false;
      }
    });
  }

  openCancelModal(booking: Booking): void {
    this.selectedBooking = booking;
    this.cancellationReason = '';
    this.showCancelModal = true;
  }

  closeCancelModal(): void {
    this.showCancelModal = false;
    this.selectedBooking = null;
    this.cancellationReason = '';
  }

  confirmCancel(): void {
    if (!this.selectedBooking || !this.cancellationReason) {
      alert('Please provide a reason for cancellation');
      return;
    }

    this.isProcessing = true;
    this.providerService.cancelBooking(this.selectedBooking.bookingId, this.cancellationReason).subscribe({
      next: () => {
        this.loadBookings();
        this.loadStatistics();
        this.closeCancelModal();
        this.isProcessing = false;
        alert('Booking cancelled');
      },
      error: (error) => {
        console.error('Error cancelling booking:', error);
        alert('Failed to cancel booking. Please try again.');
        this.isProcessing = false;
      }
    });
  }

  // Utility methods
  getStatusClass(status: string): string {
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

  getStatusLabel(status: string): string {
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

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'SAR',
      minimumFractionDigits: 2
    }).format(amount);
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

  canAccept(booking: Booking): boolean {
    return booking.status === 'Pending';
  }

  canReject(booking: Booking): boolean {
    return booking.status === 'Pending';
  }

  canComplete(booking: Booking): boolean {
    return booking.status === 'InProgress';
  }

  canCancel(booking: Booking): boolean {
    return booking.status === 'Confirmed' || booking.status === 'InProgress';
  }
}
