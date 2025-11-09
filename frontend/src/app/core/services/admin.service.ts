import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface UserListItem {
  userId: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  role: string;
  isActive: boolean;
  isSuspended: boolean;
  suspendedUntil?: string;
  emailConfirmed: boolean;
  phoneConfirmed: boolean;
  registeredAt: string;
  lastLoginAt?: string;
}

export interface UserDetail {
  userId: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  role: string;
  isActive: boolean;
  isSuspended: boolean;
  suspendedUntil?: string;
  emailConfirmed: boolean;
  phoneConfirmed: boolean;
  registeredAt: string;
  lastLoginAt?: string;
  totalBookings?: number;
  totalSpent?: number;
  averageRating?: number;
  totalReviews?: number;
  reportCount?: number;
}

export interface SuspendUserRequest {
  reason: string;
  suspensionDays?: number;
  notifyUser?: boolean;
}

export interface ProviderVerificationRequest {
  providerId: string;
  businessName: string;
  businessLicenseNumber: string;
  licenseExpiryDate: string;
  yearsOfExperience: number;
  status: string;
  requestedAt: string;
  documents: {
    businessRegistrationDocument?: string;
    identityDocument?: string;
    proofOfAddress?: string;
    additionalDocuments?: string[];
  };
}

export interface ApproveVerificationRequest {
  verificationNotes?: string;
  verifiedBadge?: boolean;
}

export interface RejectVerificationRequest {
  rejectionReason: string;
  feedback?: string;
}

export interface ServiceApprovalItem {
  serviceId: string;
  nameEn: string;
  nameAr: string;
  providerName: string;
  providerId: string;
  categoryName: string;
  basePrice: number;
  status: string;
  submittedAt: string;
  images?: string[];
}

export interface AdminTicket {
  ticketId: string;
  ticketNumber: string;
  subject: string;
  category: string;
  priority: string;
  status: string;
  userId: string;
  userName: string;
  userEmail: string;
  lastMessageAt: string;
  createdAt: string;
  messagesCount: number;
}

export interface TicketStatistics {
  totalTickets: number;
  openTickets: number;
  inProgressTickets: number;
  resolvedTickets: number;
  closedTickets: number;
  averageResolutionTimeHours: number;
  ticketsByCategory: Record<string, number>;
  ticketsByPriority: Record<string, number>;
}

export interface ContentItem {
  contentId: string;
  type: string;
  slug: string;
  titleEn: string;
  titleAr: string;
  isPublished: boolean;
  order: number;
  viewCount: number;
  lastPublishedAt?: string;
  updatedAt: string;
}

export interface CreateContentRequest {
  type: string;
  titleEn: string;
  titleAr: string;
  bodyEn: string;
  bodyAr: string;
  slug?: string;
  metaTitleEn?: string;
  metaTitleAr?: string;
  metaDescriptionEn?: string;
  metaDescriptionAr?: string;
  isPublished?: boolean;
  order?: number;
  category?: string;
}

export interface UpdateContentRequest {
  titleEn?: string;
  titleAr?: string;
  bodyEn?: string;
  bodyAr?: string;
  slug?: string;
  metaTitleEn?: string;
  metaTitleAr?: string;
  metaDescriptionEn?: string;
  metaDescriptionAr?: string;
  isPublished?: boolean;
  order?: number;
  category?: string;
}

export interface ReportListItem {
  reportId: string;
  reportNumber: string;
  type: string;
  reason: string;
  status: string;
  reporterName: string;
  reportedEntityName?: string;
  priority: string;
  createdAt: string;
  reviewedAt?: string;
}

