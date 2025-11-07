import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CreateReportRequest {
  type: string;
  reason: string;
  description: string;
  reportedUserId?: string;
  reportedBookingId?: string;
  reportedReviewId?: string;
  reportedServiceId?: string;
  evidence?: string[];
}

export interface Report {
  reportId: string;
  reportNumber: string;
  type: string;
  reason: string;
  status: string;
  description: string;
  createdAt: string;
  reviewedAt?: string;
  resolvedAt?: string;
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
  reportedBookingStatus?: string;
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

export enum ReportType {
  User = 'User',
  Booking = 'Booking',
  Review = 'Review',
  Service = 'Service'
}

export enum ReportReason {
  InappropriateContent = 'InappropriateContent',
  Harassment = 'Harassment',
  Spam = 'Spam',
  Fraud = 'Fraud',
  ViolenceThreat = 'ViolenceThreat',
  HateSpeech = 'HateSpeech',
  FakeProfile = 'FakeProfile',
  PoorServiceQuality = 'PoorServiceQuality',
  NoShow = 'NoShow',
  PaymentIssue = 'PaymentIssue',
  Other = 'Other'
}

export enum ReportStatus {
  Pending = 'Pending',
  UnderReview = 'UnderReview',
  Resolved = 'Resolved',
  Dismissed = 'Dismissed',
  ActionTaken = 'ActionTaken'
}

export enum ModerationAction {
  None = 'None',
  WarningIssued = 'WarningIssued',
  ContentRemoved = 'ContentRemoved',
  UserSuspended = 'UserSuspended',
  UserBanned = 'UserBanned',
  RefundIssued = 'RefundIssued',
  BookingCancelled = 'BookingCancelled',
  ReviewRemoved = 'ReviewRemoved',
  ServiceDeactivated = 'ServiceDeactivated'
}

@Injectable({
  providedIn: 'root'
})
export class ReportsService {
  constructor(private apiService: ApiService) {}

  // Create a report
  createReport(request: CreateReportRequest): Observable<any> {
    return this.apiService.post('reports', request);
  }

  // Report a user
  reportUser(userId: string, reason: string, description: string, evidence?: string[]): Observable<any> {
    return this.createReport({
      type: ReportType.User,
      reason,
      description,
      reportedUserId: userId,
      evidence
    });
  }

  // Report a booking
  reportBooking(bookingId: string, reason: string, description: string, evidence?: string[]): Observable<any> {
    return this.createReport({
      type: ReportType.Booking,
      reason,
      description,
      reportedBookingId: bookingId,
      evidence
    });
  }

  // Report a review
  reportReview(reviewId: string, reason: string, description: string, evidence?: string[]): Observable<any> {
    return this.createReport({
      type: ReportType.Review,
      reason,
      description,
      reportedReviewId: reviewId,
      evidence
    });
  }

  // Report a service
  reportService(serviceId: string, reason: string, description: string, evidence?: string[]): Observable<any> {
    return this.createReport({
      type: ReportType.Service,
      reason,
      description,
      reportedServiceId: serviceId,
      evidence
    });
  }

