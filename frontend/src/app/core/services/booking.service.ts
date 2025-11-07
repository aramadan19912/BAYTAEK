import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CreateBookingRequest {
  serviceId: string;
  providerId: string;
  addressId: string;
  scheduledDateTime: string;
  customerNotes?: string;
  promoCode?: string;
}

export interface BookingHistory {
  bookings: Booking[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalBookings: number;
  activeBookings: number;
  completedBookings: number;
  cancelledBookings: number;
  totalSpent: number;
}

export interface Booking {
  bookingId: string;
  bookingNumber: string;
  status: string;
  scheduledDateTime: string;
  createdAt: string;
  serviceNameEn: string;
  serviceNameAr: string;
  serviceImageUrl?: string;
  providerName: string;
  providerRating: number;
  totalAmount: number;
  currency: string;
  isPaid: boolean;
  hasReviewed: boolean;
}

export interface BookingDetail {
  bookingId: string;
  bookingNumber: string;
  status: string;
  scheduledDateTime: string;
  service: {
    serviceId: string;
    nameEn: string;
    nameAr: string;
    imageUrl?: string;
  };
  provider: {
    providerId: string;
    name: string;
    profileImageUrl?: string;
    rating: number;
    isVerified: boolean;
  };
  pricing: {
    servicePrice: number;
    taxAmount: number;
    discountAmount: number;
    totalAmount: number;
    currency: string;
  };
  timeline: TimelineItem[];
}

export interface TimelineItem {
  status: string;
  timestamp: string;
  notes?: string;
}

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  constructor(private apiService: ApiService) {}

  createBooking(request: CreateBookingRequest): Observable<any> {
    return this.apiService.post('bookings', request);
  }

  getBookingHistory(params?: {
    status?: string;
    startDate?: string;
    endDate?: string;
    searchTerm?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<BookingHistory> {
    return this.apiService.get<BookingHistory>('bookings/history', params);
  }

  getBookingDetails(bookingId: string): Observable<BookingDetail> {
    return this.apiService.get<BookingDetail>(`bookings/${bookingId}/details`);
  }

  cancelBooking(bookingId: string, reason: string): Observable<any> {
    return this.apiService.post(`bookings/${bookingId}/cancel`, { reason });
  }
}
