import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { TranslateModule } from '@ngx-translate/core';
import { ReviewCard } from '../review-card/review-card';
import { ReviewService } from '../../../core/services/review.service';

export interface ReviewFilters {
  sortBy?: 'newest' | 'oldest' | 'highest' | 'lowest' | 'helpful';
  rating?: number;
  verified?: boolean;
}

@Component({
  selector: 'app-review-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatFormFieldModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    TranslateModule,
    ReviewCard
  ],
  templateUrl: './review-list.html',
  styleUrl: './review-list.scss'
})
export class ReviewList implements OnInit {
  @Input() serviceId?: string;
  @Input() providerId?: string;
  @Input() userId?: string;
  @Input() showServiceInfo = false;
  @Input() showActions = true;
  @Input() canRespond = false;
  @Input() pageSize = 10;

  @Output() reviewEdited = new EventEmitter<string>();
  @Output() reviewDeleted = new EventEmitter<string>();
  @Output() reviewResponded = new EventEmitter<{ reviewId: string, response: string }>();

  reviews: any[] = [];
  loading = false;
  loadingMore = false;
  hasMore = false;
  totalCount = 0;
  currentPage = 1;
  currentUserId = ''; // Should be set from auth service

  // Filters
  selectedSort: 'newest' | 'oldest' | 'highest' | 'lowest' | 'helpful' = 'newest';
  selectedRating?: number;
  showVerifiedOnly = false;

  // Stats
  averageRating = 0;
  ratingDistribution: { [key: number]: number } = {
    5: 0,
    4: 0,
    3: 0,
    2: 0,
    1: 0
  };

  constructor(private reviewService: ReviewService) {}

  ngOnInit(): void {
    this.loadReviews();
    this.loadStats();
  }

  loadReviews(append = false): void {
    if (append) {
      this.loadingMore = true;
    } else {
      this.loading = true;
      this.currentPage = 1;
      this.reviews = [];
    }

    const params: any = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortBy: this.selectedSort
    };

    if (this.selectedRating) {
      params.rating = this.selectedRating;
    }

    if (this.showVerifiedOnly) {
      params.verified = true;
    }

    let request$;
    if (this.serviceId) {
      request$ = this.reviewService.getServiceReviews(this.serviceId, params);
    } else if (this.providerId) {
      request$ = this.reviewService.getProviderReviews(this.providerId, params);
    } else if (this.userId) {
      request$ = this.reviewService.getMyReviews(params);
    } else {
      console.error('ReviewList: No ID provided (serviceId, providerId, or userId required)');
      this.loading = false;
      return;
    }

    request$.subscribe({
      next: (response) => {
        if (append) {
          this.reviews = [...this.reviews, ...(response.items || [])];
        } else {
          this.reviews = response.items || [];
        }

        this.totalCount = response.totalCount || 0;
        this.hasMore = (this.currentPage * this.pageSize) < this.totalCount;
        this.loading = false;
        this.loadingMore = false;
      },
      error: (error) => {
        console.error('Error loading reviews:', error);
        this.loading = false;
        this.loadingMore = false;
      }
    });
  }

  loadStats(): void {
    if (!this.serviceId) return;

    this.reviewService.getServiceReviews(this.serviceId, { pageSize: 1 }).subscribe({
      next: (response) => {
        if (response.stats) {
          this.averageRating = response.stats.averageRating || 0;
          this.ratingDistribution = response.stats.ratingDistribution || this.ratingDistribution;
        }
      }
    });
  }

  loadMore(): void {
    this.currentPage++;
    this.loadReviews(true);
  }

  onSortChange(): void {
    this.loadReviews();
  }

  onRatingFilterChange(rating?: number): void {
    this.selectedRating = rating;
    this.loadReviews();
  }

  onVerifiedFilterChange(): void {
    this.showVerifiedOnly = !this.showVerifiedOnly;
    this.loadReviews();
  }

  onMarkHelpful(reviewId: string): void {
    this.reviewService.markReviewHelpful(reviewId).subscribe({
      next: () => {
        const review = this.reviews.find(r => r.id === reviewId);
        if (review) {
          review.helpfulCount = (review.helpfulCount || 0) + 1;
          review.isHelpful = true;
        }
      },
      error: (error) => {
        console.error('Error marking review helpful:', error);
      }
    });
  }

  onRespondToReview(event: { reviewId: string, response: string }): void {
    this.reviewResponded.emit(event);
  }

  onEditReview(reviewId: string): void {
    this.reviewEdited.emit(reviewId);
  }

  onDeleteReview(reviewId: string): void {
    this.reviewDeleted.emit(reviewId);
  }

  getRatingPercentage(rating: number): number {
    if (this.totalCount === 0) return 0;
    return (this.ratingDistribution[rating] / this.totalCount) * 100;
  }

  get filteredCount(): number {
    return this.reviews.length;
  }
}
