import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface ProviderProfile {
  providerId: string;
  businessName?: string;
  businessLicenseNumber?: string;
  yearsOfExperience: number;
  bio?: string;
  services: string[];
  serviceRegions: string[];
  averageRating: number;
  totalReviews: number;
  isVerified: boolean;
  verificationStatus: string;
  instantBookingEnabled: boolean;
  acceptsOnlinePayment: boolean;
  acceptsCashPayment: boolean;
}

export interface CreateServiceRequest {
  nameEn: string;
  nameAr: string;
  descriptionEn?: string;
  descriptionAr?: string;
  categoryId: string;
  basePrice: number;
  estimatedDurationMinutes: number;
  serviceRegions: string[];
  images?: string[];
  isActive?: boolean;
}

export interface UpdateServiceRequest {
  nameEn?: string;
  nameAr?: string;
  descriptionEn?: string;
  descriptionAr?: string;
  basePrice?: number;
  estimatedDurationMinutes?: number;
  serviceRegions?: string[];
  images?: string[];
  isActive?: boolean;
}

export interface ProviderService {
  serviceId: string;
  nameEn: string;
  nameAr: string;
  descriptionEn?: string;
  descriptionAr?: string;
  categoryNameEn: string;
  categoryNameAr: string;
  basePrice: number;
  estimatedDurationMinutes: number;
  averageRating: number;
  totalReviews: number;
  totalBookings: number;
  images?: string[];
  isActive: boolean;
  createdAt: string;
}

export interface ProviderBooking {
  bookingId: string;
  bookingNumber: string;
  serviceName: string;
  customerName: string;
  customerPhone?: string;
  customerAddress: string;
  scheduledDateTime: string;
  status: string;
  totalAmount: number;
  paymentStatus: string;
  customerNotes?: string;
  createdAt: string;
}

export interface ProviderBookingDetail {
  bookingId: string;
  bookingNumber: string;
  status: string;
  service: {
    serviceId: string;
    nameEn: string;
    nameAr: string;
    basePrice: number;
    estimatedDurationMinutes: number;
  };
  customer: {
    customerId: string;
    name: string;
    email: string;
    phone?: string;
  };
  address: {
    addressLine: string;
    region: string;
    city: string;
    additionalDetails?: string;
    coordinates?: {
      latitude: number;
      longitude: number;
    };
  };
  scheduledDateTime: string;
  startedAt?: string;
  completedAt?: string;
  cancelledAt?: string;
  pricing: {
    basePrice: number;
    promoDiscount: number;
    totalAmount: number;
    platformFee: number;
    providerEarnings: number;
  };
  payment: {
    paymentMethod: string;
    paymentStatus: string;
    transactionId?: string;
    paidAt?: string;
  };
  customerNotes?: string;
  providerNotes?: string;
  cancellationReason?: string;
  timeline: TimelineItem[];
}

export interface TimelineItem {
  status: string;
  timestamp: string;
  actor?: string;
  notes?: string;
}

export interface UpdateBookingStatusRequest {
  status: string;
  notes?: string;
}

export interface ProviderAnalytics {
  earnings: {
    totalEarnings: number;
    pendingPayout: number;
    completedPayouts: number;
    platformFees: number;
    refunds: number;
    thisMonthEarnings: number;
    lastMonthEarnings: number;
    growthPercentage: number;
  };
  bookings: {
    totalBookings: number;
    completedBookings: number;
    cancelledBookings: number;
    inProgressBookings: number;
    upcomingBookings: number;
    cancellationRate: number;
    completionRate: number;
  };
  performance: {
    averageRating: number;
    totalReviews: number;
    responseTimeMinutes: number;
    acceptanceRate: number;
    onTimePercentage: number;
  };
  customers: {
    totalCustomers: number;
    repeatCustomers: number;
    repeatRate: number;
    newCustomersThisMonth: number;
  };
  topServices: ServicePerformance[];
  dailyTrends: DailyTrend[];
}

export interface ServicePerformance {
  serviceId: string;
  serviceName: string;
  bookings: number;
  revenue: number;
  averageRating: number;
}

