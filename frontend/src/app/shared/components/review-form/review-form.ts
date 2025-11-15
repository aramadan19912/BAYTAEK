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
