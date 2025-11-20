import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatRadioModule } from '@angular/material/radio';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { PaymentService, PaymentMethod } from '../../../core/services/payment.service';
import { TranslateModule } from '@ngx-translate/core';

export interface PaymentFormData {
  paymentMethod: 'Card' | 'Cash' | 'Wallet';
  paymentMethodId?: string;
  cardNumber?: string;
  cardHolderName?: string;
  expiryMonth?: string;
  expiryYear?: string;
  cvv?: string;
  saveCard?: boolean;
}

@Component({
  selector: 'app-payment-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatRadioModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatDividerModule,
    MatCheckboxModule,
    TranslateModule
  ],
  templateUrl: './payment-form.component.html',
  styleUrls: ['./payment-form.component.scss']
})
export class PaymentFormComponent implements OnInit {
  @Input() amount: number = 0;
  @Input() currency: string = 'SAR';
  @Input() loading: boolean = false;
  @Output() submitPayment = new EventEmitter<PaymentFormData>();
  @Output() cancel = new EventEmitter<void>();

  paymentForm!: FormGroup;
  savedPaymentMethods: PaymentMethod[] = [];
  loadingMethods: boolean = false;
  selectedPaymentType: 'Card' | 'Cash' | 'Wallet' | 'SavedCard' = 'Card';
  cardBrand: string = '';

  constructor(
    private fb: FormBuilder,
    private paymentService: PaymentService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadSavedPaymentMethods();
    this.setupCardValidation();
  }

