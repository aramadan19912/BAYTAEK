import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from './api.service';

export interface Address {
  addressId: string;
  userId: string;
  label: string;
  addressLine: string;
  region: string;
  city: string;
  postalCode?: string;
  additionalDetails?: string;
  latitude?: number;
  longitude?: number;
  isDefault: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateAddressRequest {
  label: string;
  addressLine: string;
  region: string;
  city: string;
  postalCode?: string;
  additionalDetails?: string;
  latitude?: number;
  longitude?: number;
  isDefault?: boolean;
}

export interface UpdateAddressRequest {
  label?: string;
  addressLine?: string;
  region?: string;
  city?: string;
  postalCode?: string;
  additionalDetails?: string;
  latitude?: number;
  longitude?: number;
  isDefault?: boolean;
}

export interface Coordinates {
  latitude: number;
  longitude: number;
}

export interface GeocodeResult {
  address: string;
  city: string;
  region: string;
  postalCode?: string;
  coordinates: Coordinates;
}

@Injectable({
  providedIn: 'root'
})
export class AddressService {
  private addressesSubject = new BehaviorSubject<Address[]>([]);
  public addresses$ = this.addressesSubject.asObservable();

  private defaultAddressSubject = new BehaviorSubject<Address | null>(null);
  public defaultAddress$ = this.defaultAddressSubject.asObservable();

  constructor(private apiService: ApiService) {
    this.loadAddresses();
  }

  // Load all addresses
  loadAddresses(): void {
    this.getAddresses().subscribe({
      next: (result) => {
        this.addressesSubject.next(result.addresses);
        const defaultAddr = result.addresses.find(a => a.isDefault) || null;
        this.defaultAddressSubject.next(defaultAddr);
      },
      error: (error) => {
        console.error('Error loading addresses:', error);
      }
    });
  }

  // Get all user addresses
  getAddresses(): Observable<{ addresses: Address[] }> {
    return this.apiService.get<{ addresses: Address[] }>('addresses');
  }

  // Get address by ID
  getAddressById(addressId: string): Observable<Address> {
    return this.apiService.get<Address>(`addresses/${addressId}`);
  }

  // Create a new address
  createAddress(request: CreateAddressRequest): Observable<any> {
    return this.apiService.post('addresses', request).pipe(
      tap(() => this.loadAddresses())
    );
  }

  // Update an address
  updateAddress(addressId: string, request: UpdateAddressRequest): Observable<any> {
    return this.apiService.put(`addresses/${addressId}`, request).pipe(
      tap(() => this.loadAddresses())
    );
  }

  // Delete an address
  deleteAddress(addressId: string): Observable<any> {
    return this.apiService.delete(`addresses/${addressId}`).pipe(
      tap(() => this.loadAddresses())
    );
  }

  // Set address as default
  setDefaultAddress(addressId: string): Observable<any> {
    return this.apiService.post(`addresses/${addressId}/set-default`, {}).pipe(
      tap(() => this.loadAddresses())
    );
  }

  // Get default address
  getDefaultAddress(): Address | null {
    return this.defaultAddressSubject.value;
  }

  // Get current addresses
  getCurrentAddresses(): Address[] {
    return this.addressesSubject.value;
  }

  // Geocode address (convert address string to coordinates)
  geocodeAddress(address: string): Observable<GeocodeResult> {
    return this.apiService.post<GeocodeResult>('addresses/geocode', { address });
  }

  // Reverse geocode (convert coordinates to address)
  reverseGeocode(latitude: number, longitude: number): Observable<GeocodeResult> {
    return this.apiService.post<GeocodeResult>('addresses/reverse-geocode', {
      latitude,
      longitude
    });
  }

  // Get current location
  getCurrentLocation(): Promise<Coordinates> {
    return new Promise((resolve, reject) => {
      if (!navigator.geolocation) {
        reject(new Error('Geolocation is not supported by this browser'));
        return;
      }

      navigator.geolocation.getCurrentPosition(
        (position) => {
          resolve({
            latitude: position.coords.latitude,
            longitude: position.coords.longitude
          });
        },
        (error) => {
          reject(error);
        },
        {
          enableHighAccuracy: true,
          timeout: 5000,
          maximumAge: 0
        }
      );
    });
  }

  // Calculate distance between two coordinates (in kilometers)
  calculateDistance(
    coord1: Coordinates,
    coord2: Coordinates
  ): number {
    const R = 6371; // Earth's radius in kilometers
    const dLat = this.toRad(coord2.latitude - coord1.latitude);
    const dLon = this.toRad(coord2.longitude - coord1.longitude);

    const a =
      Math.sin(dLat / 2) * Math.sin(dLat / 2) +
      Math.cos(this.toRad(coord1.latitude)) *
        Math.cos(this.toRad(coord2.latitude)) *
        Math.sin(dLon / 2) *
        Math.sin(dLon / 2);

    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    const distance = R * c;

    return Math.round(distance * 100) / 100; // Round to 2 decimal places
  }

  private toRad(degrees: number): number {
    return degrees * (Math.PI / 180);
  }

  // Format address for display
  formatAddress(address: Address): string {
    const parts = [
      address.addressLine,
      address.city,
      address.region
    ];

    if (address.postalCode) {
      parts.push(address.postalCode);
    }

    return parts.filter(p => p).join(', ');
  }

  // Format short address (for compact display)
  formatShortAddress(address: Address): string {
    return `${address.addressLine}, ${address.city}`;
  }

  // Get address label icon
  getAddressLabelIcon(label: string): string {
    const labelMap: Record<string, string> = {
      'Home': 'home',
      'Work': 'business',
      'Other': 'location_on'
    };
    return labelMap[label] || 'location_on';
  }

  // Get available address labels
  getAddressLabels(): { value: string; label: string; icon: string }[] {
    return [
      { value: 'Home', label: 'Home', icon: 'home' },
      { value: 'Work', label: 'Work', icon: 'business' },
      { value: 'Other', label: 'Other', icon: 'location_on' }
    ];
  }

  // Get regions list (Saudi Arabia regions)
  getRegions(): { value: string; label: string }[] {
    return [
      { value: 'Riyadh', label: 'Riyadh' },
      { value: 'Makkah', label: 'Makkah' },
      { value: 'Madinah', label: 'Madinah' },
      { value: 'Eastern Province', label: 'Eastern Province' },
      { value: 'Asir', label: 'Asir' },
      { value: 'Tabuk', label: 'Tabuk' },
      { value: 'Qassim', label: 'Qassim' },
      { value: 'Ha\'il', label: 'Ha\'il' },
      { value: 'Northern Borders', label: 'Northern Borders' },
      { value: 'Jazan', label: 'Jazan' },
      { value: 'Najran', label: 'Najran' },
      { value: 'Al Bahah', label: 'Al Bahah' },
      { value: 'Al Jawf', label: 'Al Jawf' }
    ];
  }

  // Validate address completeness
  isAddressComplete(address: Partial<CreateAddressRequest>): boolean {
    return !!(
      address.label &&
      address.addressLine &&
      address.region &&
      address.city
    );
  }

  // Check if coordinates are valid
  areCoordinatesValid(coordinates?: Coordinates): boolean {
    if (!coordinates) return false;

    return (
      coordinates.latitude >= -90 &&
      coordinates.latitude <= 90 &&
      coordinates.longitude >= -180 &&
      coordinates.longitude <= 180
    );
  }
}
