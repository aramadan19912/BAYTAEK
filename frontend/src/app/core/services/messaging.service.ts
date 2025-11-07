import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, interval } from 'rxjs';
import { ApiService } from './api.service';

export interface SendMessageRequest {
  receiverId: string;
  bookingId?: string;
  message: string;
  attachments?: string[];
}

export interface Message {
  messageId: string;
  conversationId: string;
  senderId: string;
  receiverId: string;
  bookingId?: string;
  message: string;
  attachments?: string[];
  isRead: boolean;
  sentAt: string;
  readAt?: string;
}

export interface Conversation {
  conversationId: string;
  otherUserId: string;
  otherUserName: string;
  otherUserRole: string;
  otherUserAvatar?: string;
  lastMessage: string;
  lastMessageAt: string;
  lastMessageSenderId: string;
  unreadCount: number;
  bookingId?: string;
  bookingNumber?: string;
  isOnline?: boolean;
}

export interface ConversationDetail {
  conversationId: string;
  participants: ConversationParticipant[];
  messages: Message[];
  bookingId?: string;
  bookingNumber?: string;
  totalMessages: number;
  hasMore: boolean;
}

export interface ConversationParticipant {
  userId: string;
  name: string;
  role: string;
  avatar?: string;
  isOnline: boolean;
  lastSeenAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class MessagingService {
  private unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$ = this.unreadCountSubject.asObservable();

  private activeConversationSubject = new BehaviorSubject<string | null>(null);
  public activeConversation$ = this.activeConversationSubject.asObservable();

  constructor(private apiService: ApiService) {
    // Poll for unread count every 30 seconds
    interval(30000).subscribe(() => {
      this.refreshUnreadCount();
    });
  }

  // Send a message
  sendMessage(request: SendMessageRequest): Observable<any> {
    return this.apiService.post('messages', request);
  }

  // Get user conversations
  getConversations(params?: {
    pageNumber?: number;
    pageSize?: number;
  }): Observable<{ conversations: Conversation[]; totalCount: number }> {
    return this.apiService.get('messages/conversations', params);
  }

  // Get conversation details with messages
  getConversation(conversationId: string, params?: {
    pageNumber?: number;
    pageSize?: number;
  }): Observable<ConversationDetail> {
    return this.apiService.get<ConversationDetail>(`messages/conversations/${conversationId}`, params);
  }

  // Get messages for a specific conversation
  getMessages(conversationId: string, params?: {
    pageNumber?: number;
    pageSize?: number;
    beforeMessageId?: string;
  }): Observable<{ messages: Message[]; totalCount: number; hasMore: boolean }> {
    return this.apiService.get(`messages/conversations/${conversationId}/messages`, params);
  }

  // Mark messages as read
  markAsRead(conversationId: string): Observable<any> {
    return this.apiService.post(`messages/conversations/${conversationId}/read`, {});
  }

  // Mark a specific message as read
  markMessageAsRead(messageId: string): Observable<any> {
    return this.apiService.post(`messages/${messageId}/read`, {});
  }

  // Get unread message count
  getUnreadCount(): Observable<{ unreadCount: number }> {
    return this.apiService.get<{ unreadCount: number }>('messages/unread-count');
  }

  // Refresh unread count and update subject
  refreshUnreadCount(): void {
    this.getUnreadCount().subscribe({
      next: (result) => {
        this.unreadCountSubject.next(result.unreadCount);
      },
      error: (error) => {
        console.error('Error fetching unread count:', error);
      }
    });
  }

  // Start a conversation with a user
  startConversation(receiverId: string, bookingId?: string): Observable<any> {
    return this.apiService.post('messages/conversations/start', {
      receiverId,
      bookingId
    });
  }

  // Delete a conversation
  deleteConversation(conversationId: string): Observable<any> {
    return this.apiService.delete(`messages/conversations/${conversationId}`);
  }

  // Get conversation by booking ID
  getConversationByBooking(bookingId: string): Observable<ConversationDetail> {
    return this.apiService.get<ConversationDetail>(`messages/conversations/booking/${bookingId}`);
  }

  // Search conversations
  searchConversations(searchTerm: string): Observable<{ conversations: Conversation[] }> {
    return this.apiService.get('messages/conversations/search', { searchTerm });
  }

  // Set active conversation
  setActiveConversation(conversationId: string | null): void {
    this.activeConversationSubject.next(conversationId);
    if (conversationId) {
      this.markAsRead(conversationId).subscribe();
    }
  }

  // Get active conversation ID
  getActiveConversation(): string | null {
    return this.activeConversationSubject.value;
  }

  // Upload attachment
  uploadAttachment(file: File): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.apiService.post<{ url: string }>('messages/attachments/upload', formData);
  }

  // Format timestamp for display
  formatMessageTime(timestamp: string): string {
    const date = new Date(timestamp);
    const now = new Date();
    const diffInHours = (now.getTime() - date.getTime()) / (1000 * 60 * 60);

    if (diffInHours < 24) {
      // Show time if today
      return date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
    } else if (diffInHours < 48) {
      // Show "Yesterday" if yesterday
      return 'Yesterday';
    } else if (diffInHours < 168) {
      // Show day of week if within last week
      return date.toLocaleDateString('en-US', { weekday: 'short' });
    } else {
      // Show date if older
      return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    }
  }

  // Check if message is from current user
  isMyMessage(message: Message, currentUserId: string): boolean {
    return message.senderId === currentUserId;
  }

  // Group messages by date
  groupMessagesByDate(messages: Message[]): { date: string; messages: Message[] }[] {
    const groups: { [key: string]: Message[] } = {};

    messages.forEach(message => {
      const date = new Date(message.sentAt).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      });

      if (!groups[date]) {
        groups[date] = [];
      }
      groups[date].push(message);
    });

    return Object.keys(groups).map(date => ({
      date,
      messages: groups[date]
    }));
  }

  // Get conversation name (other user's name)
  getConversationName(conversation: Conversation): string {
    return conversation.otherUserName;
  }

  // Get conversation avatar (other user's avatar)
  getConversationAvatar(conversation: Conversation): string | undefined {
    return conversation.otherUserAvatar;
  }
}
