import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface ValidatePromoCodeRequest {
  code: string;
  serviceId?: string;
  categoryId?: string;
  subtotalAmount: number;
}

export interface PromoCodeValidation {
  isValid: boolean;
  promoCodeId?: string;
  code?: string;
  discountType?: string;
  discountValue?: number;
  discountAmount?: number;
  finalAmount?: number;
  message: string;
  errors?: string[];
}

export interface PromoCode {
  promoCodeId: string;
  code: string;
  discountType: string;
  discountValue: number;
  maxDiscountAmount?: number;
  minOrderAmount?: number;
  maxUsageCount?: number;
  currentUsageCount: number;
  maxUsagePerUser?: number;
  validFrom: string;
  validUntil: string;
  isActive: boolean;
  applicableUserRole?: string;
  applicableToNewUsersOnly?: boolean;
  applicableCategories?: string[];
  applicableServices?: string[];
  createdAt: string;
}

export interface CreatePromoCodeRequest {
  code: string;
  discountType: string;
  discountValue: number;
  maxDiscountAmount?: number;
  minOrderAmount?: number;
  maxUsageCount?: number;
  maxUsagePerUser?: number;
  validFrom: string;
  validUntil: string;
  isActive?: boolean;
  applicableUserRole?: string;
  applicableToNewUsersOnly?: boolean;
  applicableCategories?: string[];
  applicableServices?: string[];
}

export interface UpdatePromoCodeStatusRequest {
  isActive: boolean;
  reason?: string;
}

export enum DiscountType {
  Percentage = 'Percentage',
  FixedAmount = 'FixedAmount',
  FreeDelivery = 'FreeDelivery'
}

@Injectable({
  providedIn: 'root'
})
export class PromoCodeService {
  constructor(private apiService: ApiService) {}

  // Validate a promo code
  validatePromoCode(request: ValidatePromoCodeRequest): Observable<PromoCodeValidation> {
    return this.apiService.post<PromoCodeValidation>('promocodes/validate', request);
  }

  // Quick validate (simplified version for inline checking)
  quickValidate(code: string, subtotalAmount: number): Observable<PromoCodeValidation> {
    return this.validatePromoCode({
      code,
      subtotalAmount
    });
  }

  // Calculate discount amount based on promo code
  calculateDiscount(
    discountType: string,
    discountValue: number,
    subtotalAmount: number,
    maxDiscountAmount?: number
  ): number {
    let discount = 0;

    switch (discountType) {
      case DiscountType.Percentage:
        discount = (subtotalAmount * discountValue) / 100;
        if (maxDiscountAmount && discount > maxDiscountAmount) {
          discount = maxDiscountAmount;
        }
        break;
      case DiscountType.FixedAmount:
        discount = discountValue;
        if (discount > subtotalAmount) {
          discount = subtotalAmount; // Can't discount more than the subtotal
        }
        break;
      case DiscountType.FreeDelivery:
        // Frontend would need to know delivery fee to calculate this
        discount = 0; // Backend will handle the actual delivery fee discount
        break;
    }

    return discount;
  }

  // Get available discount types
  getDiscountTypes(): { value: string; label: string }[] {
    return [
      { value: DiscountType.Percentage, label: 'Percentage' },
      { value: DiscountType.FixedAmount, label: 'Fixed Amount' },
      { value: DiscountType.FreeDelivery, label: 'Free Delivery' }
    ];
  }

  // Format discount display text
  formatDiscountDisplay(discountType: string, discountValue: number): string {
    switch (discountType) {
      case DiscountType.Percentage:
        return `${discountValue}% OFF`;
      case DiscountType.FixedAmount:
        return `SAR ${discountValue} OFF`;
      case DiscountType.FreeDelivery:
        return 'FREE DELIVERY';
      default:
        return '';
    }
  }

  // Check if promo code is currently valid (date range check)
  isPromoCodeDateValid(validFrom: string, validUntil: string): boolean {
    const now = new Date();
    const from = new Date(validFrom);
    const until = new Date(validUntil);
    return now >= from && now <= until;
  }

  // Get user-friendly validation error message
  getValidationErrorMessage(errors: string[]): string {
    if (!errors || errors.length === 0) {
      return 'Invalid promo code';
    }

    const errorMap: Record<string, string> = {
      'code_not_found': 'Promo code not found',
      'code_expired': 'This promo code has expired',
      'code_not_started': 'This promo code is not yet active',
      'code_inactive': 'This promo code is no longer active',
      'max_usage_reached': 'This promo code has reached its usage limit',
      'user_limit_reached': 'You have already used this promo code the maximum number of times',
      'below_min_amount': 'Order amount is below the minimum required for this promo code',
      'wrong_user_role': 'This promo code is not applicable to your account type',
      'new_users_only': 'This promo code is only available for new users',
      'category_not_applicable': 'This promo code is not applicable to the selected service category',
      'service_not_applicable': 'This promo code is not applicable to the selected service'
    };

    return errors.map(error => errorMap[error] || error).join('. ');
  }
}
