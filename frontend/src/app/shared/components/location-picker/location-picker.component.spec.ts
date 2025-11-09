import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { of, throwError } from 'rxjs';
import { LocationPickerComponent } from './location-picker.component';
import { AddressService, Address, Coordinates, GeocodeResult } from '../../../core/services/address.service';
import { LanguageService } from '../../../core/services/language.service';

describe('LocationPickerComponent', () => {
  let component: LocationPickerComponent;
  let fixture: ComponentFixture<LocationPickerComponent>;
  let mockAddressService: jasmine.SpyObj<AddressService>;
  let mockLanguageService: jasmine.SpyObj<LanguageService>;

  const mockAddresses: Address[] = [
    {
      addressId: '1',
      userId: 'user1',
      label: 'Home',
      addressLine: '123 Main St',
      city: 'Riyadh',
      region: 'Riyadh',
      isDefault: true,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      latitude: 24.7136,
      longitude: 46.6753
    },
    {
      addressId: '2',
      userId: 'user1',
      label: 'Work',
      addressLine: '456 Business Ave',
      city: 'Jeddah',
      region: 'Makkah',
      isDefault: false,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    }
  ];

  beforeEach(async () => {
    mockAddressService = jasmine.createSpyObj('AddressService', [
      'getCurrentLocation',
      'reverseGeocode',
      'formatShortAddress'
    ], {
      addresses$: of(mockAddresses)
    });

    mockLanguageService = jasmine.createSpyObj('LanguageService', ['getCurrentLanguage'], {
      currentLanguage$: of('en')
    });

    mockLanguageService.getCurrentLanguage.and.returnValue('en');
    mockAddressService.formatShortAddress.and.callFake((addr: Address) => `${addr.addressLine}, ${addr.city}`);

    await TestBed.configureTestingModule({
      imports: [
        LocationPickerComponent,
        FormsModule,
        TranslateModule.forRoot()
      ],
      providers: [
        { provide: AddressService, useValue: mockAddressService },
        { provide: LanguageService, useValue: mockLanguageService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LocationPickerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with modal closed', () => {
      expect(component.isOpen).toBe(false);
    });

    it('should load saved addresses on init', () => {
      expect(component.savedAddresses.length).toBe(2);
      expect(component.savedAddresses[0].label).toBe('Home');
      expect(component.savedAddresses[1].label).toBe('Work');
    });

    it('should initialize filtered cities with all cities', () => {
      expect(component.filteredCities.length).toBe(component.cities.length);
    });

    it('should set current language', () => {
      expect(component.currentLanguage).toBe('en');
    });

    it('should have cities data for Saudi Arabia and Egypt', () => {
      const saudiCities = component.cities.filter(c => c.region.includes('Riyadh') || c.region.includes('Makkah'));
      const egyptCities = component.cities.filter(c => c.region.includes('Cairo') || c.region.includes('Alexandria'));

      expect(saudiCities.length).toBeGreaterThan(0);
      expect(egyptCities.length).toBeGreaterThan(0);
    });
  });

  describe('Modal Controls', () => {
    it('should open modal', () => {
      component.open();

      expect(component.isOpen).toBe(true);
    });

    it('should close modal', () => {
      component.open();
      component.closeModal();

      expect(component.isOpen).toBe(false);
    });

    it('should emit close event when closing', () => {
      spyOn(component.close, 'emit');

      component.closeModal();

      expect(component.close.emit).toHaveBeenCalled();
    });
  });

  describe('Search Functionality', () => {
    it('should filter cities by English name', () => {
      component.searchQuery = 'riyadh';
      component.onSearchChange();

      expect(component.filteredCities.length).toBeGreaterThan(0);
      expect(component.filteredCities.every(c => c.name.toLowerCase().includes('riyadh'))).toBe(true);
    });

    it('should filter cities by Arabic name', () => {
      component.searchQuery = 'الرياض';
      component.onSearchChange();

      expect(component.filteredCities.length).toBeGreaterThan(0);
      expect(component.filteredCities.some(c => c.nameAr.includes('الرياض'))).toBe(true);
    });

    it('should filter cities by region', () => {
      component.searchQuery = 'makkah';
      component.onSearchChange();

      expect(component.filteredCities.length).toBeGreaterThan(0);
    });

    it('should reset to all cities when search is empty', () => {
      component.searchQuery = 'riyadh';
      component.onSearchChange();

      component.searchQuery = '';
      component.onSearchChange();

      expect(component.filteredCities.length).toBe(component.cities.length);
    });

    it('should handle case-insensitive search', () => {
      component.searchQuery = 'RIYADH';
      component.onSearchChange();

      expect(component.filteredCities.length).toBeGreaterThan(0);
    });

    it('should return empty array for non-matching search', () => {
      component.searchQuery = 'xyz123nonexistent';
      component.onSearchChange();

      expect(component.filteredCities.length).toBe(0);
    });
  });

  describe('Location Selection', () => {
    it('should emit location when saved address is selected', () => {
      spyOn(component.locationSelected, 'emit');
      const address = mockAddresses[0];

      component.selectSavedAddress(address);

      expect(component.locationSelected.emit).toHaveBeenCalledWith({
        city: 'Riyadh',
        region: 'Riyadh',
        coordinates: { latitude: 24.7136, longitude: 46.6753 }
      });
    });

    it('should close modal after selecting saved address', () => {
      component.open();
      component.selectSavedAddress(mockAddresses[0]);

      expect(component.isOpen).toBe(false);
    });

    it('should emit location when city is selected', () => {
      spyOn(component.locationSelected, 'emit');
      const city = component.cities[0]; // Riyadh

      component.selectCity(city);

      expect(component.locationSelected.emit).toHaveBeenCalledWith(jasmine.objectContaining({
        coordinates: city.coordinates
      }));
    });

    it('should close modal after selecting city', () => {
      component.open();
      component.selectCity(component.cities[0]);

      expect(component.isOpen).toBe(false);
    });

    it('should use Arabic name when language is Arabic', () => {
      spyOn(component.locationSelected, 'emit');
      component.currentLanguage = 'ar';
      const city = component.cities[0]; // Riyadh

      component.selectCity(city);

      const emittedCity = (component.locationSelected.emit as jasmine.Spy).calls.argsFor(0)[0].city;
      expect(emittedCity).toBe(city.nameAr);
    });
  });

  describe('Current Location', () => {
    it('should get current location successfully', async () => {
      const mockCoords: Coordinates = { latitude: 24.7136, longitude: 46.6753 };
      const mockGeocodeResult: GeocodeResult = {
        address: '123 Main St',
        city: 'Riyadh',
        region: 'Riyadh Region',
        coordinates: mockCoords
      };

      mockAddressService.getCurrentLocation.and.returnValue(Promise.resolve(mockCoords));
      mockAddressService.reverseGeocode.and.returnValue(of(mockGeocodeResult));
      spyOn(component.locationSelected, 'emit');

      await component.useCurrentLocation();

      expect(component.locationSelected.emit).toHaveBeenCalledWith({
        city: 'Riyadh',
        region: 'Riyadh Region',
        coordinates: mockCoords
      });
    });

    it('should show loading state while getting location', async () => {
      const mockCoords: Coordinates = { latitude: 24.7136, longitude: 46.6753 };
      mockAddressService.getCurrentLocation.and.returnValue(
        new Promise(resolve => setTimeout(() => resolve(mockCoords), 100))
      );

      const promise = component.useCurrentLocation();
      expect(component.isLoadingCurrentLocation).toBe(true);

      await promise;
    });

    it('should handle geolocation error', async () => {
      mockAddressService.getCurrentLocation.and.returnValue(
        Promise.reject(new Error('Geolocation not available'))
      );
      spyOn(window, 'alert');

      await component.useCurrentLocation();

      expect(component.isLoadingCurrentLocation).toBe(false);
      expect(window.alert).toHaveBeenCalled();
    });

    it('should handle reverse geocoding error gracefully', async () => {
      const mockCoords: Coordinates = { latitude: 24.7136, longitude: 46.6753 };
      mockAddressService.getCurrentLocation.and.returnValue(Promise.resolve(mockCoords));
      mockAddressService.reverseGeocode.and.returnValue(throwError(() => new Error('Reverse geocode failed')));
      spyOn(component.locationSelected, 'emit');

      await component.useCurrentLocation();

      // Should still emit coordinates even if reverse geocode fails
      expect(component.locationSelected.emit).toHaveBeenCalledWith({
        city: 'Current Location',
        region: 'Unknown',
        coordinates: mockCoords
      });
    });

    it('should close modal after getting current location', async () => {
      const mockCoords: Coordinates = { latitude: 24.7136, longitude: 46.6753 };
      const mockGeocodeResult: GeocodeResult = {
        address: '123 Main St',
        city: 'Riyadh',
        region: 'Riyadh Region',
        coordinates: mockCoords
      };

      mockAddressService.getCurrentLocation.and.returnValue(Promise.resolve(mockCoords));
      mockAddressService.reverseGeocode.and.returnValue(of(mockGeocodeResult));
      component.open();

      await component.useCurrentLocation();

      expect(component.isOpen).toBe(false);
    });
  });

  describe('Display Methods', () => {
    it('should return English city name when language is English', () => {
      component.currentLanguage = 'en';
      const city = component.cities[0];

      const displayName = component.getCityDisplayName(city);

      expect(displayName).toBe(city.name);
    });

    it('should return Arabic city name when language is Arabic', () => {
      component.currentLanguage = 'ar';
      const city = component.cities[0];

      const displayName = component.getCityDisplayName(city);

      expect(displayName).toBe(city.nameAr);
    });

    it('should return English region name when language is English', () => {
      component.currentLanguage = 'en';
      const city = component.cities[0];

      const displayName = component.getRegionDisplayName(city);

      expect(displayName).toBe(city.region);
    });

    it('should return Arabic region name when language is Arabic', () => {
      component.currentLanguage = 'ar';
      const city = component.cities[0];

      const displayName = component.getRegionDisplayName(city);

      expect(displayName).toBe(city.regionAr);
    });

    it('should format saved address correctly', () => {
      const address = mockAddresses[0];

      const formatted = component.formatSavedAddress(address);

      expect(formatted).toBe('123 Main St, Riyadh');
    });
  });

  describe('TrackBy Functions', () => {
    it('should track cities by name', () => {
      const city = component.cities[0];

      const trackValue = component.trackByCityName(0, city);

      expect(trackValue).toBe(city.name);
    });

    it('should track addresses by ID', () => {
      const address = mockAddresses[0];

      const trackValue = component.trackByAddressId(0, address);

      expect(trackValue).toBe(address.addressId);
    });
  });

  describe('Language Changes', () => {
    it('should update when language changes', () => {
      mockLanguageService.currentLanguage$ = of('ar');

      // Create new component instance
      const newFixture = TestBed.createComponent(LocationPickerComponent);
      newFixture.detectChanges();

      newFixture.componentInstance.ngOnInit();

      expect(newFixture.componentInstance.currentLanguage).toBe('ar');
    });
  });
});
