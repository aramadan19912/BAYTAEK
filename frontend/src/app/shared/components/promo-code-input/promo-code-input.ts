import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import { PromoCodeService } from '../../../core/services/promo-code.service';

@Component({
  selector: 'app-promo-code-input',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    TranslateModule
  ],
  templateUrl: './promo-code-input.html',
  styleUrl: './promo-code-input.scss'
})
export class PromoCodeInput {
  @Output() promoApplied = new EventEmitter<any>();
  @Output() promoRemoved = new EventEmitter<void>();

  promoCode = '';
  loading = false;
  isApplied = false;
  errorMessage = '';
  discount: any = null;

  constructor(private promoCodeService: PromoCodeService) {}

  applyPromoCode(): void {
    if (!this.promoCode.trim()) return;

    this.loading = true;
    this.errorMessage = '';

    this.promoCodeService.validatePromoCode(this.promoCode).subscribe({
      next: (response) => {
        this.loading = false;
        this.isApplied = true;
        this.discount = response.data;
        this.promoApplied.emit(this.discount);
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error.error?.message || 'Invalid promo code';
      }
    });
  }

  removePromoCode(): void {
    this.promoCode = '';
    this.isApplied = false;
    this.discount = null;
    this.errorMessage = '';
    this.promoRemoved.emit();
  }

  onKeyEnter(event: Event): void {
    event.preventDefault();
    if (!this.isApplied && this.promoCode.trim()) {
      this.applyPromoCode();
    }
  }
}
