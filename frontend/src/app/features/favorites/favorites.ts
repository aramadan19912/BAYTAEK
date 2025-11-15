import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import { FavoritesService } from '../../core/services/favorites.service';
import { NotificationService } from '../../core/services/notification.service';
import { FavoriteButton } from '../../shared/components/favorite-button/favorite-button';

@Component({
  selector: 'app-favorites',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    TranslateModule,
    FavoriteButton
  ],
  templateUrl: './favorites.html',
  styleUrl: './favorites.scss'
})
export class Favorites implements OnInit {
  favorites: any[] = [];
  loading = false;

  constructor(
    private favoritesService: FavoritesService,
    private notificationService: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadFavorites();
  }

  loadFavorites(): void {
    this.loading = true;
    this.favoritesService.getFavorites().subscribe({
      next: (response) => {
        this.favorites = response.data || [];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.notificationService.showError('Failed to load favorites');
      }
    });
  }

  onRemoveFavorite(serviceId: string): void {
    this.favoritesService.removeFavorite(serviceId).subscribe({
      next: () => {
        this.favorites = this.favorites.filter(f => f.serviceId !== serviceId);
        this.notificationService.showSuccess('favorites.removedFromFavorites');
      },
      error: () => {
        this.notificationService.showError('Failed to remove from favorites');
      }
    });
  }

  viewService(serviceId: string): void {
    this.router.navigate(['/services', serviceId]);
  }
}
