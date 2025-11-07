import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil, forkJoin } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { CategoryService, Category } from '../../core/services/category.service';
import { ServiceService, Service } from '../../core/services/service.service';
import { BookingService, Booking } from '../../core/services/booking.service';
import { NotificationService } from '../../core/services/notification.service';
import { LanguageService } from '../../core/services/language.service';

interface ServiceCategory {
  id: string;
  name: string;
  icon: string;
  route: string;
}

interface PromoBanner {
  id: string;
  imageUrl: string;
  title: string;
  description: string;
  link?: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, FormsModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  currentLocation = 'Riyadh, Saudi Arabia';
  notificationCount = 0;
  searchQuery = '';
  recentSearches: string[] = [];
  currentBannerIndex = 0;
  isLoggedIn = false;
  currentLanguage = 'en';

  // Loading states
  isLoadingCategories = true;
  isLoadingFeaturedServices = true;
  isLoadingBookings = false;

  categories: ServiceCategory[] = [];
  featuredServices: Service[] = [];
  recentBookings: Booking[] = [];

  promoBanners: PromoBanner[] = [
    {
      id: '1',
      imageUrl: 'assets/images/promo1.jpg',
      title: 'First Booking 20% Off',
      description: 'Book your first service and get instant discount',
      link: '/services'
    },
    {
      id: '2',
      imageUrl: 'assets/images/promo2.jpg',
      title: 'AC Cleaning Special',
      description: 'Summer Special - Get your AC cleaned at discounted rates',
      link: '/services?category=ac'
    },
    {
      id: '3',
      imageUrl: 'assets/images/promo3.jpg',
      title: 'Refer & Earn',
      description: 'Invite friends and earn rewards',
      link: '/profile/referral'
    }
  ];

  constructor(
    private authService: AuthService,
    private categoryService: CategoryService,
    private serviceService: ServiceService,
    private bookingService: BookingService,
    private notificationService: NotificationService,
    private languageService: LanguageService,
    private router: Router
  ) {
    // Load recent searches from localStorage
    const savedSearches = localStorage.getItem('recentSearches');
    if (savedSearches) {
      this.recentSearches = JSON.parse(savedSearches);
    }
  }

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isAuthenticated();

    // Get current language
    this.currentLanguage = this.languageService.getCurrentLanguage();

    // Subscribe to language changes
    this.languageService.currentLanguage$
      .pipe(takeUntil(this.destroy$))
      .subscribe(lang => {
        this.currentLanguage = lang;
        this.loadCategories(); // Reload categories when language changes
      });

    // Load initial data
    this.loadCategories();
    this.loadFeaturedServices();

    if (this.isLoggedIn) {
      this.loadRecentBookings();
      this.loadNotificationCount();
    }

    // Auto-rotate banners every 5 seconds
    setInterval(() => {
      this.nextBanner();
    }, 5000);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadCategories(): void {
    this.isLoadingCategories = true;
    this.categoryService.getActiveCategories()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.categories = result.categories.map((cat: Category) => ({
            id: cat.categoryId,
            name: this.categoryService.getLocalizedName(cat, this.currentLanguage),
            icon: this.categoryService.getCategoryIcon(cat),
            route: this.categoryService.getCategoryRoute(cat.categoryId)
          }));
          this.categories = this.categories.slice(0, 9); // Show only top 9
          this.isLoadingCategories = false;
        },
        error: (error) => {
          console.error('Error loading categories:', error);
          this.isLoadingCategories = false;
          // Keep empty array, template will handle empty state
        }
      });
  }

  loadFeaturedServices(): void {
    this.isLoadingFeaturedServices = true;
    this.serviceService.searchServices({
      sortBy: 'rating',
      sortOrder: 'desc',
      pageSize: 8
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.featuredServices = result.services;
          this.isLoadingFeaturedServices = false;
        },
        error: (error) => {
          console.error('Error loading featured services:', error);
          this.isLoadingFeaturedServices = false;
        }
      });
  }

  loadRecentBookings(): void {
    this.isLoadingBookings = true;
    this.bookingService.getBookingHistory({
      pageSize: 3,
      sortBy: 'scheduledDateTime',
      sortOrder: 'desc'
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.recentBookings = result.bookings;
          this.isLoadingBookings = false;
        },
        error: (error) => {
          console.error('Error loading recent bookings:', error);
          this.isLoadingBookings = false;
        }
      });
  }

  loadNotificationCount(): void {
    this.notificationService.unreadCount$
      .pipe(takeUntil(this.destroy$))
      .subscribe(count => {
        this.notificationCount = count;
      });
  }

  onSearch(): void {
    if (this.searchQuery.trim()) {
      // Add to recent searches
      if (!this.recentSearches.includes(this.searchQuery)) {
        this.recentSearches.unshift(this.searchQuery);
        if (this.recentSearches.length > 5) {
          this.recentSearches.pop();
        }
        // Save to localStorage
        localStorage.setItem('recentSearches', JSON.stringify(this.recentSearches));
      }
      // Navigate to search results
      this.router.navigate(['/services'], { queryParams: { q: this.searchQuery } });
    }
  }

  searchFromRecent(searchTerm: string): void {
    this.searchQuery = searchTerm;
    this.onSearch();
  }

  changeLocation(): void {
    // Open location picker dialog
    console.log('Change location clicked');
    // TODO: Implement location picker modal
  }

  previousBanner(): void {
    this.currentBannerIndex =
      this.currentBannerIndex === 0
        ? this.promoBanners.length - 1
        : this.currentBannerIndex - 1;
  }

  nextBanner(): void {
    this.currentBannerIndex =
      this.currentBannerIndex === this.promoBanners.length - 1
        ? 0
        : this.currentBannerIndex + 1;
  }

  goToBanner(index: number): void {
    this.currentBannerIndex = index;
  }

  bookAgain(booking: Booking): void {
    // Navigate to service detail to book again
    this.router.navigate(['/services', booking.serviceId]);
  }

  viewBookingDetails(bookingId: string): void {
    this.router.navigate(['/bookings', bookingId]);
  }

  getStarArray(rating: number): boolean[] {
    return Array(5).fill(false).map((_, i) => i < Math.floor(rating));
  }

  getServiceName(service: Service): string {
    return this.currentLanguage === 'ar' ? service.nameAr : service.nameEn;
  }

  formatPrice(price: number): string {
    return `SAR ${price.toFixed(2)}`;
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString(this.currentLanguage === 'ar' ? 'ar-SA' : 'en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getBookingStatusClass(status: string): string {
    const statusMap: Record<string, string> = {
      'Pending': 'badge-warning',
      'Confirmed': 'badge-info',
      'InProgress': 'badge-primary',
      'Completed': 'badge-success',
      'Cancelled': 'badge-danger',
      'Rejected': 'badge-danger'
    };
    return statusMap[status] || 'badge-secondary';
  }

  getBookingStatusLabel(status: string): string {
    const statusMap: Record<string, string> = {
      'Pending': 'Pending',
      'Confirmed': 'Confirmed',
      'InProgress': 'In Progress',
      'Completed': 'Completed',
      'Cancelled': 'Cancelled',
      'Rejected': 'Rejected'
    };
    return statusMap[status] || status;
  }

  trackByServiceId(index: number, service: Service): string {
    return service.serviceId;
  }

  trackByBookingId(index: number, booking: Booking): string {
    return booking.bookingId;
  }

  trackByCategoryId(index: number, category: ServiceCategory): string {
    return category.id;
  }
}
