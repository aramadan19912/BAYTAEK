import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PaymentFormComponent, PaymentFormData } from '../../../shared/components/payment-form/payment-form.component';
import { BookingService, BookingDetail } from '../../../core/services/booking.service';
import { PaymentService } from '../../../core/services/payment.service';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-payment-checkout',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDividerModule,
    TranslateModule,
    PaymentFormComponent
  ],
  templateUrl: './payment-checkout.component.html',
  styleUrls: ['./payment-checkout.component.scss']
})
export class PaymentCheckoutComponent implements OnInit {
  bookingId: string = '';
  booking: BookingDetail | null = null;
  loading: boolean = false;
  processingPayment: boolean = false;
  error: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private bookingService: BookingService,
    private paymentService: PaymentService,
    private snackBar: MatSnackBar,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.bookingId = this.route.snapshot.paramMap.get('id') || '';

    if (!this.bookingId) {
      this.router.navigate(['/bookings']);
      return;
    }

    this.loadBookingDetails();
  }

  loadBookingDetails(): void {
    this.loading = true;
    this.error = '';

    this.bookingService.getBookingDetails(this.bookingId)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (booking) => {
          this.booking = booking;

          // Check if booking is already paid
          const paymentStatus = booking.payment?.paymentStatus || booking.payment?.status;
          if (paymentStatus && paymentStatus.toLowerCase() === 'paid') {
            this.translate.get('PAYMENT.ALREADY_PAID').subscribe(msg => {
              this.snackBar.open(msg, '', { duration: 3000 });
            });
            this.router.navigate(['/bookings', this.bookingId]);
          }

          // Check if booking is cancelled
          if (booking.status === 'Cancelled') {
            this.translate.get('PAYMENT.BOOKING_CANCELLED').subscribe(msg => {
              this.snackBar.open(msg, '', { duration: 3000 });
            });
            this.router.navigate(['/bookings', this.bookingId]);
          }
        },
        error: (err) => {
          console.error('Error loading booking:', err);
          this.error = 'PAYMENT.ERRORS.LOAD_BOOKING_FAILED';
          this.translate.get(this.error).subscribe(msg => {
            this.snackBar.open(msg, '', { duration: 5000 });
          });
        }
      });
  }

  onSubmitPayment(paymentData: PaymentFormData): void {
    if (!this.booking) return;

    this.processingPayment = true;

    // Handle Cash or Wallet payment (no payment processing needed)
    if (paymentData.paymentMethod === 'Cash' || paymentData.paymentMethod === 'Wallet') {
      this.router.navigate(['/payment/success'], {
        queryParams: {
          bookingId: this.bookingId,
          method: paymentData.paymentMethod
        }
      });
      return;
    }

    // Create payment intent
    const paymentRequest = {
      bookingId: this.bookingId,
      amount: this.booking.pricing.totalAmount,
      currency: this.booking.pricing.currency,
      paymentMethod: 'Card',
      paymentMethodId: paymentData.paymentMethodId,
      cardNumber: paymentData.cardNumber,
      cardHolderName: paymentData.cardHolderName,
      expiryMonth: paymentData.expiryMonth,
      expiryYear: paymentData.expiryYear,
      cvv: paymentData.cvv,
      saveCard: paymentData.saveCard
    };

    this.paymentService.createPaymentIntent(paymentRequest)
      .pipe(finalize(() => this.processingPayment = false))
      .subscribe({
        next: (response) => {
          // Payment intent created successfully
          if (response.paymentIntentId) {
            this.confirmPayment(response.paymentIntentId);
          } else {
            this.handlePaymentError('PAYMENT.ERRORS.INTENT_CREATION_FAILED');
          }
        },
        error: (err) => {
          console.error('Payment intent creation failed:', err);
          this.handlePaymentError('PAYMENT.ERRORS.PAYMENT_FAILED');
        }
      });
  }

  confirmPayment(paymentIntentId: string): void {
    this.paymentService.confirmPayment({
      paymentIntentId: paymentIntentId,
      bookingId: this.bookingId
    }).subscribe({
      next: (response) => {
        if (response.status === 'Paid' || response.status === 'Succeeded') {
          this.router.navigate(['/payment/success'], {
            queryParams: {
              bookingId: this.bookingId,
              paymentId: response.paymentId
            }
          });
        } else if (response.status === 'RequiresAction') {
          // Handle 3D Secure or additional authentication
          this.handle3DSecure(response);
        } else {
          this.router.navigate(['/payment/failed'], {
            queryParams: {
              bookingId: this.bookingId,
              reason: 'Payment confirmation failed'
            }
          });
        }
      },
      error: (err) => {
        console.error('Payment confirmation failed:', err);
        this.router.navigate(['/payment/failed'], {
          queryParams: {
            bookingId: this.bookingId,
            reason: err.error?.message || 'Payment processing error'
          }
        });
      }
    });
  }

  handle3DSecure(response: any): void {
    // Handle 3D Secure authentication
    // This would typically involve redirecting to the payment provider's 3DS page
    if (response.redirectUrl) {
      window.location.href = response.redirectUrl;
    } else {
      this.handlePaymentError('PAYMENT.ERRORS.AUTHENTICATION_REQUIRED');
    }
  }

  handlePaymentError(errorKey: string): void {
    this.translate.get(errorKey).subscribe(msg => {
      this.snackBar.open(msg, '', { duration: 5000 });
    });

    this.router.navigate(['/payment/failed'], {
      queryParams: {
        bookingId: this.bookingId,
        reason: errorKey
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/bookings', this.bookingId]);
  }

  get currentLang(): string {
    return this.translate.currentLang || 'en';
  }

  getServiceName(): string {
    if (!this.booking) return '';
    return this.currentLang === 'ar' ? this.booking.service.nameAr : this.booking.service.nameEn;
  }

  formatAddress(): string {
    if (!this.booking?.address) return '';

    const addr = this.booking.address;
    const parts = [
      addr.buildingNumber,
      addr.streetName || addr.street,
      addr.district,
      addr.city,
      addr.region,
      addr.postalCode
    ].filter(part => !!part);
    return parts.join(', ');
  }
}
