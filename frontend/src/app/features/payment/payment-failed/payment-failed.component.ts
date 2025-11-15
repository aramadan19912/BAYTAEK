import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { BookingService, BookingDetail } from '../../../core/services/booking.service';

@Component({
  selector: 'app-payment-failed',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    TranslateModule
  ],
  templateUrl: './payment-failed.component.html',
  styleUrls: ['./payment-failed.component.scss']
})
export class PaymentFailedComponent implements OnInit {
  bookingId: string = '';
  reason: string = '';
  booking: BookingDetail | null = null;
  loading: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private bookingService: BookingService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.bookingId = params['bookingId'] || '';
      this.reason = params['reason'] || '';

      if (!this.bookingId) {
        this.router.navigate(['/bookings']);
        return;
      }

      this.loadBookingDetails();
    });
  }

  loadBookingDetails(): void {
    this.loading = true;

    this.bookingService.getBookingDetails(this.bookingId).subscribe({
      next: (booking) => {
        this.booking = booking;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading booking:', err);
        this.loading = false;
      }
    });
  }

  retryPayment(): void {
    this.router.navigate(['/bookings', this.bookingId, 'payment']);
  }

  viewBookingDetails(): void {
    this.router.navigate(['/bookings', this.bookingId]);
  }

  goToBookings(): void {
    this.router.navigate(['/bookings']);
  }

  contactSupport(): void {
    // TODO: Implement contact support functionality
    console.log('Contact support for booking:', this.bookingId);
  }

  get currentLang(): string {
    return this.translate.currentLang || 'en';
  }

  getServiceName(): string {
    if (!this.booking) return '';
    return this.currentLang === 'ar' ? this.booking.service.nameAr : this.booking.service.nameEn;
  }

  getFailureReason(): string {
    if (!this.reason) return 'PAYMENT.ERRORS.GENERIC_ERROR';

    // Map error reasons to translation keys
    const errorMap: { [key: string]: string } = {
      'insufficient_funds': 'PAYMENT.ERRORS.INSUFFICIENT_FUNDS',
      'card_declined': 'PAYMENT.ERRORS.CARD_DECLINED',
      'expired_card': 'PAYMENT.ERRORS.EXPIRED_CARD',
      'invalid_card': 'PAYMENT.ERRORS.INVALID_CARD',
      'processing_error': 'PAYMENT.ERRORS.PROCESSING_ERROR',
      'authentication_failed': 'PAYMENT.ERRORS.AUTHENTICATION_FAILED',
      'network_error': 'PAYMENT.ERRORS.NETWORK_ERROR'
    };

    return errorMap[this.reason] || 'PAYMENT.ERRORS.GENERIC_ERROR';
  }

  getCommonSolutions(): string[] {
    return [
      'PAYMENT.SOLUTIONS.CHECK_CARD_DETAILS',
      'PAYMENT.SOLUTIONS.CHECK_BALANCE',
      'PAYMENT.SOLUTIONS.TRY_DIFFERENT_CARD',
      'PAYMENT.SOLUTIONS.CONTACT_BANK',
      'PAYMENT.SOLUTIONS.TRY_AGAIN_LATER'
    ];
  }
}
