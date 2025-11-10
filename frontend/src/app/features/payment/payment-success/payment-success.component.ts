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
  selector: 'app-payment-success',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    TranslateModule
  ],
  templateUrl: './payment-success.component.html',
  styleUrls: ['./payment-success.component.scss']
})
export class PaymentSuccessComponent implements OnInit {
  bookingId: string = '';
  paymentId: string = '';
  paymentMethod: string = 'Card';
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
      this.paymentId = params['paymentId'] || '';
      this.paymentMethod = params['method'] || 'Card';

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

  viewBookingDetails(): void {
    this.router.navigate(['/bookings', this.bookingId]);
  }

  goToBookings(): void {
    this.router.navigate(['/bookings']);
  }

  goToHome(): void {
    this.router.navigate(['/home']);
  }

  get currentLang(): string {
    return this.translate.currentLang || 'en';
  }

  getServiceName(): string {
    if (!this.booking) return '';
    return this.currentLang === 'ar' ? this.booking.service.nameAr : this.booking.service.nameEn;
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString(this.currentLang === 'ar' ? 'ar-SA' : 'en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getPaymentMethodIcon(): string {
    switch (this.paymentMethod) {
      case 'Card':
        return 'credit_card';
      case 'Cash':
        return 'money';
      case 'Wallet':
        return 'account_balance_wallet';
      default:
        return 'payment';
    }
  }

  getPaymentMethodLabel(): string {
    switch (this.paymentMethod) {
      case 'Card':
        return 'PAYMENT.METHODS.CARD';
      case 'Cash':
        return 'PAYMENT.METHODS.CASH';
      case 'Wallet':
        return 'PAYMENT.METHODS.WALLET';
      default:
        return 'PAYMENT.METHODS.CARD';
    }
  }

  downloadReceipt(): void {
    // TODO: Implement receipt download functionality
    console.log('Download receipt for booking:', this.bookingId);
  }
}
