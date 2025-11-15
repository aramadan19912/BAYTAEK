import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CreatePaymentIntentRequest {
  bookingId: string;
  paymentMethod: string;
  saveCard?: boolean;
}

export interface PaymentIntent {
  paymentIntentId: string;
  clientSecret: string;
  amount: number;
  currency: string;
  status: string;
}

export interface ConfirmPaymentRequest {
  paymentIntentId: string;
  paymentMethodId?: string;
}

export interface PaymentMethod {
  paymentMethodId: string;
  type: string;
  cardBrand?: string;
  cardLast4?: string;
  cardExpMonth?: number;
  cardExpYear?: number;
  isDefault: boolean;
  createdAt: string;
}

export interface SavePaymentMethodRequest {
  paymentMethodId: string;
  isDefault?: boolean;
}

export interface PaymentHistory {
  paymentId: string;
  bookingId: string;
  bookingNumber: string;
  serviceName: string;
  amount: number;
  currency: string;
  paymentMethod: string;
  status: string;
  transactionId?: string;
  paidAt: string;
  refundedAmount?: number;
  refundedAt?: string;
}

export interface RefundRequest {
  paymentId: string;
  amount?: number;
  reason: string;
}

export interface RefundResponse {
  refundId: string;
  amount: number;
  status: string;
  refundedAt: string;
}

export enum PaymentMethodType {
  Card = 'Card',
  Cash = 'Cash',
  Wallet = 'Wallet'
}

