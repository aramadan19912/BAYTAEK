import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';

interface ServiceCategory {
  id: string;
  name: string;
  icon: string;
  route: string;
}

interface FeaturedService {
  id: string;
  name: string;
  imageUrl: string;
  price: number;
  rating: number;
  reviewCount: number;
}

interface RecentBooking {
  id: string;
  serviceName: string;
  providerName: string;
  scheduledDate: Date;
  status: string;
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
export class HomeComponent implements OnInit {
  currentLocation = 'Riyadh, Saudi Arabia';
  notificationCount = 3;
  searchQuery = '';
  recentSearches: string[] = ['Cleaning', 'Plumbing', 'AC Repair'];
  currentBannerIndex = 0;
  isLoggedIn = false;

  categories: ServiceCategory[] = [
    { id: '1', name: 'Cleaning Services', icon: '🧹', route: '/services?category=cleaning' },
    { id: '2', name: 'Plumbing', icon: '🔧', route: '/services?category=plumbing' },
    { id: '3', name: 'Electrical', icon: '⚡', route: '/services?category=electrical' },
    { id: '4', name: 'Carpentry', icon: '🪚', route: '/services?category=carpentry' },
    { id: '5', name: 'Appliance Repair', icon: '🔌', route: '/services?category=appliance' },
    { id: '6', name: 'Painting', icon: '🎨', route: '/services?category=painting' },
    { id: '7', name: 'Pest Control', icon: '🐛', route: '/services?category=pest-control' },
    { id: '8', name: 'AC Maintenance', icon: '❄️', route: '/services?category=ac' },
    { id: '9', name: 'Moving & Packing', icon: '📦', route: '/services?category=moving' },
  ];

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

  featuredServices: FeaturedService[] = [
    {
      id: '1',
      name: 'Deep Home Cleaning',
      imageUrl: 'assets/images/cleaning.jpg',
      price: 299,
      rating: 4.8,
      reviewCount: 1234
    },
    {
      id: '2',
      name: 'AC Installation & Repair',
      imageUrl: 'assets/images/ac-service.jpg',
      price: 199,
      rating: 4.9,
      reviewCount: 892
    },
    {
      id: '3',
      name: 'Plumbing Services',
      imageUrl: 'assets/images/plumbing.jpg',
      price: 149,
      rating: 4.7,
      reviewCount: 654
    },
    {
      id: '4',
      name: 'Electrical Services',
      imageUrl: 'assets/images/electrical.jpg',
      price: 179,
      rating: 4.8,
      reviewCount: 543
    }
  ];

  recentBookings: RecentBooking[] = [];

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isAuthenticated();

    if (this.isLoggedIn) {
      this.loadRecentBookings();
    }

    // Auto-rotate banners every 5 seconds
    setInterval(() => {
      this.nextBanner();
    }, 5000);
  }

  onSearch(): void {
    if (this.searchQuery.trim()) {
      // Add to recent searches
      if (!this.recentSearches.includes(this.searchQuery)) {
        this.recentSearches.unshift(this.searchQuery);
        if (this.recentSearches.length > 5) {
          this.recentSearches.pop();
        }
      }
      // Navigate to search results
      // this.router.navigate(['/services'], { queryParams: { q: this.searchQuery } });
    }
  }

  changeLocation(): void {
    // Open location picker dialog
    console.log('Change location clicked');
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

  loadRecentBookings(): void {
    // Mock data - would normally come from API
    this.recentBookings = [
      {
        id: '1',
        serviceName: 'Home Cleaning',
        providerName: 'Ahmad Khan',
        scheduledDate: new Date('2025-11-15T10:00:00'),
        status: 'Upcoming'
      },
      {
        id: '2',
        serviceName: 'AC Repair',
        providerName: 'Mohamed Ali',
        scheduledDate: new Date('2025-11-05T14:00:00'),
        status: 'Completed'
      }
    ];
  }

  bookAgain(bookingId: string): void {
    console.log('Book again:', bookingId);
    // Navigate to booking flow with pre-filled data
  }

  getStarArray(rating: number): boolean[] {
    return Array(5).fill(false).map((_, i) => i < Math.floor(rating));
  }
}
