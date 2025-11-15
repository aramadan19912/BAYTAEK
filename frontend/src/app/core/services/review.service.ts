import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface Review {
  id: string;
  bookingId: string;
  userId: string;
  userName: string;
  userAvatar?: string;
  serviceId: string;
  serviceName: string;
  providerId: string;
  providerName: string;
  rating: number;
  comment?: string;
  imageUrls?: string[];
  videoUrls?: string[];
  providerResponse?: string;
  providerResponseDate?: Date;
  helpfulCount: number;
  isHelpful?: boolean;
  isVerified: boolean;
  isHidden: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

export interface ReviewStats {
  averageRating: number;
  totalReviews: number;
  ratingDistribution: {
    [key: number]: number;
  };
}

export interface ReviewsResponse {
  items: Review[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  stats?: ReviewStats;
}

export interface CreateReviewRequest {
  bookingId: string;
  rating: number;
  comment?: string;
  imageUrls?: string[];
  videoUrls?: string[];
}

export interface UpdateReviewRequest {
  rating: number;
  comment?: string;
  imageUrls?: string[];
  videoUrls?: string[];
}

export interface RespondToReviewRequest {
  response: string;
}

export enum ReviewModerationAction {
  Approve = 'Approve',
  Reject = 'Reject',
  Hide = 'Hide',
  Show = 'Show'
}

export interface ModerateReviewRequest {
  action: ReviewModerationAction;
  reason?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  constructor(private apiService: ApiService) {}

  /**
   * Create a review for a completed booking
   */
  createReview(request: CreateReviewRequest): Observable<any> {
    return this.apiService.post('/reviews', request);
  }

  /**
   * Update an existing review (within 48 hours)
   */
  updateReview(reviewId: string, request: UpdateReviewRequest): Observable<any> {
    return this.apiService.put(`/reviews/${reviewId}`, request);
  }

  /**
   * Delete a review
   */
  deleteReview(reviewId: string): Observable<any> {
    return this.apiService.delete(`/reviews/${reviewId}`);
  }

  /**
   * Get reviews for a specific service
   */
  getServiceReviews(
    serviceId: string,
    params?: {
      minRating?: number;
      withImagesOnly?: boolean;
      pageNumber?: number;
      pageSize?: number;
    }
  ): Observable<ReviewsResponse> {
    return this.apiService.get<ReviewsResponse>(`/reviews/service/${serviceId}`, params);
  }

  /**
   * Get reviews for a specific provider
   */
  getProviderReviews(
    providerId: string,
    params?: {
      minRating?: number;
      withImagesOnly?: boolean;
      pageNumber?: number;
      pageSize?: number;
    }
  ): Observable<ReviewsResponse> {
    return this.apiService.get<ReviewsResponse>(`/reviews/provider/${providerId}`, params);
  }

  /**
   * Get my reviews as a customer
   */
  getMyReviews(params?: { pageNumber?: number; pageSize?: number }): Observable<ReviewsResponse> {
    return this.apiService.get<ReviewsResponse>('/reviews/my-reviews', params);
  }

  /**
   * Provider responds to a review
   */
  respondToReview(reviewId: string, request: RespondToReviewRequest): Observable<any> {
    return this.apiService.post(`/reviews/${reviewId}/respond`, request);
  }

  /**
   * Mark a review as helpful
   */
  markReviewHelpful(reviewId: string): Observable<any> {
    return this.apiService.post(`/reviews/${reviewId}/helpful`, {});
  }

  /**
   * Admin: Get pending reviews for moderation
   */
  getPendingReviews(params?: {
    onlyUnverified?: boolean;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<ReviewsResponse> {
    return this.apiService.get<ReviewsResponse>('/reviews/admin/pending', params);
  }

  /**
   * Admin: Moderate a review
   */
  moderateReview(reviewId: string, request: ModerateReviewRequest): Observable<any> {
    return this.apiService.post(`/reviews/${reviewId}/moderate`, request);
  }
}
