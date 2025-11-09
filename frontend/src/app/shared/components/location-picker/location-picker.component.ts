import { Component, OnInit, OnDestroy, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { Subject, takeUntil } from 'rxjs';
import { AddressService, Address, Coordinates } from '../../../core/services/address.service';
import { LanguageService } from '../../../core/services/language.service';

interface LocationOption {
  id: string;
  type: 'saved' | 'city' | 'current';
  label: string;
  sublabel?: string;
  city?: string;
  region?: string;
  coordinates?: Coordinates;
}

interface CityOption {
  name: string;
  nameAr: string;
  region: string;
  regionAr: string;
  coordinates: Coordinates;
}

@Component({
  selector: 'app-location-picker',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './location-picker.component.html',
  styleUrls: ['./location-picker.component.scss']
})
export class LocationPickerComponent implements OnInit, OnDestroy {
  @Output() locationSelected = new EventEmitter<{ city: string; region: string; coordinates?: Coordinates }>();
  @Output() close = new EventEmitter<void>();

  private destroy$ = new Subject<void>();

  isOpen = false;
  searchQuery = '';
  savedAddresses: Address[] = [];
  filteredCities: CityOption[] = [];
  isLoadingCurrentLocation = false;
  currentLanguage = 'en';

  // Popular cities in Saudi Arabia and Egypt
  cities: CityOption[] = [
    // Saudi Arabia
    { name: 'Riyadh', nameAr: 'الرياض', region: 'Riyadh', regionAr: 'الرياض', coordinates: { latitude: 24.7136, longitude: 46.6753 } },
    { name: 'Jeddah', nameAr: 'جدة', region: 'Makkah', regionAr: 'مكة المكرمة', coordinates: { latitude: 21.4858, longitude: 39.1925 } },
    { name: 'Makkah', nameAr: 'مكة', region: 'Makkah', regionAr: 'مكة المكرمة', coordinates: { latitude: 21.3891, longitude: 39.8579 } },
    { name: 'Madinah', nameAr: 'المدينة المنورة', region: 'Madinah', regionAr: 'المدينة المنورة', coordinates: { latitude: 24.5247, longitude: 39.5692 } },
    { name: 'Dammam', nameAr: 'الدمام', region: 'Eastern Province', regionAr: 'المنطقة الشرقية', coordinates: { latitude: 26.4367, longitude: 50.1038 } },
    { name: 'Khobar', nameAr: 'الخبر', region: 'Eastern Province', regionAr: 'المنطقة الشرقية', coordinates: { latitude: 26.2172, longitude: 50.1971 } },
    { name: 'Dhahran', nameAr: 'الظهران', region: 'Eastern Province', regionAr: 'المنطقة الشرقية', coordinates: { latitude: 26.2361, longitude: 50.0393 } },
    { name: 'Tabuk', nameAr: 'تبوك', region: 'Tabuk', regionAr: 'تبوك', coordinates: { latitude: 28.3998, longitude: 36.5782 } },
    { name: 'Abha', nameAr: 'أبها', region: 'Asir', regionAr: 'عسير', coordinates: { latitude: 18.2164, longitude: 42.5053 } },
    { name: 'Buraidah', nameAr: 'بريدة', region: 'Qassim', regionAr: 'القصيم', coordinates: { latitude: 26.3260, longitude: 43.9750 } },
    { name: 'Hail', nameAr: 'حائل', region: "Ha'il", regionAr: 'حائل', coordinates: { latitude: 27.5219, longitude: 41.6901 } },
    { name: 'Yanbu', nameAr: 'ينبع', region: 'Madinah', regionAr: 'المدينة المنورة', coordinates: { latitude: 24.0897, longitude: 38.0618 } },
    { name: 'Jazan', nameAr: 'جازان', region: 'Jazan', regionAr: 'جازان', coordinates: { latitude: 16.8892, longitude: 42.5511 } },
    { name: 'Najran', nameAr: 'نجران', region: 'Najran', regionAr: 'نجران', coordinates: { latitude: 17.4924, longitude: 44.1277 } },

    // Egypt
    { name: 'Cairo', nameAr: 'القاهرة', region: 'Cairo', regionAr: 'القاهرة', coordinates: { latitude: 30.0444, longitude: 31.2357 } },
    { name: 'Alexandria', nameAr: 'الإسكندرية', region: 'Alexandria', regionAr: 'الإسكندرية', coordinates: { latitude: 31.2001, longitude: 29.9187 } },
    { name: 'Giza', nameAr: 'الجيزة', region: 'Giza', regionAr: 'الجيزة', coordinates: { latitude: 30.0131, longitude: 31.2089 } },
    { name: 'Shubra El Kheima', nameAr: 'شبرا الخيمة', region: 'Qalyubia', regionAr: 'القليوبية', coordinates: { latitude: 30.1286, longitude: 31.2422 } },
    { name: 'Port Said', nameAr: 'بورسعيد', region: 'Port Said', regionAr: 'بورسعيد', coordinates: { latitude: 31.2653, longitude: 32.3019 } },
    { name: 'Suez', nameAr: 'السويس', region: 'Suez', regionAr: 'السويس', coordinates: { latitude: 29.9668, longitude: 32.5498 } },
    { name: 'Luxor', nameAr: 'الأقصر', region: 'Luxor', regionAr: 'الأقصر', coordinates: { latitude: 25.6872, longitude: 32.6396 } },
    { name: 'Mansoura', nameAr: 'المنصورة', region: 'Dakahlia', regionAr: 'الدقهلية', coordinates: { latitude: 31.0409, longitude: 31.3785 } },
    { name: 'Tanta', nameAr: 'طنطا', region: 'Gharbia', regionAr: 'الغربية', coordinates: { latitude: 30.7865, longitude: 31.0004 } },
    { name: 'Aswan', nameAr: 'أسوان', region: 'Aswan', regionAr: 'أسوان', coordinates: { latitude: 24.0889, longitude: 32.8998 } }
  ];

  constructor(
    private addressService: AddressService,
    private languageService: LanguageService
  ) {}

  ngOnInit(): void {
    this.currentLanguage = this.languageService.getCurrentLanguage();

    // Subscribe to language changes
    this.languageService.currentLanguage$
      .pipe(takeUntil(this.destroy$))
      .subscribe(lang => {
        this.currentLanguage = lang;
      });

    // Load saved addresses
    this.loadSavedAddresses();

    // Initialize with all cities
    this.filteredCities = [...this.cities];
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  open(): void {
    this.isOpen = true;
  }

  closeModal(): void {
    this.isOpen = false;
    this.close.emit();
  }

  loadSavedAddresses(): void {
    this.addressService.addresses$
      .pipe(takeUntil(this.destroy$))
      .subscribe(addresses => {
        this.savedAddresses = addresses;
      });
  }

  onSearchChange(): void {
    const query = this.searchQuery.toLowerCase().trim();

    if (!query) {
      this.filteredCities = [...this.cities];
      return;
    }

    this.filteredCities = this.cities.filter(city =>
      city.name.toLowerCase().includes(query) ||
      city.nameAr.includes(query) ||
      city.region.toLowerCase().includes(query) ||
      city.regionAr.includes(query)
    );
  }

  selectSavedAddress(address: Address): void {
    this.locationSelected.emit({
      city: address.city,
      region: address.region,
      coordinates: address.latitude && address.longitude
        ? { latitude: address.latitude, longitude: address.longitude }
        : undefined
    });
    this.closeModal();
  }

  selectCity(city: CityOption): void {
    const cityName = this.currentLanguage === 'ar' ? city.nameAr : city.name;
    const regionName = this.currentLanguage === 'ar' ? city.regionAr : city.region;

    this.locationSelected.emit({
      city: cityName,
      region: regionName,
      coordinates: city.coordinates
    });
    this.closeModal();
  }

  async useCurrentLocation(): Promise<void> {
    this.isLoadingCurrentLocation = true;

    try {
      const coordinates = await this.addressService.getCurrentLocation();

      // Reverse geocode to get address details
      this.addressService.reverseGeocode(coordinates.latitude, coordinates.longitude)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (result) => {
            this.locationSelected.emit({
              city: result.city,
              region: result.region,
              coordinates: result.coordinates
            });
            this.closeModal();
            this.isLoadingCurrentLocation = false;
          },
          error: (error) => {
            console.error('Error reverse geocoding:', error);
            this.isLoadingCurrentLocation = false;
            // Still emit the coordinates even if reverse geocode fails
            this.locationSelected.emit({
              city: 'Current Location',
              region: 'Unknown',
              coordinates: coordinates
            });
            this.closeModal();
          }
        });
    } catch (error) {
      console.error('Error getting current location:', error);
      this.isLoadingCurrentLocation = false;
      alert(this.currentLanguage === 'ar'
        ? 'فشل الوصول إلى موقعك الحالي. يرجى التأكد من تمكين خدمات الموقع.'
        : 'Failed to access your current location. Please ensure location services are enabled.');
    }
  }

  getCityDisplayName(city: CityOption): string {
    return this.currentLanguage === 'ar' ? city.nameAr : city.name;
  }

  getRegionDisplayName(city: CityOption): string {
    return this.currentLanguage === 'ar' ? city.regionAr : city.region;
  }

  formatSavedAddress(address: Address): string {
    return this.addressService.formatShortAddress(address);
  }

  trackByCityName(index: number, city: CityOption): string {
    return city.name;
  }

  trackByAddressId(index: number, address: Address): string {
    return address.addressId;
  }
}
