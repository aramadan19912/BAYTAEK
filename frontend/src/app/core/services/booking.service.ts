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
  completedAt?: string;
  serviceId: string;
  serviceNameEn: string;
  serviceNameAr: string;
  serviceImageUrl?: string;
  serviceName?: string; // Alias for serviceNameEn
  categoryNameEn: string;
  categoryNameAr: string;
  providerId: string;
  providerName: string;
  providerProfileImageUrl?: string;
  providerRating: number;
  isProviderVerified: boolean;
  addressId: string;
  addressLabel: string;
  fullAddress: string;
  servicePrice: number;
  totalAmount: number;
  currency: string;
  isPaid: boolean;
  paymentStatus?: string;
  hasReviewed: boolean;
  customerNotes?: string;
  providerNotes?: string;
}

export interface BookingDetail {
  bookingId: string;
  bookingNumber: string;
  status: string;
  scheduledDateTime: string;
  createdAt: string;
  confirmedAt?: string;
  startedAt?: string;
  completedAt?: string;
  cancelledAt?: string;
  service: {
    serviceId: string;
    nameEn: string;
    nameAr: string;
    descriptionEn?: string;
    descriptionAr?: string;
    imageUrl?: string;
    categoryNameEn: string;
    categoryNameAr: string;
    basePrice: number;
    estimatedDurationMinutes: number;
  };
  provider: {
    providerId: string;
    name: string;
    businessName?: string;
    profileImageUrl?: string;
    phoneNumber?: string;
    email?: string;
    rating: number;
    totalReviews: number;
    isVerified: boolean;
    yearsOfExperience: number;
  };
  address: {
    addressId: string;
    label: string;
    fullAddress: string;
    buildingNumber?: string;
    street?: string;
    streetName?: string; // Alias for street
    district?: string;
    city: string;
    region: string;
    postalCode?: string;
    latitude: number;
    longitude: number;
    additionalInfo?: string;
    additionalDirections?: string; // Alias for additionalInfo
    title?: string; // Alias for label
  };
  pricing: {
    servicePrice: number;
    taxAmount: number;
    discountAmount: number;
    totalAmount: number;
    currency: string;
  };
  payment?: {
    paymentId: string;
    paymentMethod: string;
    status: string;
    paymentStatus?: string; // Alias for status
    amount: number;
    paidAt?: string;
    transactionId?: string;
  };
  review?: {
    reviewId: string;
    rating: number;
    comment?: string;
    images: string[];
    createdAt: string;
    providerResponse?: string;
    providerRespondedAt?: string;
  };
  timeline: TimelineItem[];
  customerNotes?: string;
  providerNotes?: string;
  adminNotes?: string;
  beforePhotos: string[];
  afterPhotos: string[];
  cancellationReason?: string;
  refundAmount?: number;
  refundStatus?: string;
}

export interface TimelineItem {
  status: string;
  timestamp: string;
  notes?: string;
}

export interface BookingHistoryParams {
  status?: string;
  startDate?: string;
  endDate?: string;
  searchTerm?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: string;
}

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  constructor(private apiService: ApiService) {}

  createBooking(request: CreateBookingRequest): Observable<any> {
    return this.apiService.post('bookings', request);
  }

  getBookingHistory(params?: BookingHistoryParams): Observable<BookingHistory> {
    return this.apiService.get<BookingHistory>('bookings/history', params);
  }

  getBookingDetails(bookingId: string): Observable<BookingDetail> {
    return this.apiService.get<BookingDetail>(`bookings/${bookingId}/details`);
  }

  cancelBooking(bookingId: string, reason: string): Observable<any> {
    return this.apiService.post(`bookings/${bookingId}/cancel`, { reason });
  }
}
