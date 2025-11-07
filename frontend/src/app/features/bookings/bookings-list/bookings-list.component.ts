import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { Booking, BookingStatus } from '../../../shared/models/booking.model';

@Component({
  selector: 'app-bookings-list',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  template: `
    <div class="bookings-container">
      <h1>{{ 'bookings.title' | translate }}</h1>

      <div class="status-filters">
        <button
          *ngFor="let status of statuses"
          (click)="filterByStatus(status)"
          [class.active]="selectedStatus === status"
          class="status-btn"
        >
          {{ 'bookings.status.' + status.toLowerCase() | translate }}
        </button>
      </div>

      <div class="bookings-list" *ngIf="!loading">
        <div *ngFor="let booking of bookings" class="booking-card">
          <div class="booking-header">
            <h3>{{ booking.serviceName }}</h3>
            <span class="status-badge" [class]="'status-' + booking.status.toLowerCase()">
              {{ 'bookings.status.' + booking.status.toLowerCase() | translate }}
            </span>
          </div>

          <div class="booking-details">
            <div class="detail-row">
              <span class="label">{{ 'bookings.date' | translate }}:</span>
              <span>{{ booking.scheduledAt | date:'medium' }}</span>
            </div>
            <div class="detail-row">
              <span class="label">{{ 'bookings.amount' | translate }}:</span>
              <span class="amount">{{ booking.totalAmount }} {{ booking.currency }}</span>
            </div>
            <div class="detail-row">
              <span class="label">{{ 'bookings.address' | translate }}:</span>
              <span>{{ booking.address.street }}, {{ booking.address.city }}</span>
            </div>
          </div>

          <div class="booking-actions">
            <button [routerLink]="['/bookings', booking.id]" class="btn btn-view">
              {{ 'bookings.view_details' | translate }}
            </button>
            <button
              *ngIf="booking.status === 'Pending' || booking.status === 'Confirmed'"
              (click)="cancelBooking(booking.id)"
              class="btn btn-cancel"
            >
              {{ 'bookings.cancel' | translate }}
            </button>
            <button
              *ngIf="booking.status === 'Completed'"
              [routerLink]="['/bookings', booking.id, 'review']"
              class="btn btn-review"
            >
              {{ 'bookings.rate' | translate }}
            </button>
          </div>
        </div>
      </div>

      <app-loading-spinner *ngIf="loading"></app-loading-spinner>

      <div *ngIf="!loading && bookings.length === 0" class="no-bookings">
        <p>{{ 'bookings.no_bookings' | translate }}</p>
        <button routerLink="/services" class="btn btn-primary">
          {{ 'bookings.browse_services' | translate }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .bookings-container {
      padding: 2rem;
      max-width: 1000px;
      margin: 0 auto;
    }

    h1 {
      color: #333;
      margin-bottom: 2rem;
    }

    .status-filters {
      display: flex;
      gap: 1rem;
      margin-bottom: 2rem;
      flex-wrap: wrap;
    }

    .status-btn {
      padding: 0.5rem 1rem;
      border: 1px solid #ddd;
      background: white;
      border-radius: 20px;
      cursor: pointer;
      transition: all 0.3s;
    }

    .status-btn:hover,
    .status-btn.active {
      background: #007bff;
      color: white;
      border-color: #007bff;
    }

    .bookings-list {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .booking-card {
      background: white;
      border-radius: 8px;
      padding: 1.5rem;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .booking-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
      padding-bottom: 1rem;
      border-bottom: 1px solid #eee;
    }

    .booking-header h3 {
      margin: 0;
      color: #333;
    }

    .status-badge {
      padding: 0.25rem 0.75rem;
      border-radius: 12px;
      font-size: 0.875rem;
      font-weight: 500;
    }

    .status-pending { background: #fff3cd; color: #856404; }
    .status-confirmed { background: #d1ecf1; color: #0c5460; }
    .status-inprogress { background: #d4edda; color: #155724; }
    .status-completed { background: #d4edda; color: #155724; }
    .status-cancelled { background: #f8d7da; color: #721c24; }
    .status-disputed { background: #f8d7da; color: #721c24; }

    .booking-details {
      margin-bottom: 1rem;
    }

    .detail-row {
      display: flex;
      justify-content: space-between;
      padding: 0.5rem 0;
    }

    .label {
      color: #666;
      font-weight: 500;
    }

    .amount {
      color: #007bff;
      font-weight: bold;
    }

    .booking-actions {
      display: flex;
      gap: 1rem;
      flex-wrap: wrap;
    }

    .btn {
      padding: 0.5rem 1rem;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 0.9rem;
      transition: all 0.3s;
    }

    .btn-view {
      background: #007bff;
      color: white;
    }

    .btn-cancel {
      background: #dc3545;
      color: white;
    }

    .btn-review {
      background: #28a745;
      color: white;
    }

    .btn:hover {
      opacity: 0.9;
      transform: translateY(-2px);
    }

    .no-bookings {
      text-align: center;
      padding: 3rem;
      color: #666;
    }

    .no-bookings .btn-primary {
      margin-top: 1rem;
      background: #007bff;
      color: white;
    }
  `]
})
export class BookingsListComponent implements OnInit {
  bookings: Booking[] = [];
  loading = false;
  selectedStatus: string | null = null;
  statuses = ['Pending', 'Confirmed', 'InProgress', 'Completed', 'Cancelled'];

  constructor(
    private apiService: ApiService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadBookings();
  }

  loadBookings(): void {
    const user = this.authService.getCurrentUser();
    if (!user) return;

    this.loading = true;
    const params: any = { userId: user.id };
    if (this.selectedStatus) {
      params.status = this.selectedStatus;
    }

    this.apiService.get<any>('bookings', params).subscribe({
      next: (response) => {
        this.bookings = response.data?.items || [];
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading bookings', error);
        this.loading = false;
      }
    });
  }

  filterByStatus(status: string): void {
    this.selectedStatus = this.selectedStatus === status ? null : status;
    this.loadBookings();
  }

  cancelBooking(bookingId: string): void {
    if (confirm('Are you sure you want to cancel this booking?')) {
      this.apiService.post(`bookings/${bookingId}/cancel`, {
        reason: 'Customer cancellation'
      }).subscribe({
        next: () => {
          this.loadBookings();
        },
        error: (error) => {
          console.error('Error cancelling booking', error);
        }
      });
    }
  }
}