  initForm(): void {
    this.paymentForm = this.fb.group({
      paymentType: ['Card', Validators.required],
      savedPaymentMethodId: [''],
      cardNumber: ['', [Validators.required, Validators.minLength(13), Validators.maxLength(19)]],
      cardHolderName: ['', [Validators.required, Validators.minLength(3)]],
      expiryMonth: ['', [Validators.required, Validators.min(1), Validators.max(12)]],
      expiryYear: ['', [Validators.required, Validators.min(2024)]],
      cvv: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(4)]],
      saveCard: [false]
    });

    // Update form validation based on payment type
    this.paymentForm.get('paymentType')?.valueChanges.subscribe(type => {
      this.selectedPaymentType = type;
      this.updateFormValidation();
    });
  }

  loadSavedPaymentMethods(): void {
    this.loadingMethods = true;
    this.paymentService.getPaymentMethods().subscribe({
      next: (methods) => {
        this.savedPaymentMethods = methods;
        this.loadingMethods = false;
      },
      error: () => {
        this.loadingMethods = false;
      }
    });
  }
  getPaymentMethodExpiry(method: PaymentMethod): string {
    const month = method.expiryMonth ?? method.cardExpMonth;
    const year = method.expiryYear ?? method.cardExpYear;
    if (!month || !year) {
      return 'N/A';
    }
    return `${month.toString().padStart(2, '0')}/${year.toString().slice(-2)}`;
  }

  setupCardValidation(): void {
    const cardNumberControl = this.paymentForm.get('cardNumber');

    cardNumberControl?.valueChanges.subscribe(value => {
      if (value) {
        // Remove spaces and non-digits
        const cleaned = value.replace(/\s+/g, '').replace(/[^0-9]/g, '');

        // Detect card brand
        this.cardBrand = this.paymentService.getCardBrand(cleaned);

        // Format card number with spaces
        const formatted = cleaned.match(/.{1,4}/g)?.join(' ') || cleaned;

        if (formatted !== value) {
          cardNumberControl.setValue(formatted, { emitEvent: false });
        }
      }
    });
  }

  updateFormValidation(): void {
    const cardFields = ['cardNumber', 'cardHolderName', 'expiryMonth', 'expiryYear', 'cvv'];

    if (this.selectedPaymentType === 'Card') {
      // Enable card field validations
      cardFields.forEach(field => {
        this.paymentForm.get(field)?.enable();
        this.paymentForm.get(field)?.setValidators([Validators.required]);
      });
      this.paymentForm.get('savedPaymentMethodId')?.clearValidators();
    } else if (this.selectedPaymentType === 'SavedCard') {
      // Disable card fields and require saved method
      cardFields.forEach(field => {
        this.paymentForm.get(field)?.disable();
        this.paymentForm.get(field)?.clearValidators();
      });
      this.paymentForm.get('savedPaymentMethodId')?.setValidators([Validators.required]);
    } else {
      // Cash or Wallet - disable all card fields
      cardFields.forEach(field => {
        this.paymentForm.get(field)?.disable();
        this.paymentForm.get(field)?.clearValidators();
      });
      this.paymentForm.get('savedPaymentMethodId')?.clearValidators();
    }

    this.paymentForm.updateValueAndValidity();
  }

  onSubmit(): void {
    if (this.selectedPaymentType === 'Cash' || this.selectedPaymentType === 'Wallet') {
      // For cash or wallet, submit immediately
      this.submitPayment.emit({
        paymentMethod: this.selectedPaymentType
      });
      return;
    }

    if (this.selectedPaymentType === 'SavedCard') {
      const methodId = this.paymentForm.get('savedPaymentMethodId')?.value;
      if (methodId) {
        this.submitPayment.emit({
          paymentMethod: 'Card',
          paymentMethodId: methodId
        });
      }
      return;
    }

    // New card payment
    if (this.paymentForm.valid) {
      const cardNumber = this.paymentForm.get('cardNumber')?.value.replace(/\s+/g, '');

      // Validate card using Luhn algorithm
      if (!this.paymentService.validateCardNumber(cardNumber)) {
        this.paymentForm.get('cardNumber')?.setErrors({ invalidCard: true });
        return;
      }

      // Validate expiry date
      const month = parseInt(this.paymentForm.get('expiryMonth')?.value);
      const year = parseInt(this.paymentForm.get('expiryYear')?.value);
      const now = new Date();
      const currentYear = now.getFullYear();
      const currentMonth = now.getMonth() + 1;

      if (year < currentYear || (year === currentYear && month < currentMonth)) {
        this.paymentForm.get('expiryMonth')?.setErrors({ expired: true });
        this.paymentForm.get('expiryYear')?.setErrors({ expired: true });
        return;
      }

      const formData: PaymentFormData = {
        paymentMethod: 'Card',
        cardNumber: cardNumber,
        cardHolderName: this.paymentForm.get('cardHolderName')?.value,
        expiryMonth: month.toString().padStart(2, '0'),
        expiryYear: year.toString(),
        cvv: this.paymentForm.get('cvv')?.value,
        saveCard: this.paymentForm.get('saveCard')?.value
      };

      this.submitPayment.emit(formData);
    } else {
      this.markFormGroupTouched(this.paymentForm);
    }
  }

  onCancel(): void {
    this.cancel.emit();
  }

  getCardIcon(): string {
    switch (this.cardBrand) {
      case 'Visa':
        return 'credit_card';
      case 'Mastercard':
        return 'credit_card';
      case 'American Express':
        return 'credit_card';
      default:
        return 'payment';
    }
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  get cardNumberError(): string {
    const control = this.paymentForm.get('cardNumber');
    if (control?.hasError('required') && control?.touched) {
      return 'PAYMENT.ERRORS.CARD_NUMBER_REQUIRED';
    }
    if (control?.hasError('invalidCard') && control?.touched) {
      return 'PAYMENT.ERRORS.INVALID_CARD';
    }
    return '';
  }

  get expiryError(): string {
    const monthControl = this.paymentForm.get('expiryMonth');
    const yearControl = this.paymentForm.get('expiryYear');
    if (monthControl?.hasError('expired') || yearControl?.hasError('expired')) {
      return 'PAYMENT.ERRORS.EXPIRED_CARD';
    }
    return '';
  }
}