  // Get user's reports
  getMyReports(params?: {
    status?: string;
    type?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<{ reports: Report[]; totalCount: number }> {
    return this.apiService.get('reports/my-reports', params);
  }

  // Get report details
  getReportDetails(reportId: string): Observable<ReportDetail> {
    return this.apiService.get<ReportDetail>(`reports/${reportId}`);
  }

  // Upload evidence file
  uploadEvidence(file: File): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.apiService.post<{ url: string }>('reports/evidence/upload', formData);
  }

  // Get available report types
  getReportTypes(): { value: string; label: string }[] {
    return [
      { value: ReportType.User, label: 'User' },
      { value: ReportType.Booking, label: 'Booking' },
      { value: ReportType.Review, label: 'Review' },
      { value: ReportType.Service, label: 'Service' }
    ];
  }

  // Get available report reasons
  getReportReasons(): { value: string; label: string }[] {
    return [
      { value: ReportReason.InappropriateContent, label: 'Inappropriate Content' },
      { value: ReportReason.Harassment, label: 'Harassment' },
      { value: ReportReason.Spam, label: 'Spam' },
      { value: ReportReason.Fraud, label: 'Fraud' },
      { value: ReportReason.ViolenceThreat, label: 'Violence or Threat' },
      { value: ReportReason.HateSpeech, label: 'Hate Speech' },
      { value: ReportReason.FakeProfile, label: 'Fake Profile' },
      { value: ReportReason.PoorServiceQuality, label: 'Poor Service Quality' },
      { value: ReportReason.NoShow, label: 'No Show' },
      { value: ReportReason.PaymentIssue, label: 'Payment Issue' },
      { value: ReportReason.Other, label: 'Other' }
    ];
  }

  // Get available report statuses
  getReportStatuses(): { value: string; label: string }[] {
    return [
      { value: ReportStatus.Pending, label: 'Pending' },
      { value: ReportStatus.UnderReview, label: 'Under Review' },
      { value: ReportStatus.Resolved, label: 'Resolved' },
      { value: ReportStatus.Dismissed, label: 'Dismissed' },
      { value: ReportStatus.ActionTaken, label: 'Action Taken' }
    ];
  }

  // Get user-friendly status label
  getStatusLabel(status: string): string {
    const statusMap: Record<string, string> = {
      [ReportStatus.Pending]: 'Pending Review',
      [ReportStatus.UnderReview]: 'Under Review',
      [ReportStatus.Resolved]: 'Resolved',
      [ReportStatus.Dismissed]: 'Dismissed',
      [ReportStatus.ActionTaken]: 'Action Taken'
    };
    return statusMap[status] || status;
  }

  // Get status badge color
  getStatusBadgeClass(status: string): string {
    const statusColorMap: Record<string, string> = {
      [ReportStatus.Pending]: 'badge-warning',
      [ReportStatus.UnderReview]: 'badge-info',
      [ReportStatus.Resolved]: 'badge-success',
      [ReportStatus.Dismissed]: 'badge-secondary',
      [ReportStatus.ActionTaken]: 'badge-primary'
    };
    return statusColorMap[status] || 'badge-secondary';
  }

  // Get user-friendly reason label
  getReasonLabel(reason: string): string {
    const reasonMap: Record<string, string> = {
      [ReportReason.InappropriateContent]: 'Inappropriate Content',
      [ReportReason.Harassment]: 'Harassment',
      [ReportReason.Spam]: 'Spam',
      [ReportReason.Fraud]: 'Fraud',
      [ReportReason.ViolenceThreat]: 'Violence or Threat',
      [ReportReason.HateSpeech]: 'Hate Speech',
      [ReportReason.FakeProfile]: 'Fake Profile',
      [ReportReason.PoorServiceQuality]: 'Poor Service Quality',
      [ReportReason.NoShow]: 'No Show',
      [ReportReason.PaymentIssue]: 'Payment Issue',
      [ReportReason.Other]: 'Other'
    };
    return reasonMap[reason] || reason;
  }

  // Check if user can report (prevent duplicate reports within 24 hours)
  canReport(lastReportTimestamp?: string): boolean {
    if (!lastReportTimestamp) {
      return true;
    }

    const lastReport = new Date(lastReportTimestamp);
    const now = new Date();
    const hoursSinceLastReport = (now.getTime() - lastReport.getTime()) / (1000 * 60 * 60);

    return hoursSinceLastReport >= 24;
  }

  // Get recommended actions based on report type and reason
  getRecommendedActions(type: string, reason: string): string[] {
    const actions: string[] = [];

    if (reason === ReportReason.Fraud || reason === ReportReason.ViolenceThreat) {
      actions.push('Contact support immediately');
      actions.push('Preserve all evidence');
    }

    if (type === ReportType.Booking) {
      actions.push('Cancel booking if necessary');
      actions.push('Request refund if applicable');
    }

    if (type === ReportType.User) {
      actions.push('Block user from future interactions');
    }

    return actions;
  }
}
