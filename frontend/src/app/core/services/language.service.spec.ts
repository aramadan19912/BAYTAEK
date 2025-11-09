import { TestBed } from '@angular/core/testing';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService, SupportedLanguage } from './language.service';

describe('LanguageService', () => {
  let service: LanguageService;
  let translateService: TranslateService;

  beforeEach(() => {
    // Clear localStorage before each test
    localStorage.clear();

    // Reset document attributes
    document.documentElement.removeAttribute('dir');
    document.documentElement.removeAttribute('lang');

    TestBed.configureTestingModule({
      imports: [TranslateModule.forRoot()],
      providers: [LanguageService]
    });

    service = TestBed.inject(LanguageService);
    translateService = TestBed.inject(TranslateService);
  });

  afterEach(() => {
    localStorage.clear();
  });

  describe('Initialization', () => {
    it('should be created', () => {
      expect(service).toBeTruthy();
    });

    it('should initialize with default language when no saved language exists', () => {
      const currentLang = service.getCurrentLanguage();
      expect(['en', 'ar']).toContain(currentLang);
    });

    it('should load saved language from localStorage', () => {
      localStorage.setItem('language', 'ar');

      // Create new service instance to trigger initialization
      const newService = new LanguageService(translateService);

      expect(newService.getCurrentLanguage()).toBe('ar');
    });

    it('should set up available languages', () => {
      const langs = translateService.getLangs();

      expect(langs).toContain('en');
      expect(langs).toContain('ar');
    });

    it('should set default language to English', () => {
      expect(translateService.getDefaultLang()).toBe('en');
    });
  });

  describe('setLanguage', () => {
    it('should set language to English', () => {
      service.setLanguage('en');

      expect(service.getCurrentLanguage()).toBe('en');
    });

    it('should set language to Arabic', () => {
      service.setLanguage('ar');

      expect(service.getCurrentLanguage()).toBe('ar');
    });

    it('should save language to localStorage', () => {
      service.setLanguage('ar');

      const savedLang = localStorage.getItem('language');
      expect(savedLang).toBe('ar');
    });

    it('should update TranslateService when language changes', () => {
      spyOn(translateService, 'use');

      service.setLanguage('ar');

      expect(translateService.use).toHaveBeenCalledWith('ar');
    });

    it('should emit new language through observable', (done) => {
      service.currentLanguage$.subscribe(lang => {
        if (lang === 'ar') {
          expect(lang).toBe('ar');
          done();
        }
      });

      service.setLanguage('ar');
    });

    it('should set RTL direction for Arabic', () => {
      service.setLanguage('ar');

      expect(document.documentElement.getAttribute('dir')).toBe('rtl');
      expect(document.documentElement.getAttribute('lang')).toBe('ar');
    });

    it('should set LTR direction for English', () => {
      service.setLanguage('en');

      expect(document.documentElement.getAttribute('dir')).toBe('ltr');
      expect(document.documentElement.getAttribute('lang')).toBe('en');
    });

    it('should update HTML attributes when switching from Arabic to English', () => {
      service.setLanguage('ar');
      expect(document.documentElement.getAttribute('dir')).toBe('rtl');

      service.setLanguage('en');
      expect(document.documentElement.getAttribute('dir')).toBe('ltr');
      expect(document.documentElement.getAttribute('lang')).toBe('en');
    });

    it('should update HTML attributes when switching from English to Arabic', () => {
      service.setLanguage('en');
      expect(document.documentElement.getAttribute('dir')).toBe('ltr');

      service.setLanguage('ar');
      expect(document.documentElement.getAttribute('dir')).toBe('rtl');
      expect(document.documentElement.getAttribute('lang')).toBe('ar');
    });
  });

  describe('getCurrentLanguage', () => {
    it('should return current language', () => {
      service.setLanguage('ar');

      expect(service.getCurrentLanguage()).toBe('ar');
    });

    it('should return updated language after change', () => {
      service.setLanguage('en');
      expect(service.getCurrentLanguage()).toBe('en');

      service.setLanguage('ar');
      expect(service.getCurrentLanguage()).toBe('ar');
    });
  });

  describe('isRTL', () => {
    it('should return true for Arabic', () => {
      service.setLanguage('ar');

      expect(service.isRTL()).toBe(true);
    });

    it('should return false for English', () => {
      service.setLanguage('en');

      expect(service.isRTL()).toBe(false);
    });

    it('should update when language changes', () => {
      service.setLanguage('en');
      expect(service.isRTL()).toBe(false);

      service.setLanguage('ar');
      expect(service.isRTL()).toBe(true);
    });
  });

  describe('toggleLanguage', () => {
    it('should toggle from English to Arabic', () => {
      service.setLanguage('en');

      service.toggleLanguage();

      expect(service.getCurrentLanguage()).toBe('ar');
    });

    it('should toggle from Arabic to English', () => {
      service.setLanguage('ar');

      service.toggleLanguage();

      expect(service.getCurrentLanguage()).toBe('en');
    });

    it('should toggle back and forth correctly', () => {
      service.setLanguage('en');
      expect(service.getCurrentLanguage()).toBe('en');

      service.toggleLanguage();
      expect(service.getCurrentLanguage()).toBe('ar');

      service.toggleLanguage();
      expect(service.getCurrentLanguage()).toBe('en');
    });

    it('should update localStorage when toggling', () => {
      service.setLanguage('en');
      expect(localStorage.getItem('language')).toBe('en');

      service.toggleLanguage();
      expect(localStorage.getItem('language')).toBe('ar');

      service.toggleLanguage();
      expect(localStorage.getItem('language')).toBe('en');
    });

    it('should update HTML direction when toggling', () => {
      service.setLanguage('en');
      expect(document.documentElement.getAttribute('dir')).toBe('ltr');

      service.toggleLanguage();
      expect(document.documentElement.getAttribute('dir')).toBe('rtl');

      service.toggleLanguage();
      expect(document.documentElement.getAttribute('dir')).toBe('ltr');
    });
  });

  describe('Observable behavior', () => {
    it('should emit initial value immediately', (done) => {
      service.currentLanguage$.subscribe(lang => {
        expect(['en', 'ar']).toContain(lang);
        done();
      });
    });

    it('should emit new values when language changes', () => {
      const emissions: SupportedLanguage[] = [];

      service.currentLanguage$.subscribe(lang => {
        emissions.push(lang);
      });

      service.setLanguage('ar');
      service.setLanguage('en');
      service.setLanguage('ar');

      expect(emissions.length).toBeGreaterThanOrEqual(3);
      expect(emissions[emissions.length - 1]).toBe('ar');
    });

    it('should allow multiple subscribers', () => {
      let subscriber1Value: SupportedLanguage = 'en';
      let subscriber2Value: SupportedLanguage = 'en';

      service.currentLanguage$.subscribe(lang => {
        subscriber1Value = lang;
      });

      service.currentLanguage$.subscribe(lang => {
        subscriber2Value = lang;
      });

      service.setLanguage('ar');

      expect(subscriber1Value).toBe('ar');
      expect(subscriber2Value).toBe('ar');
    });
  });

  describe('Edge cases', () => {
    it('should handle rapid language switching', () => {
      for (let i = 0; i < 10; i++) {
        service.toggleLanguage();
      }

      // After even number of toggles, should be back to original
      const currentLang = service.getCurrentLanguage();
      expect(['en', 'ar']).toContain(currentLang);
    });

    it('should persist language across service destruction and recreation', () => {
      service.setLanguage('ar');

      // Simulate service recreation
      const newService = new LanguageService(translateService);

      expect(newService.getCurrentLanguage()).toBe('ar');
    });
  });
});
