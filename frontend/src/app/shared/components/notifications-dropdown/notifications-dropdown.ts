import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject, takeUntil } from 'rxjs';
import { NotificationService, Notification, NotificationType } from '../../../core/services/notification.service';

@Component({
  selector: 'app-notifications-dropdown',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatBadgeModule,
    MatMenuModule,
    MatDividerModule,
    MatTooltipModule,
    TranslateModule
  ],
  templateUrl: './notifications-dropdown.html',
  styleUrl: './notifications-dropdown.scss'
})
export class NotificationsDropdown implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  notifications: Notification[] = [];
  unreadCount = 0;
  loading = false;
  maxDisplayedNotifications = 5;

  constructor(
    private notificationService: NotificationService,
    private router: Router,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    // Subscribe to notifications
    this.notificationService.notifications$
      .pipe(takeUntil(this.destroy$))
      .subscribe(notifications => {
        this.notifications = notifications.slice(0, this.maxDisplayedNotifications);
      });

    // Subscribe to unread count
    this.notificationService.unreadCount$
      .pipe(takeUntil(this.destroy$))
      .subscribe(count => {
        this.unreadCount = count;
      });

    // Initial load
    this.notificationService.loadNotifications();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onNotificationClick(notification: Notification): void {
    // Mark as read if unread
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.notificationId).subscribe();
    }

    // Navigate to action URL if provided
    if (notification.actionUrl) {
      this.router.navigateByUrl(notification.actionUrl);
    } else {
      // Default navigation based on type
      this.navigateByType(notification);
    }
  }

  private navigateByType(notification: Notification): void {
    const data = notification.data || {};

    switch (notification.type) {
      case NotificationType.BookingConfirmed:
      case NotificationType.BookingAccepted:
      case NotificationType.BookingRejected:
      case NotificationType.BookingCancelled:
      case NotificationType.BookingStarted:
      case NotificationType.BookingCompleted:
        if (data.bookingId) {
          this.router.navigate(['/bookings', data.bookingId]);
        }
        break;

      case NotificationType.PaymentReceived:
      case NotificationType.PaymentFailed:
      case NotificationType.RefundProcessed:
        if (data.bookingId) {
          this.router.navigate(['/bookings', data.bookingId]);
        } else {
          this.router.navigate(['/payments']);
        }
        break;

      case NotificationType.ReviewReceived:
        if (data.reviewId) {
          this.router.navigate(['/reviews', data.reviewId]);
        } else {
          this.router.navigate(['/reviews']);
        }
        break;

      case NotificationType.MessageReceived:
        if (data.conversationId) {
          this.router.navigate(['/messages', data.conversationId]);
        } else {
          this.router.navigate(['/messages']);
        }
        break;

      case NotificationType.PromoCodeAvailable:
        this.router.navigate(['/services']);
        break;

      case NotificationType.TicketUpdated:
        if (data.ticketId) {
          this.router.navigate(['/support', data.ticketId]);
        } else {
          this.router.navigate(['/support']);
        }
        break;

      default:
        this.router.navigate(['/notifications']);
        break;
    }
  }

  markAllAsRead(): void {
    this.loading = true;
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  viewAllNotifications(): void {
    this.router.navigate(['/notifications']);
  }

  getNotificationIcon(type: string): string {
    return this.notificationService.getNotificationIcon(type);
  }

  getNotificationColor(type: string): string {
    return this.notificationService.getNotificationColor(type);
  }

  getNotificationTime(timestamp: string): string {
    return this.notificationService.formatNotificationTime(timestamp);
  }

  getNotificationTitle(notification: Notification): string {
    const currentLang = this.translate.currentLang || 'en';
    return currentLang === 'ar' && notification.titleAr
      ? notification.titleAr
      : notification.title;
  }

  getNotificationMessage(notification: Notification): string {
    const currentLang = this.translate.currentLang || 'en';
    return currentLang === 'ar' && notification.messageAr
      ? notification.messageAr
      : notification.message;
  }
}
