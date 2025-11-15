# Integration Checklist - Quick Setup Guide

Use this checklist to integrate all the implemented features into your application.

---

## ✅ Step 1: Add Favorites Route (2 minutes)

**File:** `frontend/src/app/app.routes.ts`

Add this route:

```typescript
{
  path: 'favorites',
  loadComponent: () => import('./features/favorites/favorites').then(m => m.Favorites),
  canActivate: [AuthGuard] // Add if authentication is required
}
```

**Test:**
- Navigate to `http://localhost:4200/favorites`
- Should see the favorites page

---

## ✅ Step 2: Add Favorites to Navigation (5 minutes)

### Option A: In Header/Toolbar

**Find your header component** (e.g., `header.component.html`)

Add:
```html
<button mat-button routerLink="/favorites">
  <mat-icon>favorite</mat-icon>
  <span>{{ 'favorites.title' | translate }}</span>
</button>
```

### Option B: In Side Menu

```html
<a mat-list-item routerLink="/favorites">
  <mat-icon matListItemIcon>favorite</mat-icon>
  <span matListItemTitle>{{ 'favorites.title' | translate }}</span>
</a>
```

**Test:**
- Click the favorites link
- Should navigate to favorites page

---

## ✅ Step 3: Add Favorite Button to Service Cards (10 minutes)

**Find your service card component** (e.g., `service-card.component.html`)

### Add to Template:

```html
<mat-card class="service-card">
  <!-- Existing content -->

  <!-- Add this at the top of the card or in a corner -->
  <app-favorite-button
    class="favorite-btn"
    [serviceId]="service.id"
    [isFavorite]="service.isFavorite || false"
    (favoriteToggle)="onFavoriteToggle($event)">
  </app-favorite-button>

  <!-- Rest of card content -->
</mat-card>
```

### Add to Component TypeScript:

```typescript
import { FavoritesService } from '../../core/services/favorites.service';
import { NotificationService } from '../../core/services/notification.service';

// In constructor
constructor(
  private favoritesService: FavoritesService,
  private notificationService: NotificationService
) {}

// Add method
onFavoriteToggle(isFavorite: boolean): void {
  const serviceId = this.service.id;

  if (isFavorite) {
    // Add to favorites
    this.favoritesService.addFavorite(serviceId).subscribe({
      next: () => {
        this.service.isFavorite = true;
        this.notificationService.showSuccess('favorites.addedToFavorites');
      },
      error: () => {
        this.notificationService.showError('Failed to add to favorites');
      }
    });
  } else {
    // Remove from favorites
    this.favoritesService.removeFavorite(serviceId).subscribe({
      next: () => {
        this.service.isFavorite = false;
        this.notificationService.showSuccess('favorites.removedFromFavorites');
      },
      error: () => {
        this.notificationService.showError('Failed to remove from favorites');
      }
    });
  }
}
```

### Add to Component Imports:

```typescript
import { FavoriteButton } from '../../shared/components/favorite-button/favorite-button';

@Component({
  // ...
  imports: [
    // ... existing imports
    FavoriteButton
  ]
})
```

### Optional CSS (for positioning):

```scss
.service-card {
  position: relative;

  .favorite-btn {
    position: absolute;
    top: 8px;
    right: 8px;
    z-index: 10;
  }
}
```

**Test:**
- Click the heart icon on a service card
- Should toggle favorite status
- Check favorites page - service should appear/disappear

---

## ✅ Step 4: Add Promo Code to Checkout/Booking (10 minutes)

**Find your booking or checkout component** (e.g., `booking.component.html`)

### Add to Template:

