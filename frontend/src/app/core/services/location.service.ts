import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Coordinates } from './address.service';

export interface UserLocation {
  city: string;
  region: string;
  coordinates?: Coordinates;
}

const DEFAULT_LOCATION: UserLocation = {
  city: 'Riyadh',
  region: 'Saudi Arabia'
};

const LOCATION_STORAGE_KEY = 'userLocation';

@Injectable({
  providedIn: 'root'
})
export class LocationService {
  private currentLocationSubject: BehaviorSubject<UserLocation>;
  public currentLocation$: Observable<UserLocation>;

  constructor() {
    // Load location from localStorage or use default
    const savedLocation = this.loadLocationFromStorage();
    this.currentLocationSubject = new BehaviorSubject<UserLocation>(savedLocation);
    this.currentLocation$ = this.currentLocationSubject.asObservable();
  }

  /**
   * Get the current location value
   */
  getCurrentLocation(): UserLocation {
    return this.currentLocationSubject.value;
  }

  /**
   * Update the current location
   */
  setLocation(location: UserLocation): void {
    this.currentLocationSubject.next(location);
    this.saveLocationToStorage(location);
  }

  /**
   * Update only the city
   */
  setCity(city: string): void {
    const current = this.getCurrentLocation();
    this.setLocation({ ...current, city });
  }

  /**
   * Update only the region
   */
  setRegion(region: string): void {
    const current = this.getCurrentLocation();
    this.setLocation({ ...current, region });
  }

  /**
   * Update only the coordinates
   */
  setCoordinates(coordinates: Coordinates): void {
    const current = this.getCurrentLocation();
    this.setLocation({ ...current, coordinates });
  }

  /**
   * Get the location display string
   */
  getLocationDisplayString(): string {
    const location = this.getCurrentLocation();
    return `${location.city}, ${location.region}`;
  }

  /**
   * Reset to default location
   */
  resetToDefault(): void {
    this.setLocation(DEFAULT_LOCATION);
  }

  /**
   * Clear location from storage
   */
  clearLocation(): void {
    localStorage.removeItem(LOCATION_STORAGE_KEY);
    this.resetToDefault();
  }

  /**
   * Check if coordinates are available
   */
  hasCoordinates(): boolean {
    const location = this.getCurrentLocation();
    return !!(location.coordinates?.latitude && location.coordinates?.longitude);
  }

  /**
   * Get coordinates if available
   */
  getCoordinates(): Coordinates | undefined {
    return this.getCurrentLocation().coordinates;
  }

  /**
   * Load location from localStorage
   */
  private loadLocationFromStorage(): UserLocation {
    try {
      const saved = localStorage.getItem(LOCATION_STORAGE_KEY);
      if (saved) {
        const parsed = JSON.parse(saved);
        // Validate the parsed data
        if (this.isValidLocation(parsed)) {
          return parsed;
        }
      }
    } catch (error) {
      console.error('Error loading location from storage:', error);
    }
    return DEFAULT_LOCATION;
  }

  /**
   * Save location to localStorage
   */
  private saveLocationToStorage(location: UserLocation): void {
    try {
      localStorage.setItem(LOCATION_STORAGE_KEY, JSON.stringify(location));
    } catch (error) {
      console.error('Error saving location to storage:', error);
    }
  }

  /**
   * Validate location object
   */
  private isValidLocation(location: any): location is UserLocation {
    return (
      location &&
      typeof location === 'object' &&
      typeof location.city === 'string' &&
      typeof location.region === 'string' &&
      location.city.length > 0 &&
      location.region.length > 0
    );
  }

  /**
   * Check if a location is within a certain distance (in km) from the current location
   * Requires both locations to have coordinates
   */
  isWithinDistance(coordinates: Coordinates, maxDistanceKm: number): boolean {
    const currentCoordinates = this.getCoordinates();
    if (!currentCoordinates) {
      return false;
    }

    const distance = this.calculateDistance(currentCoordinates, coordinates);
    return distance <= maxDistanceKm;
  }

  /**
   * Calculate distance between two coordinates in kilometers
   */
  private calculateDistance(coord1: Coordinates, coord2: Coordinates): number {
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

  /**
   * Convert degrees to radians
   */
  private toRad(degrees: number): number {
    return degrees * (Math.PI / 180);
  }
}
