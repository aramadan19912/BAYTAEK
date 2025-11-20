import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil, interval } from 'rxjs';
import { BookingService, BookingDetail, TimelineItem } from '../../../core/services/booking.service';
import { MessagingService } from '../../../core/services/messaging.service';
import { ReportsService } from '../../../core/services/reports.service';
import { ServiceService } from '../../../core/services/service.service';
import { AuthService } from '../../../core/services/auth.service';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-booking-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, FormsModule],
  templateUrl: './booking-detail.component.html',
  styleUrls: ['./booking-detail.component.scss']
})
export class BookingDetailComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  bookingId: string = '';
  booking: BookingDetail | null = null;
  loading = true;
  currentLanguage = 'en';

  // Actions
  showCancelModal = false;
  cancellationReason = '';
  cancellingBooking = false;

  showReviewModal = false;
  reviewRating = 5;
  reviewComment = '';
  submittingReview = false;

  showReportModal = false;
  reportReason = '';
  reportDescription = '';
  submittingReport = false;

  // Messaging
  showMessageModal = false;
  messageText = '';
  sendingMessage = false;

  // Available reasons
  cancellationReasons = [
    'Change of plans',
    'Found better service',
    'Schedule conflict',
    'Price too high',
    'Provider not responsive',
    'Other'
  ];

  reportReasons = [
    'No show',
    'Poor service quality',
    'Unprofessional behavior',
    'Overcharging',
    'Safety concern',
    'Other'
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private bookingService: BookingService,
    private messagingService: MessagingService,
    private reportsService: ReportsService,
    private serviceService: ServiceService,
    private authService: AuthService,
    private languageService: LanguageService
  ) {}

  ngOnInit(): void {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth/login']);
      return;
    }

    this.currentLanguage = this.languageService.getCurrentLanguage();

    // Subscribe to language changes
    this.languageService.currentLanguage$
      .pipe(takeUntil(this.destroy$))
      .subscribe(lang => {
        this.currentLanguage = lang;
      });

    // Get booking ID from route
    this.route.params
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        this.bookingId = params['id'];
        this.loadBookingDetails();
      });

    // Auto-refresh every 30 seconds for active bookings
    interval(30000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (this.booking && this.isActiveBooking(this.booking)) {
          this.loadBookingDetails();
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadBookingDetails(): void {
    this.loading = true;
    this.bookingService.getBookingDetails(this.bookingId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (booking) => {
          this.booking = booking;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading booking details:', error);
          this.loading = false;
          this.router.navigate(['/bookings']);
        }
      });
  }

  // Cancel booking
  openCancelModal(): void {
    this.showCancelModal = true;
    this.cancellationReason = '';
  }

  closeCancelModal(): void {
    this.showCancelModal = false;
    this.cancellationReason = '';
  }

  confirmCancellation(): void {
    if (!this.cancellationReason.trim()) {
      alert('Please provide a cancellation reason');
      return;
    }

    this.cancellingBooking = true;
    this.bookingService.cancelBooking(this.bookingId, this.cancellationReason)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.cancellingBooking = false;
          this.closeCancelModal();
          this.loadBookingDetails();
        },
        error: (error) => {
          console.error('Error cancelling booking:', error);
          this.cancellingBooking = false;
          alert('Failed to cancel booking. Please try again.');
        }
      });
  }

  // Review
  openReviewModal(): void {
    this.showReviewModal = true;
    this.reviewRating = 5;
    this.reviewComment = '';
  }

  closeReviewModal(): void {
    this.showReviewModal = false;
    this.reviewRating = 5;
    this.reviewComment = '';
  }

  submitReview(): void {
    if (!this.reviewComment.trim() || !this.booking) {
      alert('Please provide a review comment');
      return;
    }

    this.submittingReview = true;
    this.serviceService.submitReview({
      bookingId: this.booking.bookingId,
      rating: this.reviewRating,
      comment: this.reviewComment
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.submittingReview = false;
          this.closeReviewModal();
          alert('Review submitted successfully!');
        },
        error: (error) => {
          console.error('Error submitting review:', error);
          this.submittingReview = false;
          alert('Failed to submit review. Please try again.');
        }
      });
  }

  // Report
  openReportModal(): void {
    this.showReportModal = true;
    this.reportReason = '';
    this.reportDescription = '';
  }

  closeReportModal(): void {
    this.showReportModal = false;
    this.reportReason = '';
    this.reportDescription = '';
  }

  submitReport(): void {
    if (!this.reportReason || !this.reportDescription.trim()) {
      alert('Please provide a reason and description for your report');
      return;
    }

    this.submittingReport = true;
    this.reportsService.reportBooking(
      this.bookingId,
      this.reportReason,
      this.reportDescription
    )
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.submittingReport = false;
          this.closeReportModal();
          alert('Report submitted successfully. Our team will review it.');
        },
        error: (error) => {
          console.error('Error submitting report:', error);
          this.submittingReport = false;
          alert('Failed to submit report. Please try again.');
        }
      });
  }

  // Messaging
  openMessageModal(): void {
    if (!this.booking) return;
    this.showMessageModal = true;
    this.messageText = '';
  }

  closeMessageModal(): void {
    this.showMessageModal = false;
    this.messageText = '';
  }

  sendMessage(): void {
    if (!this.messageText.trim() || !this.booking) {
      alert('Please enter a message');
      return;
    }

    this.sendingMessage = true;
    this.messagingService.sendMessage({
      receiverId: this.booking.provider.providerId,
      bookingId: this.bookingId,
      message: this.messageText
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.sendingMessage = false;
          this.closeMessageModal();
          alert('Message sent successfully!');
        },
        error: (error) => {
          console.error('Error sending message:', error);
          this.sendingMessage = false;
          alert('Failed to send message. Please try again.');
        }
      });
  }

  // Navigation
  viewService(): void {
    if (this.booking) {
      this.router.navigate(['/services', this.booking.service.serviceId]);
    }
  }

  bookAgain(): void {
    if (this.booking) {
      this.router.navigate(['/services', this.booking.service.serviceId]);
    }
  }

  goToPayment(): void {
    if (this.booking) {
      this.router.navigate(['/bookings', this.bookingId, 'payment']);
    }
  }

  canRetryPayment(): boolean {
    const paymentStatus = this.getPaymentStatusValue();
    if (!paymentStatus) return false;
    return paymentStatus === 'Failed' || paymentStatus === 'Pending';
  }

  isPendingPayment(): boolean {
    const paymentStatus = this.getPaymentStatusValue();
    if (!paymentStatus) return false;
    return paymentStatus === 'Pending' || paymentStatus === 'Failed';
  }

  getPaymentStatusValue(): string {
    if (!this.booking?.payment) return '';
    return this.booking.payment.paymentStatus || this.booking.payment.status || '';
  }

  // Helper methods
  getServiceName(): string {
    if (!this.booking) return '';
    return this.currentLanguage === 'ar'
      ? this.booking.service.nameAr
      : this.booking.service.nameEn;
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString(this.currentLanguage === 'ar' ? 'ar-SA' : 'en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatPrice(price: number): string {
    return `SAR ${price.toFixed(2)}`;
  }

  getStatusClass(status: string): string {
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

  getStatusLabel(status: string): string {
    const statusMap: Record<string, string> = {
      'Pending': 'Pending Confirmation',
      'Confirmed': 'Confirmed',
      'InProgress': 'In Progress',
      'Completed': 'Completed',
      'Cancelled': 'Cancelled',
      'Rejected': 'Rejected'
    };
    return statusMap[status] || status;
  }

  getPaymentStatusClass(status: string): string {
    const statusMap: Record<string, string> = {
      'Pending': 'payment-pending',
      'Paid': 'payment-paid',
      'Failed': 'payment-failed',
      'Refunded': 'payment-refunded'
    };
    return statusMap[status] || 'payment-default';
  }

  getPaymentStatusLabel(status: string): string {
    const statusMap: Record<string, string> = {
      'Pending': 'Payment Pending',
      'Paid': 'Paid',
      'Failed': 'Payment Failed',
      'Refunded': 'Refunded'
    };
    return statusMap[status] || status;
  }

  canCancelBooking(): boolean {
    if (!this.booking) return false;
    return this.booking.status === 'Pending' || this.booking.status === 'Confirmed';
  }

  canReviewBooking(): boolean {
    if (!this.booking) return false;
    return this.booking.status === 'Completed';
  }

  canContactProvider(): boolean {
    if (!this.booking) return false;
    return this.booking.status !== 'Cancelled' && this.booking.status !== 'Rejected';
  }

  isActiveBooking(booking: BookingDetail): boolean {
    return booking.status === 'Pending' ||
           booking.status === 'Confirmed' ||
           booking.status === 'InProgress';
  }

  getTimelineIcon(item: TimelineItem): string {
    const iconMap: Record<string, string> = {
      'Pending': '🕐',
      'Confirmed': '✅',
      'InProgress': '🔄',
      'Completed': '✔️',
      'Cancelled': '❌',
      'Rejected': '🚫'
    };
    return iconMap[item.status] || '📍';
  }

  getStarArray(rating: number): number[] {
    return Array.from({ length: 5 }, (_, i) => i + 1);
  }

  setReviewRating(rating: number): void {
    this.reviewRating = rating;
  }

  getEstimatedCompletionTime(): string {
    if (!this.booking || !this.booking.startedAt) return '';

    const startTime = new Date(this.booking.startedAt);
    const estimatedEnd = new Date(startTime.getTime() + this.booking.service.estimatedDurationMinutes * 60000);

    return estimatedEnd.toLocaleTimeString(this.currentLanguage === 'ar' ? 'ar-SA' : 'en-US', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getTimeUntilScheduled(): string {
    if (!this.booking) return '';

    const now = new Date();
    const scheduled = new Date(this.booking.scheduledDateTime);
    const diffMs = scheduled.getTime() - now.getTime();

    if (diffMs < 0) return 'Past due';

    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    const diffDays = Math.floor(diffHours / 24);

    if (diffDays > 0) {
      return `${diffDays} day${diffDays > 1 ? 's' : ''} from now`;
    } else if (diffHours > 0) {
      return `${diffHours} hour${diffHours > 1 ? 's' : ''} from now`;
    } else {
      const diffMins = Math.floor(diffMs / (1000 * 60));
      return `${diffMins} minute${diffMins > 1 ? 's' : ''} from now`;
    }
  }
}
