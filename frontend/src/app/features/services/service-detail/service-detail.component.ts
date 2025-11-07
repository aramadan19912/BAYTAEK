import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-service-detail',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="container">
      <h2>{{ 'services.details' | translate }}</h2>
      <p>Service detail view - To be implemented</p>
    </div>
  `
})
export class ServiceDetailComponent {}