export interface DailyTrend {
  date: string;
  bookings: number;
  earnings: number;
  completedBookings: number;
}

export interface RequestVerificationRequest {
  businessLicenseNumber: string;
  licenseExpiryDate: string;
  businessRegistrationDocument?: string;
  identityDocument?: string;
  proofOfAddress?: string;
  additionalDocuments?: string[];
}

export interface AvailabilitySlot {
  dayOfWeek: number;
  startTime: string;
  endTime: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProviderService {
  constructor(private apiService: ApiService) {}

  // Profile Management
  getProfile(): Observable<ProviderProfile> {
    return this.apiService.get<ProviderProfile>('provider/profile');
  }

  updateProfile(profile: Partial<ProviderProfile>): Observable<any> {
    return this.apiService.put('provider/profile', profile);
  }

  // Service Management
  getMyServices(): Observable<{ services: ProviderService[]; totalCount: number }> {
    return this.apiService.get('provider/services');
  }

  getServiceById(serviceId: string): Observable<ProviderService> {
    return this.apiService.get<ProviderService>(`provider/services/${serviceId}`);
  }

  createService(service: CreateServiceRequest): Observable<any> {
    return this.apiService.post('provider/services', service);
  }

  updateService(serviceId: string, service: UpdateServiceRequest): Observable<any> {
    return this.apiService.put(`provider/services/${serviceId}`, service);
  }

  deleteService(serviceId: string): Observable<any> {
    return this.apiService.delete(`provider/services/${serviceId}`);
  }

  toggleServiceStatus(serviceId: string, isActive: boolean): Observable<any> {
    return this.apiService.put(`provider/services/${serviceId}/status`, { isActive });
  }

  // Booking Management
  getBookings(params?: {
    status?: string;
    startDate?: string;
    endDate?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<{ bookings: ProviderBooking[]; totalCount: number }> {
    return this.apiService.get('provider/bookings', params);
  }

  getBookingDetails(bookingId: string): Observable<ProviderBookingDetail> {
    return this.apiService.get<ProviderBookingDetail>(`provider/bookings/${bookingId}`);
  }

  updateBookingStatus(bookingId: string, request: UpdateBookingStatusRequest): Observable<any> {
    return this.apiService.put(`provider/bookings/${bookingId}/status`, request);
  }

  acceptBooking(bookingId: string): Observable<any> {
    return this.apiService.post(`provider/bookings/${bookingId}/accept`, {});
  }

  rejectBooking(bookingId: string, reason: string): Observable<any> {
    return this.apiService.post(`provider/bookings/${bookingId}/reject`, { reason });
  }

  startService(bookingId: string): Observable<any> {
    return this.apiService.post(`provider/bookings/${bookingId}/start`, {});
  }

  completeService(bookingId: string, notes?: string): Observable<any> {
    return this.apiService.post(`provider/bookings/${bookingId}/complete`, { notes });
  }

  // Analytics
  getAnalytics(startDate?: string, endDate?: string): Observable<ProviderAnalytics> {
    return this.apiService.get<ProviderAnalytics>('provider/analytics', {
      startDate,
      endDate
    });
  }

  // Verification
  requestVerification(request: RequestVerificationRequest): Observable<any> {
    return this.apiService.post('provider/verification/request', request);
  }

  getVerificationStatus(): Observable<any> {
    return this.apiService.get('provider/verification/status');
  }

  // Availability Management
  getAvailability(): Observable<AvailabilitySlot[]> {
    return this.apiService.get<AvailabilitySlot[]>('provider/availability');
  }

  updateAvailability(slots: AvailabilitySlot[]): Observable<any> {
    return this.apiService.put('provider/availability', { slots });
  }

  // Earnings & Payouts
  getEarnings(params?: {
    startDate?: string;
    endDate?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<any> {
    return this.apiService.get('provider/earnings', params);
  }

  requestPayout(amount: number, isInstant: boolean = false): Observable<any> {
    return this.apiService.post('provider/payouts/request', { amount, isInstant });
  }

  getPayouts(params?: {
    status?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<any> {
    return this.apiService.get('provider/payouts', params);
  }
}
