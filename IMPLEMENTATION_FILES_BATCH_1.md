# Implementation Files - Batch 1: Reviews Feature (Complete)

This document contains all the code needed to implement the Reviews & Ratings feature.

## Files to Create/Update

### 1. Review Form Component TypeScript

**File:** `frontend/src/app/shared/components/review-form/review-form.ts`

```typescript
import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { Review, CreateReviewRequest, UpdateReviewRequest } from '../../../core/services/review.service';

@Component({
  selector: 'app-review-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    TranslateModule
  ],
  templateUrl: './review-form.html',
  styleUrl: './review-form.scss'
})
export class ReviewForm implements OnInit {
  @Input() bookingId?: string;
  @Input() review?: Review; // For editing
  @Input() isEdit = false;

  @Output() submitReview = new EventEmitter<CreateReviewRequest | UpdateReviewRequest>();
  @Output() cancel = new EventEmitter<void>();

  reviewForm!: FormGroup;
  selectedRating = 0;
  hoveredRating = 0;

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.initForm();
    if (this.review && this.isEdit) {
      this.loadReview();
    }
  }

  initForm(): void {
    this.reviewForm = this.fb.group({
      rating: [0, [Validators.required, Validators.min(1), Validators.max(5)]],
      comment: ['', [Validators.maxLength(1000)]]
    });
  }

  loadReview(): void {
    if (this.review) {
      this.selectedRating = this.review.rating;
      this.reviewForm.patchValue({
        rating: this.review.rating,
        comment: this.review.comment || ''
      });
    }
  }

  selectRating(rating: number): void {
    this.selectedRating = rating;
    this.reviewForm.patchValue({ rating });
  }

  hoverRating(rating: number): void {
    this.hoveredRating = rating;
  }

  resetHover(): void {
    this.hoveredRating = 0;
  }

  get ratingArray(): number[] {
    return [1, 2, 3, 4, 5];
  }

  getRatingLabel(rating: number): string {
    const labels = ['', 'reviews.stats.poor', 'reviews.stats.belowAverage',
                    'reviews.stats.average', 'reviews.stats.good', 'reviews.stats.excellent'];
    return labels[rating] || '';
  }

  onSubmit(): void {
    if (this.reviewForm.valid) {
      const formValue = this.reviewForm.value;

      if (this.isEdit) {
        const updateRequest: UpdateReviewRequest = {
          rating: formValue.rating,
          comment: formValue.comment || undefined
        };
        this.submitReview.emit(updateRequest);
      } else {
        const createRequest: CreateReviewRequest = {
          bookingId: this.bookingId!,
          rating: formValue.rating,
          comment: formValue.comment || undefined
        };
        this.submitReview.emit(createRequest);
      }
    }
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
```

### 2. Review Form Component HTML

**File:** `frontend/src/app/shared/components/review-form/review-form.html`

```html
<div class="review-form-container">
  <h2 class="form-title">
    {{ (isEdit ? 'reviews.editReview' : 'reviews.writeReview') | translate }}
  </h2>

  <form [formGroup]="reviewForm" (ngSubmit)="onSubmit()">
    <!-- Rating Selection -->
    <div class="rating-section">
      <label class="rating-label">
        {{ 'reviews.form.rateYourExperience' | translate }} *
      </label>
      <div class="rating-stars">
        @for (star of ratingArray; track star) {
          <button
            type="button"
            class="star-button"
            (click)="selectRating(star)"
            (mouseenter)="hoverRating(star)"
            (mouseleave)="resetHover()"
            [class.selected]="star <= selectedRating"
            [class.hovered]="star <= hoveredRating">
            <mat-icon>
              {{ (star <= (hoveredRating || selectedRating)) ? 'star' : 'star_border' }}
            </mat-icon>
          </button>
        }
      </div>
      <div class="rating-label-text">
        @if (hoveredRating || selectedRating) {
          <span>{{ getRatingLabel(hoveredRating || selectedRating) | translate }}</span>
        }
      </div>
      @if (reviewForm.get('rating')?.hasError('required') && reviewForm.get('rating')?.touched) {
        <div class="error-message">
          {{ 'reviews.ratingRequired' | translate }}
        </div>
      }
    </div>

    <!-- Comment -->
    <mat-form-field appearance="outline" class="full-width">
      <mat-label>{{ 'reviews.form.tellUsMore' | translate }}</mat-label>
      <textarea
        matInput
        formControlName="comment"
        [placeholder]="'reviews.commentPlaceholder' | translate"
        rows="5"
        maxlength="1000"></textarea>
      <mat-hint align="end">
        {{ reviewForm.get('comment')?.value?.length || 0 }} / 1000
      </mat-hint>
    </mat-form-field>

    <!-- Form Actions -->
    <div class="form-actions">
      <button
        mat-stroked-button
        type="button"
        (click)="onCancel()">
        {{ 'common.cancel' | translate }}
      </button>
      <button
        mat-raised-button
        color="primary"
        type="submit"
        [disabled]="!reviewForm.valid">
        <mat-icon>check</mat-icon>
        {{ (isEdit ? 'reviews.update' : 'reviews.submit') | translate }}
      </button>
    </div>
  </form>
</div>
```

### 3. Review Form Component SCSS

**File:** `frontend/src/app/shared/components/review-form/review-form.scss`

```scss
.review-form-container {
  padding: 24px;

  .form-title {
    margin: 0 0 24px;
    font-size: 24px;
    font-weight: 500;
  }

  form {
    .rating-section {
      margin-bottom: 24px;

      .rating-label {
        display: block;
        margin-bottom: 12px;
        font-size: 16px;
        font-weight: 500;
        color: rgba(0, 0, 0, 0.87);
      }

      .rating-stars {
        display: flex;
        gap: 8px;
        margin-bottom: 8px;

        .star-button {
          background: none;
          border: none;
          cursor: pointer;
          padding: 4px;
          transition: transform 0.2s;

          &:hover {
            transform: scale(1.2);
          }

          mat-icon {
            font-size: 36px;
            width: 36px;
            height: 36px;
            color: #e0e0e0;
          }

          &.selected mat-icon,
          &.hovered mat-icon {
            color: #ffc107;
          }
        }
      }

      .rating-label-text {
        min-height: 24px;
        font-size: 14px;
        font-weight: 500;
        color: #2196f3;
      }

      .error-message {
        color: #f44336;
        font-size: 12px;
        margin-top: 4px;
      }
    }

    .full-width {
      width: 100%;
    }

    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: 12px;
      margin-top: 24px;

      button mat-icon {
        margin-right: 8px;
      }
    }
  }
}

// RTL support
[dir='rtl'] {
  .review-form-container {
    .rating-stars {
      flex-direction: row-reverse;
    }

    .form-actions button mat-icon {
      margin-right: 0;
      margin-left: 8px;
    }
  }
}
```

---

## Usage Example

```typescript
// In a parent component (e.g., booking detail page)
<app-review-form
  [bookingId]="booking.id"
  (submitReview)="onSubmitReview($event)"
  (cancel)="onCancelReview()">
</app-review-form>

// In component class
onSubmitReview(reviewData: CreateReviewRequest): void {
  this.reviewService.createReview(reviewData).subscribe({
    next: (response) => {
      this.notificationService.showSuccess('reviews.messages.submitSuccess');
      // Reload reviews or navigate
    },
    error: (error) => {
      this.notificationService.showError('common.error');
    }
  });
}
```

---

Continue to Batch 2 for Review List Component and Reviews Page implementation.
