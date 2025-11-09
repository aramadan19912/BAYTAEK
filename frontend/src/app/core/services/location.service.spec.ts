import { TestBed } from '@angular/core/testing';
import { LocationService, UserLocation } from './location.service';
import { Coordinates } from './address.service';

describe('LocationService', () => {
  let service: LocationService;
  const STORAGE_KEY = 'userLocation';

  beforeEach(() => {
    // Clear localStorage before each test
    localStorage.clear();

    TestBed.configureTestingModule({});
    service = TestBed.inject(LocationService);
  });

  afterEach(() => {
    localStorage.clear();
  });

  describe('Initialization', () => {
    it('should be created', () => {
      expect(service).toBeTruthy();
    });

    it('should initialize with default location when no saved location exists', () => {
      const location = service.getCurrentLocation();

      expect(location.city).toBe('Riyadh');
      expect(location.region).toBe('Saudi Arabia');
    });

    it('should load saved location from localStorage', () => {
      const savedLocation: UserLocation = {
        city: 'Cairo',
        region: 'Egypt',
        coordinates: { latitude: 30.0444, longitude: 31.2357 }
      };
      localStorage.setItem(STORAGE_KEY, JSON.stringify(savedLocation));

      // Create new service instance to trigger constructor
      const newService = new LocationService();
      const location = newService.getCurrentLocation();

      expect(location.city).toBe('Cairo');
      expect(location.region).toBe('Egypt');
      expect(location.coordinates).toEqual(savedLocation.coordinates);
    });
  });

  describe('setLocation', () => {
    it('should update the current location', () => {
      const newLocation: UserLocation = {
        city: 'Jeddah',
        region: 'Makkah'
      };

      service.setLocation(newLocation);
      const location = service.getCurrentLocation();

      expect(location.city).toBe('Jeddah');
      expect(location.region).toBe('Makkah');
    });

    it('should save location to localStorage', () => {
      const newLocation: UserLocation = {
        city: 'Alexandria',
        region: 'Egypt'
      };

      service.setLocation(newLocation);
      const saved = localStorage.getItem(STORAGE_KEY);

      expect(saved).not.toBeNull();
      expect(JSON.parse(saved!)).toEqual(newLocation);
    });

    it('should emit new location through observable', (done) => {
      const newLocation: UserLocation = {
        city: 'Dammam',
        region: 'Eastern Province'
      };

      service.currentLocation$.subscribe(location => {
        if (location.city === 'Dammam') {
          expect(location.city).toBe('Dammam');
          expect(location.region).toBe('Eastern Province');
          done();
        }
      });

      service.setLocation(newLocation);
    });
  });

  describe('setCity', () => {
    it('should update only the city', () => {
      const initialLocation: UserLocation = {
        city: 'Riyadh',
        region: 'Riyadh Region'
      };
      service.setLocation(initialLocation);

      service.setCity('Jeddah');
      const location = service.getCurrentLocation();

      expect(location.city).toBe('Jeddah');
      expect(location.region).toBe('Riyadh Region'); // Region unchanged
    });
  });

  describe('setRegion', () => {
    it('should update only the region', () => {
      const initialLocation: UserLocation = {
        city: 'Cairo',
        region: 'Cairo Governorate'
      };
      service.setLocation(initialLocation);

      service.setRegion('Giza');
      const location = service.getCurrentLocation();

      expect(location.city).toBe('Cairo'); // City unchanged
      expect(location.region).toBe('Giza');
    });
  });

  describe('setCoordinates', () => {
    it('should update only the coordinates', () => {
      const initialLocation: UserLocation = {
        city: 'Riyadh',
        region: 'Riyadh Region'
      };
      service.setLocation(initialLocation);

      const newCoordinates: Coordinates = {
        latitude: 24.7136,
        longitude: 46.6753
      };

      service.setCoordinates(newCoordinates);
      const location = service.getCurrentLocation();

      expect(location.city).toBe('Riyadh');
      expect(location.region).toBe('Riyadh Region');
      expect(location.coordinates).toEqual(newCoordinates);
    });
  });

  describe('getLocationDisplayString', () => {
    it('should return formatted location string', () => {
      const location: UserLocation = {
        city: 'Riyadh',
        region: 'Saudi Arabia'
      };
      service.setLocation(location);

      const displayString = service.getLocationDisplayString();

      expect(displayString).toBe('Riyadh, Saudi Arabia');
    });
  });

  describe('resetToDefault', () => {
    it('should reset to default location', () => {
      service.setLocation({
        city: 'Cairo',
        region: 'Egypt'
      });

      service.resetToDefault();
      const location = service.getCurrentLocation();

      expect(location.city).toBe('Riyadh');
      expect(location.region).toBe('Saudi Arabia');
    });
  });

  describe('clearLocation', () => {
    it('should clear location from localStorage', () => {
      service.setLocation({
        city: 'Cairo',
        region: 'Egypt'
      });

      expect(localStorage.getItem(STORAGE_KEY)).not.toBeNull();

      service.clearLocation();

      expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
    });

    it('should reset to default location', () => {
      service.setLocation({
        city: 'Cairo',
        region: 'Egypt'
      });

      service.clearLocation();
      const location = service.getCurrentLocation();

      expect(location.city).toBe('Riyadh');
      expect(location.region).toBe('Saudi Arabia');
    });
  });

  describe('hasCoordinates', () => {
    it('should return true when coordinates exist', () => {
      service.setLocation({
        city: 'Riyadh',
        region: 'Saudi Arabia',
        coordinates: { latitude: 24.7136, longitude: 46.6753 }
      });

      expect(service.hasCoordinates()).toBe(true);
    });

    it('should return false when coordinates do not exist', () => {
      service.setLocation({
        city: 'Riyadh',
        region: 'Saudi Arabia'
      });

      expect(service.hasCoordinates()).toBe(false);
    });
  });

  describe('getCoordinates', () => {
    it('should return coordinates when they exist', () => {
      const coordinates: Coordinates = {
        latitude: 24.7136,
        longitude: 46.6753
      };
      service.setLocation({
        city: 'Riyadh',
        region: 'Saudi Arabia',
        coordinates
      });

      expect(service.getCoordinates()).toEqual(coordinates);
    });

    it('should return undefined when coordinates do not exist', () => {
      service.setLocation({
        city: 'Riyadh',
        region: 'Saudi Arabia'
      });

      expect(service.getCoordinates()).toBeUndefined();
    });
  });

  describe('calculateDistance', () => {
    it('should calculate distance between two coordinates', () => {
      // Riyadh to Jeddah is approximately 870 km
      const riyadh: Coordinates = { latitude: 24.7136, longitude: 46.6753 };
      const jeddah: Coordinates = { latitude: 21.4858, longitude: 39.1925 };

      service.setLocation({
        city: 'Riyadh',
        region: 'Saudi Arabia',
        coordinates: riyadh
      });

      const distance = (service as any).calculateDistance(riyadh, jeddah);

      expect(distance).toBeGreaterThan(850);
      expect(distance).toBeLessThan(900);
    });

    it('should return 0 for same coordinates', () => {
      const riyadh: Coordinates = { latitude: 24.7136, longitude: 46.6753 };

      const distance = (service as any).calculateDistance(riyadh, riyadh);

      expect(distance).toBe(0);
    });
  });

  describe('isWithinDistance', () => {
    it('should return true when location is within distance', () => {
      const riyadh: Coordinates = { latitude: 24.7136, longitude: 46.6753 };
      const nearby: Coordinates = { latitude: 24.7137, longitude: 46.6754 };

      service.setLocation({
        city: 'Riyadh',
        region: 'Saudi Arabia',
        coordinates: riyadh
      });

      expect(service.isWithinDistance(nearby, 1)).toBe(true);
    });

    it('should return false when location is beyond distance', () => {
      const riyadh: Coordinates = { latitude: 24.7136, longitude: 46.6753 };
      const jeddah: Coordinates = { latitude: 21.4858, longitude: 39.1925 };

      service.setLocation({
        city: 'Riyadh',
        region: 'Saudi Arabia',
        coordinates: riyadh
      });

      expect(service.isWithinDistance(jeddah, 100)).toBe(false);
    });

    it('should return false when current location has no coordinates', () => {
      service.setLocation({
        city: 'Riyadh',
        region: 'Saudi Arabia'
      });

      const testCoords: Coordinates = { latitude: 24.7136, longitude: 46.6753 };

      expect(service.isWithinDistance(testCoords, 100)).toBe(false);
    });
  });

  describe('Storage Errors', () => {
    it('should handle invalid JSON in localStorage gracefully', () => {
      localStorage.setItem(STORAGE_KEY, 'invalid json');

      const newService = new LocationService();
      const location = newService.getCurrentLocation();

      expect(location.city).toBe('Riyadh');
      expect(location.region).toBe('Saudi Arabia');
    });

    it('should handle invalid location data structure', () => {
      localStorage.setItem(STORAGE_KEY, JSON.stringify({ invalid: 'data' }));

      const newService = new LocationService();
      const location = newService.getCurrentLocation();

      expect(location.city).toBe('Riyadh');
      expect(location.region).toBe('Saudi Arabia');
    });
  });
});
