import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, takeUntil } from 'rxjs';
import { ServiceService, Service, ServiceSearchParams } from '../../../core/services/service.service';
import { CategoryService, Category } from '../../../core/services/category.service';
import { FavoritesService } from '../../../core/services/favorites.service';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-services-list',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, FormsModule],
  templateUrl: './services-list.component.html',
  styleUrls: ['./services-list.component.scss']
})
export class ServicesListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private searchSubject$ = new Subject<string>();

  services: Service[] = [];
  categories: Category[] = [];
  loading = false;
  currentLanguage = 'en';

  // Search and Filter state
  searchTerm = '';
  selectedCategoryId: string | null = null;
  selectedRegion: string | null = null;
  minPrice: number | null = null;
  maxPrice: number | null = null;
  minRating: number | null = null;
  sortBy: string = 'rating';
  sortOrder: 'asc' | 'desc' = 'desc';

  // Pagination
  currentPage = 1;
  pageSize = 12;
  totalCount = 0;
  totalPages = 0;

  // Available options
  regions = [
    { value: 'Riyadh', label: 'Riyadh' },
    { value: 'Jeddah', label: 'Jeddah' },
    { value: 'Makkah', label: 'Makkah' },
    { value: 'Madinah', label: 'Madinah' },
    { value: 'Dammam', label: 'Dammam' },
    { value: 'Khobar', label: 'Khobar' }
  ];

  sortOptions = [
    { value: 'rating', label: 'Highest Rated' },
    { value: 'price', label: 'Price' },
    { value: 'name', label: 'Name' }
  ];

  ratingOptions = [
    { value: 4, label: '4+ Stars' },
    { value: 3, label: '3+ Stars' },
    { value: 2, label: '2+ Stars' }
  ];

  // Favorites tracking
  favoriteServiceIds = new Set<string>();

  constructor(
    private serviceService: ServiceService,
    private categoryService: CategoryService,
    private favoritesService: FavoritesService,
    private languageService: LanguageService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Get current language
    this.currentLanguage = this.languageService.getCurrentLanguage();

    // Subscribe to language changes
    this.languageService.currentLanguage$
      .pipe(takeUntil(this.destroy$))
      .subscribe(lang => {
        this.currentLanguage = lang;
      });

    // Load categories
    this.loadCategories();

    // Subscribe to favorite service IDs
    this.favoritesService.favoriteServiceIds$
      .pipe(takeUntil(this.destroy$))
      .subscribe(ids => {
        this.favoriteServiceIds = ids;
      });

    // Subscribe to query params
    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        this.searchTerm = params['q'] || '';
        this.selectedCategoryId = params['category'] || null;
        this.selectedRegion = params['region'] || null;
        this.loadServices();
      });

    // Debounced search
    this.searchSubject$
      .pipe(debounceTime(500), takeUntil(this.destroy$))
      .subscribe(() => {
        this.currentPage = 1;
        this.loadServices();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadCategories(): void {
    this.categoryService.getActiveCategories()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.categories = result.categories;
        },
        error: (error) => {
          console.error('Error loading categories:', error);
        }
      });
  }

  loadServices(): void {
    this.loading = true;

    const params: ServiceSearchParams = {
      searchTerm: this.searchTerm || undefined,
      categoryId: this.selectedCategoryId || undefined,
      region: this.selectedRegion || undefined,
      minPrice: this.minPrice || undefined,
      maxPrice: this.maxPrice || undefined,
      minRating: this.minRating || undefined,
      sortBy: this.sortBy,
      sortOrder: this.sortOrder,
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    };

    this.serviceService.searchServices(params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.services = result.services;
          this.totalCount = result.totalCount;
          this.totalPages = Math.ceil(this.totalCount / this.pageSize);
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading services:', error);
          this.loading = false;
        }
      });
  }

  onSearchChange(): void {
    this.searchSubject$.next(this.searchTerm);
  }

  onCategoryChange(categoryId: string | null): void {
    this.selectedCategoryId = categoryId;
    this.currentPage = 1;
    this.updateQueryParams();
    this.loadServices();
  }

  onRegionChange(): void {
    this.currentPage = 1;
    this.updateQueryParams();
    this.loadServices();
  }

  onPriceFilterChange(): void {
    this.currentPage = 1;
    this.loadServices();
  }

  onRatingChange(): void {
    this.currentPage = 1;
    this.loadServices();
  }

  onSortChange(): void {
    this.currentPage = 1;
    this.loadServices();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedCategoryId = null;
    this.selectedRegion = null;
    this.minPrice = null;
    this.maxPrice = null;
    this.minRating = null;
    this.sortBy = 'rating';
    this.sortOrder = 'desc';
    this.currentPage = 1;
    this.updateQueryParams();
    this.loadServices();
  }

  updateQueryParams(): void {
    const queryParams: any = {};

    if (this.searchTerm) queryParams.q = this.searchTerm;
    if (this.selectedCategoryId) queryParams.category = this.selectedCategoryId;
    if (this.selectedRegion) queryParams.region = this.selectedRegion;

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      queryParamsHandling: 'merge'
    });
  }

  // Pagination
  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadServices();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.goToPage(this.currentPage - 1);
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.goToPage(this.currentPage + 1);
    }
  }

  get paginationPages(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);

    if (end - start < maxVisible - 1) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    return pages;
  }

  // Favorites
  toggleFavorite(serviceId: string, event: Event): void {
    event.preventDefault();
    event.stopPropagation();

    this.favoritesService.toggleServiceFavorite(serviceId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          // State is automatically updated via BehaviorSubject
        },
        error: (error) => {
          console.error('Error toggling favorite:', error);
        }
      });
  }

  isFavorite(serviceId: string): boolean {
    return this.favoriteServiceIds.has(serviceId);
  }

  // Helper methods
  getServiceName(service: Service): string {
    return this.currentLanguage === 'ar' ? service.nameAr : service.nameEn;
  }

  getCategoryLabel(service: Service): string {
    return this.currentLanguage === 'ar' ? service.categoryNameAr : service.categoryNameEn;
  }

  getCategoryName(category: Category): string {
    return this.categoryService.getLocalizedName(category, this.currentLanguage);
  }

  formatPrice(price: number): string {
    return `SAR ${price.toFixed(2)}`;
  }

  getStarArray(rating: number): boolean[] {
    return Array(5).fill(false).map((_, i) => i < Math.floor(rating));
  }

  trackByServiceId(index: number, service: Service): string {
    return service.id;
  }

  trackByCategoryId(index: number, category: Category): string {
    return category.categoryId;
  }
}
