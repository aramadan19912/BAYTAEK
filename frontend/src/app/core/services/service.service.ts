import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface Service {
  id: string;
  nameEn: string;
  nameAr: string;
  descriptionEn?: string;
  descriptionAr?: string;
  basePrice: number;
  averageRating: number;
  totalReviews: number;
  images?: string[];
  categoryNameEn: string;
  categoryNameAr: string;
  providerName: string;
  isVerified: boolean;
}

export interface ServiceDetail {
  id: string;
  nameEn: string;
  nameAr: string;
  descriptionEn?: string;
  descriptionAr?: string;
  basePrice: number;
  estimatedDurationMinutes: number;
  averageRating: number;
  totalReviews: number;
  images?: string[];
  provider: {
    providerId: string;
    name: string;
    businessName?: string;
    rating: number;
    totalReviews: number;
    isVerified: boolean;
    yearsOfExperience: number;
  };
  recentReviews: Review[];
}

export interface Review {
  reviewId: string;
  rating: number;
  comment?: string;
  customerName: string;
  createdAt: string;
  images?: string[];
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
    bookingId: string;
    rating: number;
    comment?: string;
    images?: string[];
    isAnonymous?: boolean;
  }): Observable<any> {
    return this.apiService.post('reviews', review);
  }
}