export interface ReportDetail {
  reportId: string;
  reportNumber: string;
  type: string;
  reason: string;
  status: string;
  description: string;
  reporterId: string;
  reporterName: string;
  reporterEmail: string;
  reporterPhone?: string;
  reporterRole: string;
  reportedUserId?: string;
  reportedUserName?: string;
  reportedUserEmail?: string;
  reportedUserRole?: string;
  reportedUserViolationCount?: number;
  reportedBookingId?: string;
  reportedBookingNumber?: string;
  reportedReviewId?: string;
  reportedReviewRating?: number;
  reportedReviewComment?: string;
  reportedServiceId?: string;
  reportedServiceName?: string;
  evidence: string[];
  reviewedBy?: string;
  reviewedByName?: string;
  reviewedAt?: string;
  actionTaken?: string;
  adminNotes?: string;
  createdAt: string;
  resolvedAt?: string;
  relatedReportsCount: number;
}

export interface ReviewReportRequest {
  status: string;
  actionTaken?: string;
  adminNotes?: string;
  suspendUser?: boolean;
  suspensionDays?: number;
  sendWarning?: boolean;
  warningMessage?: string;
}

export interface SystemAnalytics {
  users: {
    totalUsers: number;
    totalCustomers: number;
    totalProviders: number;
    activeUsers: number;
    newUsersThisMonth: number;
  };
  bookings: {
    totalBookings: number;
    completedBookings: number;
    activeBookings: number;
    cancelledBookings: number;
    totalRevenue: number;
    platformEarnings: number;
  };
  services: {
    totalServices: number;
    activeServices: number;
    pendingApproval: number;
    averageServicePrice: number;
  };
  support: {
    openTickets: number;
    resolvedTickets: number;
    averageResolutionTimeHours: number;
  };
}

export interface DashboardStats {
  totalUsers: number;
  userGrowth: number;
  activeProviders: number;
  providerGrowth: number;
  totalBookings: number;
  bookingGrowth: number;
  totalRevenue: number;
  revenueGrowth: number;
  pendingApprovals: number;
  recentBookings: Array<{
    id: string;
    customerName: string;
    serviceName: string;
    providerName: string;
    amount: number;
    status: string;
    date: string;
  }>;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  constructor(private apiService: ApiService) {}

