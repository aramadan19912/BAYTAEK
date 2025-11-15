import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from './api.service';

export interface FavoriteService {
  favoriteId: string;
  serviceId: string;
  serviceName: string;
  serviceNameAr: string;
  categoryName: string;
  categoryNameAr: string;
  providerName: string;
  providerId: string;
  basePrice: number;
  averageRating: number;
  totalReviews: number;
  images?: string[];
  isActive: boolean;
  addedAt: string;
}

export interface FavoriteProvider {
  favoriteId: string;
  providerId: string;
  providerName: string;
  businessName?: string;
  averageRating: number;
  totalReviews: number;
  totalServices: number;
  isVerified: boolean;
  yearsOfExperience: number;
  profilePicture?: string;
  addedAt: string;
}

export interface AddToFavoritesRequest {
  entityType: 'Service' | 'Provider';
  entityId: string;
}

@Injectable({
  providedIn: 'root'
})
export class FavoritesService {
  private favoriteServicesSubject = new BehaviorSubject<FavoriteService[]>([]);
  public favoriteServices$ = this.favoriteServicesSubject.asObservable();

  private favoriteProvidersSubject = new BehaviorSubject<FavoriteProvider[]>([]);
  public favoriteProviders$ = this.favoriteProvidersSubject.asObservable();

  private favoriteServiceIdsSubject = new BehaviorSubject<Set<string>>(new Set());
  public favoriteServiceIds$ = this.favoriteServiceIdsSubject.asObservable();

  private favoriteProviderIdsSubject = new BehaviorSubject<Set<string>>(new Set());
  public favoriteProviderIds$ = this.favoriteProviderIdsSubject.asObservable();

  constructor(private apiService: ApiService) {
    this.loadFavorites();
  }

  // Load all favorites
  loadFavorites(): void {
    this.getFavoriteServices().subscribe({
      next: (result) => {
        this.favoriteServicesSubject.next(result.favorites);
        const serviceIds = new Set(result.favorites.map(f => f.serviceId));
        this.favoriteServiceIdsSubject.next(serviceIds);
      },
      error: (error) => {
        console.error('Error loading favorite services:', error);
      }
    });

    this.getFavoriteProviders().subscribe({
      next: (result) => {
        this.favoriteProvidersSubject.next(result.favorites);
        const providerIds = new Set(result.favorites.map(f => f.providerId));
        this.favoriteProviderIdsSubject.next(providerIds);
      },
      error: (error) => {
        console.error('Error loading favorite providers:', error);
      }
    });
  }

