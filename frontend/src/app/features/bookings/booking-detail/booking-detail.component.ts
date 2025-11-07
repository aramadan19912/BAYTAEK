import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-booking-detail',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="container">
      <h2>{{ 'bookings.details' | translate }}</h2>
      <p>Booking detail view - To be implemented</p>
    </div>
  `
})
export class BookingDetailComponent {}
