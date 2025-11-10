import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { ServiceService, ServiceDetail, Review } from '../../../core/services/service.service';
import { BookingService, CreateBookingRequest } from '../../../core/services/booking.service';
import { AddressService, Address } from '../../../core/services/address.service';
import { FavoritesService } from '../../../core/services/favorites.service';
import { PromoCodeService, ValidatePromoCodeRequest } from '../../../core/services/promo-code.service';
import { AuthService } from '../../../core/services/auth.service';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-service-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, FormsModule],
  templateUrl: './service-detail.component.html',
  styleUrls: ['./service-detail.component.scss']
})
export class ServiceDetailComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  serviceId: string = '';
  service: ServiceDetail | null = null;
  reviews: Review[] = [];

  loading = true;
  loadingAddresses = false;
  bookingInProgress = false;
  validatingPromoCode = false;

  currentLanguage = 'en';
  isLoggedIn = false;
  isFavorite = false;

  // Booking flow
  showBookingModal = false;
  bookingStep: 'datetime' | 'address' | 'promo' | 'confirm' = 'datetime';

  addresses: Address[] = [];
  selectedAddressId: string = '';

  scheduledDate: string = '';
  scheduledTime: string = '';
  customerNotes: string = '';
  promoCode: string = '';
  promoCodeValid = false;
  promoCodeMessage: string = '';

  // Price calculation
  basePrice: number = 0;
  promoDiscount: number = 0;
  totalAmount: number = 0;

  // Review form
  showReviewForm = false;
  reviewRating: number = 5;
  reviewComment: string = '';
  submittingReview = false;

  // Image gallery
  currentImageIndex = 0;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private serviceService: ServiceService,
    private bookingService: BookingService,
    private addressService: AddressService,
    private favoritesService: FavoritesService,
    private promoCodeService: PromoCodeService,
    private authService: AuthService,
    private languageService: LanguageService
  ) {}

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isAuthenticated();
    this.currentLanguage = this.languageService.getCurrentLanguage();

    // Subscribe to language changes
    this.languageService.currentLanguage$
      .pipe(takeUntil(this.destroy$))
      .subscribe(lang => {
        this.currentLanguage = lang;
      });

    // Get service ID from route
    this.route.params
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        this.serviceId = params['id'];
        this.loadServiceDetails();
      });

    // Subscribe to favorites
    this.favoritesService.favoriteServiceIds$
      .pipe(takeUntil(this.destroy$))
      .subscribe(ids => {
        this.isFavorite = ids.has(this.serviceId);
      });

    // Subscribe to addresses
    this.addressService.addresses$
      .pipe(takeUntil(this.destroy$))
      .subscribe(addresses => {
        this.addresses = addresses;
        // Auto-select default address
        const defaultAddress = addresses.find(a => a.isDefault);
        if (defaultAddress) {
          this.selectedAddressId = defaultAddress.addressId;
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadServiceDetails(): void {
    this.loading = true;
    this.serviceService.getServiceById(this.serviceId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (service) => {
          this.service = service;
          this.basePrice = service.basePrice;
          this.totalAmount = service.basePrice;
          this.reviews = service.recentReviews || [];
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading service details:', error);
          this.loading = false;
          // Navigate back if service not found
          this.router.navigate(['/services']);
        }
      });
  }

  // Favorites
  toggleFavorite(): void {
    this.favoritesService.toggleServiceFavorite(this.serviceId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        error: (error) => {
          console.error('Error toggling favorite:', error);
        }
      });
  }

  // Booking flow
  startBooking(): void {
    if (!this.isLoggedIn) {
      this.router.navigate(['/auth/login'], {
        queryParams: { returnUrl: `/services/${this.serviceId}` }
      });
      return;
    }

    this.showBookingModal = true;
    this.bookingStep = 'datetime';
    this.resetBookingForm();

    // Load addresses if not already loaded
    if (this.addresses.length === 0) {
      this.addressService.loadAddresses();
    }
  }

  resetBookingForm(): void {
    this.scheduledDate = '';
    this.scheduledTime = '';
    this.customerNotes = '';
    this.promoCode = '';
    this.promoCodeValid = false;
    this.promoCodeMessage = '';
    this.promoDiscount = 0;
    this.totalAmount = this.basePrice;
  }

  closeBookingModal(): void {
    this.showBookingModal = false;
    this.resetBookingForm();
  }

  nextBookingStep(): void {
    if (this.bookingStep === 'datetime') {
      if (!this.scheduledDate || !this.scheduledTime) {
        alert('Please select date and time');
        return;
      }
      this.bookingStep = 'address';
    } else if (this.bookingStep === 'address') {
      if (!this.selectedAddressId) {
        alert('Please select an address');
        return;
      }
      this.bookingStep = 'promo';
    } else if (this.bookingStep === 'promo') {
      this.bookingStep = 'confirm';
    }
  }

  previousBookingStep(): void {
    if (this.bookingStep === 'confirm') {
      this.bookingStep = 'promo';
    } else if (this.bookingStep === 'promo') {
      this.bookingStep = 'address';
    } else if (this.bookingStep === 'address') {
      this.bookingStep = 'datetime';
    }
  }

  validatePromoCode(): void {
    if (!this.promoCode.trim()) {
      this.promoCodeMessage = 'Please enter a promo code';
      return;
    }

    this.validatingPromoCode = true;
    const request: ValidatePromoCodeRequest = {
      code: this.promoCode,
      serviceId: this.serviceId,
      subtotalAmount: this.basePrice
    };

    this.promoCodeService.validatePromoCode(request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.validatingPromoCode = false;
          this.promoCodeValid = result.isValid;
          this.promoCodeMessage = result.message;

          if (result.isValid) {
            this.promoDiscount = result.discountAmount || 0;
            this.totalAmount = result.finalAmount || this.basePrice;
          } else {
            this.promoDiscount = 0;
            this.totalAmount = this.basePrice;
          }
        },
        error: (error) => {
          console.error('Error validating promo code:', error);
          this.validatingPromoCode = false;
          this.promoCodeValid = false;
          this.promoCodeMessage = 'Error validating promo code';
        }
      });
  }

  confirmBooking(): void {
    if (!this.service) return;

    const scheduledDateTime = `${this.scheduledDate}T${this.scheduledTime}:00`;

    const bookingRequest: CreateBookingRequest = {
      serviceId: this.serviceId,
      providerId: this.service.provider.providerId,
      addressId: this.selectedAddressId,
      scheduledDateTime: scheduledDateTime,
      customerNotes: this.customerNotes || undefined,
      promoCode: this.promoCodeValid ? this.promoCode : undefined
    };

    this.bookingInProgress = true;

    this.bookingService.createBooking(bookingRequest)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.bookingInProgress = false;
          this.closeBookingModal();

          // Navigate to payment page to complete the booking
          this.router.navigate(['/bookings', result.bookingId, 'payment']);
        },
        error: (error) => {
          console.error('Error creating booking:', error);
          this.bookingInProgress = false;
          alert('Failed to create booking. Please try again.');
        }
      });
  }

  // Reviews
  submitReview(): void {
    if (!this.reviewComment.trim()) {
      alert('Please enter a review comment');
      return;
    }

    this.submittingReview = true;

    this.serviceService.submitReview({
      serviceId: this.serviceId,
      rating: this.reviewRating,
      comment: this.reviewComment
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.submittingReview = false;
          this.showReviewForm = false;
          this.reviewComment = '';
          this.reviewRating = 5;

          // Reload service to get updated reviews
          this.loadServiceDetails();
        },
        error: (error) => {
          console.error('Error submitting review:', error);
          this.submittingReview = false;
          alert('Failed to submit review. Please try again.');
        }
      });
  }

  // Image gallery
  previousImage(): void {
    if (!this.service || !this.service.images) return;
    this.currentImageIndex =
      this.currentImageIndex === 0
        ? this.service.images.length - 1
        : this.currentImageIndex - 1;
  }

  nextImage(): void {
    if (!this.service || !this.service.images) return;
    this.currentImageIndex =
      this.currentImageIndex === this.service.images.length - 1
        ? 0
        : this.currentImageIndex + 1;
  }

  selectImage(index: number): void {
    this.currentImageIndex = index;
  }

  // Helper methods
  getServiceName(): string {
    if (!this.service) return '';
    return this.currentLanguage === 'ar' ? this.service.nameAr : this.service.nameEn;
  }

  getServiceDescription(): string {
    if (!this.service) return '';
    return this.currentLanguage === 'ar'
      ? this.service.descriptionAr || ''
      : this.service.descriptionEn || '';
  }

  formatPrice(price: number): string {
    return `SAR ${price.toFixed(2)}`;
  }

  formatDuration(minutes: number): string {
    if (minutes < 60) {
      return `${minutes} minutes`;
    }
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return mins > 0 ? `${hours}h ${mins}m` : `${hours}h`;
  }

  getStarArray(rating: number): boolean[] {
    return Array(5).fill(false).map((_, i) => i < Math.floor(rating));
  }

  getRatingStars(rating: number): number[] {
    return Array.from({ length: 5 }, (_, i) => i + 1);
  }

  setReviewRating(rating: number): void {
    this.reviewRating = rating;
  }

  formatAddress(address: Address): string {
    return this.addressService.formatAddress(address);
  }

  getMinScheduleDate(): string {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    return tomorrow.toISOString().split('T')[0];
  }

  getBookingStepTitle(): string {
    const titles: Record<string, string> = {
      'datetime': 'Select Date & Time',
      'address': 'Select Address',
      'promo': 'Promo Code (Optional)',
      'confirm': 'Confirm Booking'
    };
    return titles[this.bookingStep] || '';
  }

  getBookingStepNumber(): number {
    const steps: Record<string, number> = {
      'datetime': 1,
      'address': 2,
      'promo': 3,
      'confirm': 4
    };
    return steps[this.bookingStep] || 1;
  }

  canGoToNextStep(): boolean {
    if (this.bookingStep === 'datetime') {
      return !!(this.scheduledDate && this.scheduledTime);
    } else if (this.bookingStep === 'address') {
      return !!this.selectedAddressId;
    }
    return true;
  }
}