  // Get favorite services
  getFavoriteServices(params?: {
    categoryId?: string;
    searchTerm?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<{ favorites: FavoriteService[]; totalCount: number }> {
    return this.apiService.get('favorites/services', params);
  }

  // Get favorite providers
  getFavoriteProviders(params?: {
    searchTerm?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<{ favorites: FavoriteProvider[]; totalCount: number }> {
    return this.apiService.get('favorites/providers', params);
  }

  // Add service to favorites
  addServiceToFavorites(serviceId: string): Observable<any> {
    return this.apiService.post('favorites', {
      entityType: 'Service',
      entityId: serviceId
    }).pipe(
      tap(() => {
        this.loadFavorites();
      })
    );
  }

  // Add provider to favorites
  addProviderToFavorites(providerId: string): Observable<any> {
    return this.apiService.post('favorites', {
      entityType: 'Provider',
      entityId: providerId
    }).pipe(
      tap(() => {
        this.loadFavorites();
      })
    );
  }

  // Remove service from favorites
  removeServiceFromFavorites(serviceId: string): Observable<any> {
    return this.apiService.delete(`favorites/services/${serviceId}`).pipe(
      tap(() => {
        this.loadFavorites();
      })
    );
  }

  // Remove provider from favorites
  removeProviderFromFavorites(providerId: string): Observable<any> {
    return this.apiService.delete(`favorites/providers/${providerId}`).pipe(
      tap(() => {
        this.loadFavorites();
      })
    );
  }

  // Toggle service favorite
  toggleServiceFavorite(serviceId: string): Observable<any> {
    if (this.isServiceFavorite(serviceId)) {
      return this.removeServiceFromFavorites(serviceId);
    } else {
      return this.addServiceToFavorites(serviceId);
    }
  }

  // Toggle provider favorite
  toggleProviderFavorite(providerId: string): Observable<any> {
    if (this.isProviderFavorite(providerId)) {
      return this.removeProviderFromFavorites(providerId);
    } else {
      return this.addProviderToFavorites(providerId);
    }
  }

  // Check if service is favorite
  isServiceFavorite(serviceId: string): boolean {
    return this.favoriteServiceIdsSubject.value.has(serviceId);
  }

  // Check if provider is favorite
  isProviderFavorite(providerId: string): boolean {
    return this.favoriteProviderIdsSubject.value.has(providerId);
  }

  // Get current favorite services
  getCurrentFavoriteServices(): FavoriteService[] {
    return this.favoriteServicesSubject.value;
  }

  // Get current favorite providers
  getCurrentFavoriteProviders(): FavoriteProvider[] {
    return this.favoriteProvidersSubject.value;
  }

  // Get favorites count
  getFavoriteServicesCount(): number {
    return this.favoriteServiceIdsSubject.value.size;
  }

  getFavoriteProvidersCount(): number {
    return this.favoriteProviderIdsSubject.value.size;
  }

  getTotalFavoritesCount(): number {
    return this.getFavoriteServicesCount() + this.getFavoriteProvidersCount();
  }

  // Clear all favorites
  clearAllFavorites(): Observable<any> {
    return this.apiService.delete('favorites/all').pipe(
      tap(() => {
        this.favoriteServicesSubject.next([]);
        this.favoriteProvidersSubject.next([]);
        this.favoriteServiceIdsSubject.next(new Set());
        this.favoriteProviderIdsSubject.next(new Set());
      })
    );
  }

  // Get favorite service by ID
  getFavoriteServiceById(serviceId: string): FavoriteService | undefined {
    return this.favoriteServicesSubject.value.find(f => f.serviceId === serviceId);
  }

  // Get favorite provider by ID
  getFavoriteProviderById(providerId: string): FavoriteProvider | undefined {
    return this.favoriteProvidersSubject.value.find(f => f.providerId === providerId);
  }

  // Check if any favorites exist
  hasFavorites(): boolean {
    return this.getTotalFavoritesCount() > 0;
  }

  // Get recently added favorites
  getRecentlyAddedServices(limit: number = 5): FavoriteService[] {
    return this.favoriteServicesSubject.value
      .sort((a, b) => new Date(b.addedAt).getTime() - new Date(a.addedAt).getTime())
      .slice(0, limit);
  }

  // Get recently added providers
  getRecentlyAddedProviders(limit: number = 5): FavoriteProvider[] {
    return this.favoriteProvidersSubject.value
      .sort((a, b) => new Date(b.addedAt).getTime() - new Date(a.addedAt).getTime())
      .slice(0, limit);
  }

  // Filter favorite services by category
  filterServicesByCategory(categoryName: string): FavoriteService[] {
    return this.favoriteServicesSubject.value.filter(
      f => f.categoryName === categoryName || f.categoryNameAr === categoryName
    );
  }

  // Search favorite services
  searchFavoriteServices(searchTerm: string): FavoriteService[] {
    const term = searchTerm.toLowerCase();
    return this.favoriteServicesSubject.value.filter(
      f =>
        f.serviceName.toLowerCase().includes(term) ||
        f.serviceNameAr.includes(term) ||
        f.providerName.toLowerCase().includes(term) ||
        f.categoryName.toLowerCase().includes(term)
    );
  }

  // Search favorite providers
  searchFavoriteProviders(searchTerm: string): FavoriteProvider[] {
    const term = searchTerm.toLowerCase();
    return this.favoriteProvidersSubject.value.filter(
      f =>
        f.providerName.toLowerCase().includes(term) ||
        f.businessName?.toLowerCase().includes(term)
    );
  }

  // Get favorite icon state
  getFavoriteIconClass(isFavorite: boolean): string {
    return isFavorite ? 'favorite-active' : 'favorite-inactive';
  }

  // Get favorite button text
  getFavoriteButtonText(isFavorite: boolean): string {
    return isFavorite ? 'Remove from Favorites' : 'Add to Favorites';
  }

  // Export favorites as JSON
  exportFavoritesAsJson(): string {
    const data = {
      services: this.favoriteServicesSubject.value,
      providers: this.favoriteProvidersSubject.value,
      exportedAt: new Date().toISOString()
    };
    return JSON.stringify(data, null, 2);
  }
}
