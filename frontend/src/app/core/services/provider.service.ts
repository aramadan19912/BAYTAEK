import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface ProviderProfile {
  providerId: string;
  businessName?: string;
  businessLicenseNumber?: string;
  yearsOfExperience?: number;
  bio?: string;
  services?: string[];
  serviceRegions?: string[];
  averageRating?: number;
  totalReviews?: number;
  isVerified?: boolean;
  verificationStatus?: string;
  verificationNotes?: string;
  businessType?: string;
  businessDescription?: string;
  commercialRegistrationNumber?: string;
  taxNumber?: string;
  experienceYears?: number;
  verificationDocuments?: string[];
  bankAccountInfo?: string;
  instantBookingEnabled?: boolean;
  acceptsOnlinePayment?: boolean;
  acceptsCashPayment?: boolean;
}

export interface ProviderDashboardStats {
  providerId: string;
  providerName: string;
  averageRating: number;
  totalReviews: number;
  isVerified: boolean;
  todayBookings: number;
  todayCompleted: number;
  weeklyBookings: number;
  weeklyCompleted: number;
  monthlyBookings: number;
  monthlyCompleted: number;
  monthlyRevenue: number;
  monthlyNetEarnings: number;
  availableBalance: number;
  pendingPayouts: number;
  pendingBookingsCount: number;
  unrespondedReviewsCount: number;
  pendingBookings: PendingBookingDto[];
  upcomingBookings: UpcomingBookingDto[];
  recentReviews: RecentReviewDto[];
}

export interface PendingBookingDto {
  bookingId: string;
  serviceName: string;
  scheduledAt: string;
  amount: number;
  createdAt: string;
}

export interface UpcomingBookingDto {
  bookingId: string;
  serviceName: string;
  scheduledAt: string;
  amount: number;
}

export interface RecentReviewDto {
  reviewId: string;
  customerName: string;
  serviceName: string;
  rating: number;
  comment?: string;
  createdAt: string;
  hasResponse: boolean;
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
  id: string;
  categoryId: string;
  categoryNameEn: string;
  categoryNameAr: string;
  nameEn: string;
  nameAr: string;
  descriptionEn: string;
  descriptionAr: string;
  basePrice: number;
  currency: string;
  estimatedDurationMinutes: number;
  availableRegions: string[];
  requiredMaterials?: string;
  warrantyInfo?: string;
  imageUrls: string[];
  videoUrl?: string;
  isActive: boolean;
  isFeatured: boolean;
  totalBookings: number;
  averageRating: number;
  totalReviews: number;
  createdAt: string;
  updatedAt?: string;
}

export interface ProviderBooking {
  id: string;
  status: string;
  customerId: string;
  customerName: string;
  customerPhone: string;
  customerProfileImage?: string;
  serviceId: string;
  serviceName: string;
  address: string;
  latitude?: number;
  longitude?: number;
  scheduledAt: string;
  startedAt?: string;
  completedAt?: string;
  totalAmount: number;
  currency: string;
  isPaid: boolean;
  specialInstructions?: string;
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

  getProviderProfile(): Observable<ProviderProfile> {
    return this.getProfile();
  }

  updateProfile(profile: Partial<ProviderProfile>): Observable<any> {
    return this.apiService.put('provider/profile', profile);
  }

  getDashboardStats(): Observable<ProviderDashboardStats> {
    return this.apiService.get<ProviderDashboardStats>('provider/dashboard');
  }

  // Service Management
  getMyServices(params?: any): Observable<{ services: ProviderService[]; totalCount: number }> {
    return this.apiService.get('provider/services', params);
  }

  getServiceById(serviceId: string): Observable<ProviderService> {
    return this.apiService.get<ProviderService>(`provider/services/${serviceId}`);
  }


  getProviderServices(params?: any): Observable<{ services: ProviderService[]; totalCount: number }> {
    return this.getMyServices(params);
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

  updateServiceStatus(serviceId: string, status: string): Observable<any> {
    return this.apiService.put(`provider/services/${serviceId}/status`, { status });
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

  getProviderBookings(params?: {
    status?: string;
    startDate?: string;
    endDate?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<{ bookings: ProviderBooking[]; totalCount: number }> {
    return this.getBookings(params);
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
