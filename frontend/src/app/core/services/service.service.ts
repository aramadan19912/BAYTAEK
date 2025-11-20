import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface Service {
  id: string;
  serviceId?: string;
  nameEn: string;
  nameAr: string;
  descriptionEn?: string;
  descriptionAr?: string;
  basePrice: number;
  estimatedDurationMinutes?: number;
  averageRating: number;
  totalReviews: number;
  images?: string[];
  imageUrls?: string[];
  categoryNameEn: string;
  categoryNameAr: string;
  categoryName?: string;
  providerName: string;
  isVerified: boolean;
  status?: string;
}

export interface ServiceDetail {
  id: string;
  nameEn: string;
  nameAr: string;
  descriptionEn: string;
  descriptionAr: string;
  categoryId: string;
  categoryNameEn: string;
  categoryNameAr: string;
  providerId: string;
  providerName: string;
  providerBusinessName?: string;
  providerAverageRating: number;
  providerTotalReviews: number;
  providerCompletedBookings: number;
  providerIsVerified: boolean;
  basePrice: number;
  currency: string;
  estimatedDurationMinutes: number;
  isActive: boolean;
  isFeatured: boolean;
  imageUrls: string[];
  videoUrl?: string;
  availableRegions: string[];
  requiredMaterials?: string;
  warrantyInfo?: string;
  recentReviews: ServiceReviewDto[];
  totalReviews: number;
  averageRating: number;
  createdAt: string;
  updatedAt?: string;
}

export interface ServiceReviewDto {
  id: string;
  customerName: string;
  rating: number;
  comment?: string;
  imageUrls: string[];
  createdAt: string;
  providerResponse?: string;
}

export interface Review {
  id: string;
  reviewId?: string; // Alias for id
  rating: number;
  comment?: string;
  customerName: string;
  createdAt: string;
  images?: string[];
  imageUrls?: string[];
  providerResponse?: string;
}

export interface ServiceSearchParams {
  categoryId?: string;
  region?: string;
  minPrice?: number;
  maxPrice?: number;
  minRating?: number;
  searchTerm?: string;
  sortBy?: string;
  sortOrder?: string;
  pageNumber?: number;
  pageSize?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ServiceService {
  constructor(private apiService: ApiService) {}

  searchServices(params?: ServiceSearchParams): Observable<{ services: Service[]; totalCount: number }> {
    return this.apiService.get('services', params);
  }

  getServiceById(serviceId: string): Observable<ServiceDetail> {
    return this.apiService.get<ServiceDetail>(`services/${serviceId}`);
  }

  getCategories(): Observable<any[]> {
    return this.apiService.get('categories');
  }

  submitReview(review: {
    serviceId?: string;
    bookingId?: string;
    rating: number;
    comment?: string;
    images?: string[];
    isAnonymous?: boolean;
  }): Observable<any> {
    const payload = { ...review };
    return this.apiService.post('reviews', payload);
  }
}
