import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, interval } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from './api.service';

export interface Notification {
  notificationId: string;
  userId: string;
  type: string;
  title: string;
  titleAr: string;
  message: string;
  messageAr: string;
  isRead: boolean;
  data?: Record<string, any>;
  actionUrl?: string;
  createdAt: string;
  readAt?: string;
}

export interface NotificationSettings {
  emailNotifications: boolean;
  smsNotifications: boolean;
  pushNotifications: boolean;
  bookingUpdates: boolean;
  promotionalEmails: boolean;
  newsletterSubscription: boolean;
  reminderNotifications: boolean;
  reviewRequests: boolean;
  messageNotifications: boolean;
}

export enum NotificationType {
  BookingConfirmed = 'BookingConfirmed',
  BookingAccepted = 'BookingAccepted',
  BookingRejected = 'BookingRejected',
  BookingCancelled = 'BookingCancelled',
  BookingStarted = 'BookingStarted',
  BookingCompleted = 'BookingCompleted',
  PaymentReceived = 'PaymentReceived',
  PaymentFailed = 'PaymentFailed',
  RefundProcessed = 'RefundProcessed',
  ReviewReceived = 'ReviewReceived',
  MessageReceived = 'MessageReceived',
  PromoCodeAvailable = 'PromoCodeAvailable',
  AccountVerified = 'AccountVerified',
  AccountSuspended = 'AccountSuspended',
  ProviderVerificationApproved = 'ProviderVerificationApproved',
  ProviderVerificationRejected = 'ProviderVerificationRejected',
  ServiceApproved = 'ServiceApproved',
  ServiceRejected = 'ServiceRejected',
  TicketUpdated = 'TicketUpdated',
  ReportReviewed = 'ReportReviewed',
  SystemAnnouncement = 'SystemAnnouncement'
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();

  private unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$ = this.unreadCountSubject.asObservable();

  private fcmToken: string | null = null;

  constructor(private apiService: ApiService) {
    // Poll for new notifications every 30 seconds
    interval(30000).subscribe(() => {
      this.refreshNotifications();
    });

    this.loadNotifications();
  }

  // Load notifications
  loadNotifications(): void {
    this.getNotifications().subscribe({
      next: (result) => {
        this.notificationsSubject.next(result.notifications);
        this.unreadCountSubject.next(result.unreadCount);
      },
      error: (error) => {
        console.error('Error loading notifications:', error);
      }
    });
  }

  // Refresh notifications
  refreshNotifications(): void {
    this.loadNotifications();
  }

