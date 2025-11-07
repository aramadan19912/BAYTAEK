import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@angular/router';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="container">
      <h2>{{ 'nav.profile' | translate }}</h2>
      <p>Profile view - To be implemented</p>
    </div>
  `
})
export class ProfileComponent {}
