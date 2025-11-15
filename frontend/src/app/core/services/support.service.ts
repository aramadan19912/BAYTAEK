import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CreateTicketRequest {
  subject: string;
  category: string;
  priority: string;
  description: string;
  attachments?: string[];
}

export interface TicketListItem {
  ticketId: string;
  ticketNumber: string;
  subject: string;
  category: string;
  priority: string;
  status: string;
  lastMessageAt: string;
  createdAt: string;
  messagesCount: number;
  hasUnreadMessages: boolean;
}

export interface TicketDetail {
  ticketId: string;
  ticketNumber: string;
  subject: string;
  category: string;
  priority: string;
  status: string;
  description: string;
  userId: string;
  userName: string;
  userEmail: string;
  assignedToAdminId?: string;
  assignedToAdminName?: string;
  createdAt: string;
  updatedAt: string;
  closedAt?: string;
  messages: TicketMessage[];
  attachments?: string[];
}

export interface TicketMessage {
  messageId: string;
  ticketId: string;
  senderId: string;
  senderName: string;
  senderRole: string;
  message: string;
  attachments?: string[];
  isInternal: boolean;
  sentAt: string;
}

export interface AddTicketMessageRequest {
  message: string;
  attachments?: string[];
  isInternal?: boolean;
}

export interface UpdateTicketStatusRequest {
  status: string;
  adminNotes?: string;
}

export enum TicketStatus {
  Open = 'Open',
  InProgress = 'InProgress',
  WaitingForCustomer = 'WaitingForCustomer',
  Resolved = 'Resolved',
  Closed = 'Closed'
}

export enum TicketPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Urgent = 'Urgent'
}

export enum TicketCategory {
  TechnicalIssue = 'TechnicalIssue',
  PaymentIssue = 'PaymentIssue',
  BookingIssue = 'BookingIssue',
  AccountIssue = 'AccountIssue',
  ServiceQuality = 'ServiceQuality',
  RefundRequest = 'RefundRequest',
  GeneralInquiry = 'GeneralInquiry',
  FeatureRequest = 'FeatureRequest',
  BugReport = 'BugReport',
  Other = 'Other'
}

@Injectable({
  providedIn: 'root'
})
export class SupportService {
  constructor(private apiService: ApiService) {}

  // Create a new support ticket
  createTicket(request: CreateTicketRequest): Observable<any> {
    return this.apiService.post('support/tickets', request);
  }

  // Get user's tickets
  getMyTickets(params?: {
    status?: string;
    category?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<{ tickets: TicketListItem[]; totalCount: number }> {
    return this.apiService.get('support/tickets', params);
  }

  // Get ticket details
  getTicketDetails(ticketId: string): Observable<TicketDetail> {
    return this.apiService.get<TicketDetail>(`support/tickets/${ticketId}`);
  }

  // Add a message to a ticket
  addMessage(ticketId: string, request: AddTicketMessageRequest): Observable<any> {
    return this.apiService.post(`support/tickets/${ticketId}/messages`, request);
  }

  // Update ticket status (user can close/reopen their own tickets)
  updateStatus(ticketId: string, status: string): Observable<any> {
    return this.apiService.put(`support/tickets/${ticketId}/status`, { status });
  }

  // Close a ticket
  closeTicket(ticketId: string): Observable<any> {
    return this.updateStatus(ticketId, TicketStatus.Closed);
  }

  // Reopen a closed ticket
  reopenTicket(ticketId: string): Observable<any> {
    return this.updateStatus(ticketId, TicketStatus.Open);
  }

  // Get available categories
  getCategories(): { value: string; label: string }[] {
    return [
      { value: TicketCategory.TechnicalIssue, label: 'Technical Issue' },
      { value: TicketCategory.PaymentIssue, label: 'Payment Issue' },
      { value: TicketCategory.BookingIssue, label: 'Booking Issue' },
      { value: TicketCategory.AccountIssue, label: 'Account Issue' },
      { value: TicketCategory.ServiceQuality, label: 'Service Quality' },
      { value: TicketCategory.RefundRequest, label: 'Refund Request' },
      { value: TicketCategory.GeneralInquiry, label: 'General Inquiry' },
      { value: TicketCategory.FeatureRequest, label: 'Feature Request' },
      { value: TicketCategory.BugReport, label: 'Bug Report' },
      { value: TicketCategory.Other, label: 'Other' }
    ];
  }

  // Get available priorities
  getPriorities(): { value: string; label: string }[] {
    return [
      { value: TicketPriority.Low, label: 'Low' },
      { value: TicketPriority.Medium, label: 'Medium' },
      { value: TicketPriority.High, label: 'High' },
      { value: TicketPriority.Urgent, label: 'Urgent' }
    ];
  }

  // Get available statuses
  getStatuses(): { value: string; label: string }[] {
    return [
      { value: TicketStatus.Open, label: 'Open' },
      { value: TicketStatus.InProgress, label: 'In Progress' },
      { value: TicketStatus.WaitingForCustomer, label: 'Waiting for Customer' },
      { value: TicketStatus.Resolved, label: 'Resolved' },
      { value: TicketStatus.Closed, label: 'Closed' }
    ];
  }
}