  // Get notifications
  getNotifications(params?: {
    type?: string;
    isRead?: boolean;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<{ notifications: Notification[]; totalCount: number; unreadCount: number }> {
    return this.apiService.get('notifications', params);
  }

  // Get notification by ID
  getNotificationById(notificationId: string): Observable<Notification> {
    return this.apiService.get<Notification>(`notifications/${notificationId}`);
  }

  // Mark notification as read
  markAsRead(notificationId: string): Observable<any> {
    return this.apiService.put(`notifications/${notificationId}/read`, {}).pipe(
      tap(() => {
        this.loadNotifications();
      })
    );
  }

  // Mark all notifications as read
  markAllAsRead(): Observable<any> {
    return this.apiService.put('notifications/mark-all-read', {}).pipe(
      tap(() => {
        this.loadNotifications();
      })
    );
  }

  // Delete notification
  deleteNotification(notificationId: string): Observable<any> {
    return this.apiService.delete(`notifications/${notificationId}`).pipe(
      tap(() => {
        this.loadNotifications();
      })
    );
  }

  // Delete all notifications
  deleteAllNotifications(): Observable<any> {
    return this.apiService.delete('notifications/all').pipe(
      tap(() => {
        this.notificationsSubject.next([]);
        this.unreadCountSubject.next(0);
      })
    );
  }

  // Get unread count
  getUnreadCount(): number {
    return this.unreadCountSubject.value;
  }

  // Get current notifications
  getCurrentNotifications(): Notification[] {
    return this.notificationsSubject.value;
  }

  // Get unread notifications
  getUnreadNotifications(): Notification[] {
    return this.notificationsSubject.value.filter(n => !n.isRead);
  }

  // Get notification settings
  getNotificationSettings(): Observable<NotificationSettings> {
    return this.apiService.get<NotificationSettings>('notifications/settings');
  }

  // Update notification settings
  updateNotificationSettings(settings: Partial<NotificationSettings>): Observable<any> {
    return this.apiService.put('notifications/settings', settings);
  }

  // Register FCM token for push notifications
  registerFCMToken(token: string): Observable<any> {
    this.fcmToken = token;
    return this.apiService.post('notifications/register-token', {
      token,
      deviceType: this.getDeviceType(),
      deviceId: this.getDeviceId()
    });
  }

  // Unregister FCM token
  unregisterFCMToken(): Observable<any> {
    if (!this.fcmToken) {
      return new Observable(observer => {
        observer.next({ success: true });
        observer.complete();
      });
    }

    return this.apiService.post('notifications/unregister-token', {
      token: this.fcmToken
    }).pipe(
      tap(() => {
        this.fcmToken = null;
      })
    );
  }

  // Request notification permission
  async requestNotificationPermission(): Promise<boolean> {
    if (!('Notification' in window)) {
      console.warn('This browser does not support notifications');
      return false;
    }

    if (Notification.permission === 'granted') {
      return true;
    }

    if (Notification.permission !== 'denied') {
      const permission = await Notification.requestPermission();
      return permission === 'granted';
    }

    return false;
  }

  // Show browser notification
  showBrowserNotification(title: string, options?: NotificationOptions): void {
    if (Notification.permission === 'granted') {
      new Notification(title, options);
    }
  }

  // Get notification icon
  getNotificationIcon(type: string): string {
    const iconMap: Record<string, string> = {
      [NotificationType.BookingConfirmed]: 'check_circle',
      [NotificationType.BookingAccepted]: 'done',
      [NotificationType.BookingRejected]: 'cancel',
      [NotificationType.BookingCancelled]: 'event_busy',
      [NotificationType.BookingStarted]: 'play_arrow',
      [NotificationType.BookingCompleted]: 'done_all',
      [NotificationType.PaymentReceived]: 'payment',
      [NotificationType.PaymentFailed]: 'error',
      [NotificationType.RefundProcessed]: 'attach_money',
      [NotificationType.ReviewReceived]: 'star',
      [NotificationType.MessageReceived]: 'message',
      [NotificationType.PromoCodeAvailable]: 'local_offer',
      [NotificationType.AccountVerified]: 'verified_user',
      [NotificationType.AccountSuspended]: 'block',
      [NotificationType.SystemAnnouncement]: 'campaign'
    };
    return iconMap[type] || 'notifications';
  }

  // Get notification color
  getNotificationColor(type: string): string {
    const colorMap: Record<string, string> = {
      [NotificationType.BookingConfirmed]: 'success',
      [NotificationType.BookingAccepted]: 'success',
      [NotificationType.BookingRejected]: 'danger',
      [NotificationType.BookingCancelled]: 'warning',
      [NotificationType.PaymentReceived]: 'success',
      [NotificationType.PaymentFailed]: 'danger',
      [NotificationType.RefundProcessed]: 'info',
      [NotificationType.ReviewReceived]: 'primary',
      [NotificationType.MessageReceived]: 'info',
      [NotificationType.PromoCodeAvailable]: 'success',
      [NotificationType.AccountSuspended]: 'danger',
      [NotificationType.SystemAnnouncement]: 'primary'
    };
    return colorMap[type] || 'secondary';
  }

  // Format notification time
  formatNotificationTime(timestamp: string): string {
    const date = new Date(timestamp);
    const now = new Date();
    const diffInSeconds = (now.getTime() - date.getTime()) / 1000;

    if (diffInSeconds < 60) {
      return 'Just now';
    } else if (diffInSeconds < 3600) {
      const minutes = Math.floor(diffInSeconds / 60);
      return `${minutes}m ago`;
    } else if (diffInSeconds < 86400) {
      const hours = Math.floor(diffInSeconds / 3600);
      return `${hours}h ago`;
    } else if (diffInSeconds < 604800) {
      const days = Math.floor(diffInSeconds / 86400);
      return `${days}d ago`;
    } else {
      return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    }
  }

  // Group notifications by date
  groupNotificationsByDate(notifications: Notification[]): { date: string; notifications: Notification[] }[] {
    const groups: { [key: string]: Notification[] } = {};

    notifications.forEach(notification => {
      const date = this.getNotificationDateGroup(notification.createdAt);

      if (!groups[date]) {
        groups[date] = [];
      }
      groups[date].push(notification);
    });

    return Object.keys(groups).map(date => ({
      date,
      notifications: groups[date]
    }));
  }

  private getNotificationDateGroup(timestamp: string): string {
    const date = new Date(timestamp);
    const now = new Date();
    const diffInDays = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24));

    if (diffInDays === 0) {
      return 'Today';
    } else if (diffInDays === 1) {
      return 'Yesterday';
    } else if (diffInDays < 7) {
      return 'This Week';
    } else if (diffInDays < 30) {
      return 'This Month';
    } else {
      return 'Older';
    }
  }

  // Get device type
  private getDeviceType(): string {
    const userAgent = navigator.userAgent.toLowerCase();
    if (userAgent.includes('mobile')) {
      return 'mobile';
    } else if (userAgent.includes('tablet')) {
      return 'tablet';
    } else {
      return 'web';
    }
  }

  // Get device ID (simple implementation, use a proper library in production)
  private getDeviceId(): string {
    let deviceId = localStorage.getItem('deviceId');
    if (!deviceId) {
      deviceId = this.generateUUID();
      localStorage.setItem('deviceId', deviceId);
    }
    return deviceId;
  }

  private generateUUID(): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
      const r = (Math.random() * 16) | 0;
      const v = c === 'x' ? r : (r & 0x3) | 0x8;
      return v.toString(16);
    });
  }
}