```html
<div class="booking-container">
  <!-- Existing booking form -->

  <!-- Add promo code section before payment summary -->
  <div class="promo-section">
    <h3>{{ 'promoCode.label' | translate }}</h3>
    <app-promo-code-input
      (promoApplied)="onPromoApplied($event)"
      (promoRemoved)="onPromoRemoved()">
    </app-promo-code-input>
  </div>

  <!-- Payment summary -->
  <div class="payment-summary">
    <div class="summary-row">
      <span>{{ 'serviceDetail.booking.servicePrice' | translate }}</span>
      <span>{{ servicePrice | currency:'USD':'symbol':'1.2-2' }}</span>
    </div>

    @if (discount) {
      <div class="summary-row discount">
        <span>{{ 'serviceDetail.booking.discount' | translate }}</span>
        <span>-{{ discount.discountAmount | currency:'USD':'symbol':'1.2-2' }}</span>
      </div>
    }

    <div class="summary-row total">
      <span>{{ 'serviceDetail.booking.total' | translate }}</span>
      <span>{{ totalAmount | currency:'USD':'symbol':'1.2-2' }}</span>
    </div>
  </div>
</div>
```

### Add to Component TypeScript:

```typescript
import { PromoCodeInput } from '../../shared/components/promo-code-input/promo-code-input';

// In component class
discount: any = null;
servicePrice = 100; // Your service price
totalAmount = 100;

// Add methods
onPromoApplied(discount: any): void {
  this.discount = discount;
  this.calculateTotal();
}

onPromoRemoved(): void {
  this.discount = null;
  this.calculateTotal();
}

calculateTotal(): void {
  this.totalAmount = this.servicePrice;

  if (this.discount) {
    if (this.discount.discountType === 'Percentage') {
      this.totalAmount -= (this.servicePrice * this.discount.discountValue / 100);
    } else if (this.discount.discountType === 'Fixed') {
      this.totalAmount -= this.discount.discountAmount;
    }
  }

  // Ensure total is not negative
  this.totalAmount = Math.max(0, this.totalAmount);
}
```

### Add to Component Imports:

```typescript
@Component({
  // ...
  imports: [
    // ... existing imports
    PromoCodeInput
  ]
})
```

**Test:**
- Enter a promo code (create one in backend if needed)
- Should see discount applied
- Total should update
- Remove promo code - total should revert

---

## ✅ Step 5: Add Reviews to Service Detail Page (15 minutes)

**Find your service detail component** (e.g., `service-detail.component.html`)

### Add to Template:

```html
<div class="service-detail">
  <!-- Existing service details -->

  <!-- Reviews Section -->
  <section class="reviews-section">
    <div class="section-header">
      <h2>{{ 'serviceDetail.reviews.title' | translate }}</h2>

      @if (canWriteReview) {
        <button mat-raised-button color="primary" (click)="openReviewForm()">
          <mat-icon>rate_review</mat-icon>
          {{ 'serviceDetail.reviews.writeReview' | translate }}
        </button>
      }
    </div>

    @if (reviews.length === 0) {
      <div class="no-reviews">
        <mat-icon>rate_review</mat-icon>
        <p>{{ 'serviceDetail.reviews.noReviews' | translate }}</p>
        <p>{{ 'serviceDetail.reviews.beFirst' | translate }}</p>
      </div>
    } @else {
      <!-- Display reviews -->
      @for (review of reviews; track review.id) {
        <app-review-card
          [review]="review"
          [canEdit]="review.userId === currentUserId"
          [canDelete]="review.userId === currentUserId"
          (helpful)="onMarkHelpful($event)"
          (edit)="onEditReview($event)"
          (delete)="onDeleteReview($event)">
        </app-review-card>
      }

      <!-- Load more button if needed -->
      @if (hasMoreReviews) {
        <button mat-stroked-button (click)="loadMoreReviews()">
          {{ 'common.loadMore' | translate }}
        </button>
      }
    }
  </section>
</div>
```

### Add to Component TypeScript:

```typescript
import { ReviewCard } from '../../shared/components/review-card/review-card';
import { ReviewForm } from '../../shared/components/review-form/review-form';
import { ReviewService } from '../../core/services/review.service';
import { MatDialog } from '@angular/material/dialog';

// In component class
reviews: any[] = [];
currentUserId: string = ''; // Get from auth service
canWriteReview = false;
hasMoreReviews = false;
pageNumber = 1;

constructor(
  private reviewService: ReviewService,
  private dialog: MatDialog,
  // ... other services
) {}

ngOnInit(): void {
  this.loadReviews();
  // Check if user can write review (has completed booking)
}

loadReviews(): void {
  this.reviewService.getServiceReviews(this.serviceId, {
    pageNumber: this.pageNumber,
    pageSize: 10
  }).subscribe({
    next: (response) => {
      this.reviews = response.items;
      this.hasMoreReviews = response.totalCount > this.reviews.length;
    }
  });
}

openReviewForm(): void {
  const dialogRef = this.dialog.open(ReviewForm, {
    width: '600px',
    data: { bookingId: this.bookingId } // Pass booking ID
  });

  dialogRef.componentInstance.submitReview.subscribe((reviewData) => {
    this.reviewService.createReview(reviewData).subscribe({
      next: () => {
        this.notificationService.showSuccess('reviews.messages.submitSuccess');
        dialogRef.close();
        this.loadReviews();
      }
    });
  });
}

onMarkHelpful(reviewId: string): void {
  this.reviewService.markReviewHelpful(reviewId).subscribe({
    next: () => {
      // Update review in list
      const review = this.reviews.find(r => r.id === reviewId);
      if (review) {
        review.helpfulCount++;
        review.isHelpful = true;
      }
    }
  });
}

onEditReview(reviewId: string): void {
  const review = this.reviews.find(r => r.id === reviewId);
  // Open edit dialog with review data
}

onDeleteReview(reviewId: string): void {
  // Confirm and delete
  this.reviewService.deleteReview(reviewId).subscribe({
    next: () => {
      this.reviews = this.reviews.filter(r => r.id !== reviewId);
      this.notificationService.showSuccess('reviews.messages.deleteSuccess');
    }
  });
}
```

### Add to Component Imports:

```typescript
@Component({
  // ...
  imports: [
    // ... existing imports
    ReviewCard,
    MatDialogModule
  ]
})
```

**Test:**
- View service detail page
- Should see reviews
- Try marking a review as helpful
- If you have a completed booking, try writing a review

---

## 🎯 Quick Verification Checklist

After completing the above steps, verify:

- [ ] Can navigate to `/favorites` page
- [ ] Favorites page shows empty state when no favorites
- [ ] Can see favorite button on service cards
- [ ] Can add/remove favorites by clicking heart icon
- [ ] Added favorites appear on favorites page
- [ ] Can click service card to view details
- [ ] Promo code input appears on checkout/booking
- [ ] Can enter and apply a promo code
- [ ] Discount is calculated correctly
- [ ] Can remove promo code
- [ ] Reviews appear on service detail page
- [ ] Can mark review as helpful
- [ ] Can write a review (if have completed booking)
- [ ] All text is translated (test in Arabic too)

---

## 🔧 Troubleshooting

### Favorites not loading?
- Check that `FavoritesService` is imported
- Verify API endpoint is correct
- Check browser console for errors

### Promo code not working?
- Verify backend has promo codes created
- Check API response in network tab
- Ensure `PromoCodeService` is working

### Reviews not showing?
- Check service has reviews in backend
- Verify `ReviewService` API calls
- Check component is importing `ReviewCard`

### Translations missing?
- Verify keys exist in `en.json` and `ar.json`
- Check TranslateModule is imported
- Ensure language is loaded

---

## 📱 Test on Different Devices

- [ ] Desktop (1920x1080)
- [ ] Tablet (768x1024)
- [ ] Mobile (375x667)
- [ ] RTL mode (Arabic)

---

## 🚀 You're Done!

All features are now integrated. Test thoroughly and enjoy your new functionality!

**Need help?** Check:
- `SESSION_COMPLETE_SUMMARY.md` for detailed info
- `QUICK_IMPLEMENTATION_GUIDE.md` for more examples
- Existing components for patterns

**Questions?** Review the implemented code in:
- `frontend/src/app/shared/components/`
- `frontend/src/app/features/favorites/`