  // User Management
  getUsers(params?: {
    role?: string;
    searchTerm?: string;
    isActive?: boolean;
    isSuspended?: boolean;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<{ users: UserListItem[]; totalCount: number }> {
    return this.apiService.get('admin/users', params);
  }

  getUserDetails(userId: string): Observable<UserDetail> {
    return this.apiService.get<UserDetail>(`admin/users/${userId}`);
  }

  suspendUser(userId: string, request: SuspendUserRequest): Observable<any> {
    return this.apiService.post(`admin/users/${userId}/suspend`, request);
  }

  unsuspendUser(userId: string): Observable<any> {
    return this.apiService.post(`admin/users/${userId}/unsuspend`, {});
  }

  deactivateUser(userId: string, reason: string): Observable<any> {
    return this.apiService.post(`admin/users/${userId}/deactivate`, { reason });
  }

  // Provider Verification
  getVerificationRequests(params?: {
    status?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<{ requests: ProviderVerificationRequest[]; totalCount: number }> {
    return this.apiService.get('admin/providers/verification-requests', params);
  }

  approveVerification(providerId: string, request: ApproveVerificationRequest): Observable<any> {
    return this.apiService.post(`admin/providers/${providerId}/verify`, request);
  }

  rejectVerification(providerId: string, request: RejectVerificationRequest): Observable<any> {
    return this.apiService.post(`admin/providers/${providerId}/reject`, request);
  }

  // Service Management
  getPendingServices(params?: {
    categoryId?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<{ services: ServiceApprovalItem[]; totalCount: number }> {
    return this.apiService.get('admin/services/pending', params);
  }

  approveService(serviceId: string, notes?: string): Observable<any> {
    return this.apiService.post(`admin/services/${serviceId}/approve`, { notes });
  }

  rejectService(serviceId: string, reason: string): Observable<any> {
    return this.apiService.post(`admin/services/${serviceId}/reject`, { reason });
  }

  deactivateService(serviceId: string, reason: string): Observable<any> {
    return this.apiService.post(`admin/services/${serviceId}/deactivate`, { reason });
  }

  // Support Tickets
  getTickets(params?: {
    status?: string;
    priority?: string;
    category?: string;
    searchTerm?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<{ tickets: AdminTicket[]; totalCount: number; statistics: TicketStatistics }> {
    return this.apiService.get('admin/tickets', params);
  }

  getTicketDetails(ticketId: string): Observable<any> {
    return this.apiService.get(`admin/tickets/${ticketId}`);
  }

  updateTicketStatus(ticketId: string, status: string, adminNotes?: string): Observable<any> {
    return this.apiService.put(`admin/tickets/${ticketId}/status`, { status, adminNotes });
  }

  assignTicket(ticketId: string, adminUserId: string): Observable<any> {
    return this.apiService.post(`admin/tickets/${ticketId}/assign`, { adminUserId });
  }

  // Content Management
  getContent(params?: {
    type?: string;
    isPublished?: boolean;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<{ content: ContentItem[]; totalCount: number }> {
    return this.apiService.get('admin/content', params);
  }

  getContentById(contentId: string): Observable<any> {
    return this.apiService.get(`admin/content/${contentId}`);
  }

  createContent(request: CreateContentRequest): Observable<any> {
    return this.apiService.post('admin/content', request);
  }

  updateContent(contentId: string, request: UpdateContentRequest): Observable<any> {
    return this.apiService.put(`admin/content/${contentId}`, request);
  }

  deleteContent(contentId: string): Observable<any> {
    return this.apiService.delete(`admin/content/${contentId}`);
  }

  publishContent(contentId: string): Observable<any> {
    return this.apiService.post(`admin/content/${contentId}/publish`, {});
  }

  unpublishContent(contentId: string): Observable<any> {
    return this.apiService.post(`admin/content/${contentId}/unpublish`, {});
  }

  // Reports & Moderation
  getReports(params?: {
    type?: string;
    status?: string;
    reason?: string;
    startDate?: string;
    endDate?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<{ reports: ReportListItem[]; totalCount: number }> {
    return this.apiService.get('admin/reports', params);
  }

  getReportDetails(reportId: string): Observable<ReportDetail> {
    return this.apiService.get<ReportDetail>(`admin/reports/${reportId}`);
  }

  reviewReport(reportId: string, request: ReviewReportRequest): Observable<any> {
    return this.apiService.put(`admin/reports/${reportId}/review`, request);
  }

  // Promo Codes
  getPromoCodes(params?: {
    isActive?: boolean;
    searchTerm?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<any> {
    return this.apiService.get('admin/promocodes', params);
  }

  createPromoCode(request: any): Observable<any> {
    return this.apiService.post('admin/promocodes', request);
  }

  updatePromoCodeStatus(promoCodeId: string, isActive: boolean): Observable<any> {
    return this.apiService.put(`admin/promocodes/${promoCodeId}/status`, { isActive });
  }

  // Analytics
  getSystemAnalytics(startDate?: string, endDate?: string): Observable<SystemAnalytics> {
    return this.apiService.get<SystemAnalytics>('admin/analytics', {
      startDate,
      endDate
    });
  }

  getDashboardStats(): Observable<DashboardStats> {
    return this.apiService.get<DashboardStats>('admin/dashboard/stats');
  }

  // Categories
  getCategories(): Observable<any[]> {
    return this.apiService.get('admin/categories');
  }

  createCategory(category: any): Observable<any> {
    return this.apiService.post('admin/categories', category);
  }

  updateCategory(categoryId: string, category: any): Observable<any> {
    return this.apiService.put(`admin/categories/${categoryId}`, category);
  }

  deleteCategory(categoryId: string): Observable<any> {
    return this.apiService.delete(`admin/categories/${categoryId}`);
  }
}
