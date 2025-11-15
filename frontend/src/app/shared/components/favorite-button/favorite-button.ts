import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-favorite-button',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    TranslateModule
  ],
  templateUrl: './favorite-button.html',
  styleUrl: './favorite-button.scss'
})
export class FavoriteButton {
  @Input() serviceId!: string;
  @Input() isFavorite = false;
  @Input() loading = false;

  @Output() favoriteToggle = new EventEmitter<boolean>();

  toggleFavorite(): void {
    this.favoriteToggle.emit(!this.isFavorite);
  }
}
