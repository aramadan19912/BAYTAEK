import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PaymentService, PaymentMethod } from '../../core/services/payment.service';

@Component({
  selector: 'app-payment-methods',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    TranslateModule
  ],
  templateUrl: './payment-methods.html',
  styleUrl: './payment-methods.scss'
})
export class PaymentMethods implements OnInit {
  paymentMethods: PaymentMethod[] = [];
  loading = false;

  constructor(
    private paymentService: PaymentService,
    private snackBar: MatSnackBar,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadPaymentMethods();
  }

  loadPaymentMethods(): void {
    this.loading = true;
    this.paymentService.getPaymentMethods().subscribe({
      next: (result) => {
        this.paymentMethods = result.paymentMethods || [];
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading payment methods:', error);
        this.loading = false;
      }
    });
  }

  addPaymentMethod(): void {
    // For now, show a message. In production, integrate Stripe Elements
    this.showMessage('payment.addCardComingSoon');

    // TODO: Implement Stripe Elements integration
    // Example implementation:
    // 1. Open dialog with Stripe Elements card form
    // 2. Create payment method with Stripe
    // 3. Save payment method to backend
    // 4. Refresh payment methods list
  }

  setAsDefault(method: PaymentMethod): void {
    if (method.isDefault) return;

    this.paymentService.setDefaultPaymentMethod(method.paymentMethodId).subscribe({
      next: () => {
        this.showMessage('payment.defaultPaymentMethodSet');
        this.loadPaymentMethods();
      },
      error: (error) => {
        console.error('Error setting default payment method:', error);
        this.showMessage('payment.errorSettingDefault');
      }
    });
  }

  deletePaymentMethod(method: PaymentMethod): void {
    if (confirm(this.translate.instant('payment.confirmDeletePaymentMethod'))) {
      this.paymentService.deletePaymentMethod(method.paymentMethodId).subscribe({
        next: () => {
          this.showMessage('payment.paymentMethodDeleted');
          this.loadPaymentMethods();
        },
        error: (error) => {
          console.error('Error deleting payment method:', error);
          this.showMessage('payment.errorDeleting');
        }
      });
    }
  }

  getCardBrandIcon(brand?: string): string {
    const brandIcons: Record<string, string> = {
      'visa': 'payment',
      'mastercard': 'payment',
      'amex': 'payment',
      'discover': 'payment'
    };
    return brandIcons[brand?.toLowerCase() || ''] || 'credit_card';
  }

  getCardBrandClass(brand?: string): string {
    return `card-brand-${brand?.toLowerCase() || 'default'}`;
  }

  formatExpiry(month?: number, year?: number): string {
    if (!month || !year) return '';
    return `${String(month).padStart(2, '0')}/${year}`;
  }

  isExpiringSoon(year: number, month: number): boolean {
    const now = new Date();
    const currentYear = now.getFullYear();
    const currentMonth = now.getMonth() + 1;
    const expiryDate = new Date(year, month - 1);
    const threeMonthsFromNow = new Date();
    threeMonthsFromNow.setMonth(threeMonthsFromNow.getMonth() + 3);

    return expiryDate <= threeMonthsFromNow && expiryDate > now;
  }

  private showMessage(key: string): void {
    this.snackBar.open(this.translate.instant(key), '', {
      duration: 3000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom'
    });
  }
}