export enum PaymentStatus {
  Pending = 'Pending',
  Processing = 'Processing',
  Succeeded = 'Succeeded',
  Failed = 'Failed',
  Cancelled = 'Cancelled',
  Refunded = 'Refunded',
  PartiallyRefunded = 'PartiallyRefunded'
}

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  constructor(private apiService: ApiService) {}

  // Create payment intent
  createPaymentIntent(request: CreatePaymentIntentRequest): Observable<PaymentIntent> {
    return this.apiService.post<PaymentIntent>('payments/create-intent', request);
  }

  // Confirm payment
  confirmPayment(request: ConfirmPaymentRequest): Observable<any> {
    return this.apiService.post('payments/confirm', request);
  }

  // Get payment by ID
  getPaymentById(paymentId: string): Observable<PaymentHistory> {
    return this.apiService.get<PaymentHistory>(`payments/${paymentId}`);
  }

  // Get payment by booking ID
  getPaymentByBookingId(bookingId: string): Observable<PaymentHistory> {
    return this.apiService.get<PaymentHistory>(`payments/booking/${bookingId}`);
  }

  // Get payment history
  getPaymentHistory(params?: {
    startDate?: string;
    endDate?: string;
    status?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<{ payments: PaymentHistory[]; totalCount: number }> {
    return this.apiService.get('payments/history', params);
  }

  // Request refund
  requestRefund(request: RefundRequest): Observable<RefundResponse> {
    return this.apiService.post<RefundResponse>('payments/refund', request);
  }

  // Get saved payment methods
  getPaymentMethods(): Observable<{ paymentMethods: PaymentMethod[] }> {
    return this.apiService.get<{ paymentMethods: PaymentMethod[] }>('payments/methods');
  }

  // Save payment method
  savePaymentMethod(request: SavePaymentMethodRequest): Observable<any> {
    return this.apiService.post('payments/methods', request);
  }

  // Delete payment method
  deletePaymentMethod(paymentMethodId: string): Observable<any> {
    return this.apiService.delete(`payments/methods/${paymentMethodId}`);
  }

  // Set default payment method
  setDefaultPaymentMethod(paymentMethodId: string): Observable<any> {
    return this.apiService.post(`payments/methods/${paymentMethodId}/set-default`, {});
  }

  // Verify payment status
  verifyPaymentStatus(paymentIntentId: string): Observable<{ status: string; paid: boolean }> {
    return this.apiService.get<{ status: string; paid: boolean }>(`payments/verify/${paymentIntentId}`);
  }

  // Get payment receipt
  getPaymentReceipt(paymentId: string): Observable<Blob> {
    return this.apiService.get(`payments/${paymentId}/receipt`, {}, { responseType: 'blob' }) as Observable<Blob>;
  }

  // Download payment receipt
  downloadPaymentReceipt(paymentId: string, bookingNumber: string): void {
    this.getPaymentReceipt(paymentId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `receipt-${bookingNumber}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Error downloading receipt:', error);
      }
    });
  }

  // Format amount for display
  formatAmount(amount: number, currency: string = 'SAR'): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currency,
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(amount);
  }

  // Get payment method icon
  getPaymentMethodIcon(type: string, brand?: string): string {
    if (type === PaymentMethodType.Card) {
      const brandIcons: Record<string, string> = {
        'visa': 'credit_card',
        'mastercard': 'credit_card',
        'amex': 'credit_card',
        'discover': 'credit_card'
      };
      return brandIcons[brand?.toLowerCase() || ''] || 'credit_card';
    } else if (type === PaymentMethodType.Cash) {
      return 'payments';
    } else if (type === PaymentMethodType.Wallet) {
      return 'account_balance_wallet';
    }
    return 'payment';
  }

  // Get payment status badge class
  getPaymentStatusBadgeClass(status: string): string {
    const statusMap: Record<string, string> = {
      [PaymentStatus.Pending]: 'badge-warning',
      [PaymentStatus.Processing]: 'badge-info',
      [PaymentStatus.Succeeded]: 'badge-success',
      [PaymentStatus.Failed]: 'badge-danger',
      [PaymentStatus.Cancelled]: 'badge-secondary',
      [PaymentStatus.Refunded]: 'badge-primary',
      [PaymentStatus.PartiallyRefunded]: 'badge-info'
    };
    return statusMap[status] || 'badge-secondary';
  }

  // Get payment status label
  getPaymentStatusLabel(status: string): string {
    const statusMap: Record<string, string> = {
      [PaymentStatus.Pending]: 'Pending',
      [PaymentStatus.Processing]: 'Processing',
      [PaymentStatus.Succeeded]: 'Paid',
      [PaymentStatus.Failed]: 'Failed',
      [PaymentStatus.Cancelled]: 'Cancelled',
      [PaymentStatus.Refunded]: 'Refunded',
      [PaymentStatus.PartiallyRefunded]: 'Partially Refunded'
    };
    return statusMap[status] || status;
  }

  // Check if payment can be refunded
  canRefund(payment: PaymentHistory): boolean {
    return (
      payment.status === PaymentStatus.Succeeded &&
      (!payment.refundedAmount || payment.refundedAmount < payment.amount)
    );
  }

  // Calculate remaining refundable amount
  getRefundableAmount(payment: PaymentHistory): number {
    if (!this.canRefund(payment)) {
      return 0;
    }
    return payment.amount - (payment.refundedAmount || 0);
  }

  // Format card number display
  formatCardDisplay(brand: string, last4: string): string {
    return `${this.capitalizeFirstLetter(brand)} •••• ${last4}`;
  }

  // Format card expiry
  formatCardExpiry(month: number, year: number): string {
    return `${month.toString().padStart(2, '0')}/${year.toString().slice(-2)}`;
  }

  // Check if card is expired
  isCardExpired(month: number, year: number): boolean {
    const now = new Date();
    const currentYear = now.getFullYear();
    const currentMonth = now.getMonth() + 1;

    if (year < currentYear) {
      return true;
    }
    if (year === currentYear && month < currentMonth) {
      return true;
    }
    return false;
  }

  // Get available payment methods
  getAvailablePaymentMethods(): { value: string; label: string; icon: string }[] {
    return [
      { value: PaymentMethodType.Card, label: 'Credit/Debit Card', icon: 'credit_card' },
      { value: PaymentMethodType.Cash, label: 'Cash on Delivery', icon: 'payments' },
      { value: PaymentMethodType.Wallet, label: 'Digital Wallet', icon: 'account_balance_wallet' }
    ];
  }

  // Validate card number (Luhn algorithm)
  validateCardNumber(cardNumber: string): boolean {
    const digits = cardNumber.replace(/\D/g, '');
    if (digits.length < 13 || digits.length > 19) {
      return false;
    }

    let sum = 0;
    let isEven = false;

    for (let i = digits.length - 1; i >= 0; i--) {
      let digit = parseInt(digits[i], 10);

      if (isEven) {
        digit *= 2;
        if (digit > 9) {
          digit -= 9;
        }
      }

      sum += digit;
      isEven = !isEven;
    }

    return sum % 10 === 0;
  }

  // Get card brand from number
  getCardBrand(cardNumber: string): string {
    const digits = cardNumber.replace(/\D/g, '');

    if (/^4/.test(digits)) {
      return 'visa';
    } else if (/^5[1-5]/.test(digits)) {
      return 'mastercard';
    } else if (/^3[47]/.test(digits)) {
      return 'amex';
    } else if (/^6(?:011|5)/.test(digits)) {
      return 'discover';
    }

    return 'unknown';
  }

  private capitalizeFirstLetter(str: string): string {
    return str.charAt(0).toUpperCase() + str.slice(1).toLowerCase();
  }
}
