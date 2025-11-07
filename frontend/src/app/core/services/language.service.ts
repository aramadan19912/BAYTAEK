import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject } from 'rxjs';

export type SupportedLanguage = 'en' | 'ar';

@Injectable({
  providedIn: 'root'
})
export class LanguageService {
  private currentLanguageSubject = new BehaviorSubject<SupportedLanguage>('en');
  public currentLanguage$ = this.currentLanguageSubject.asObservable();

  constructor(private translate: TranslateService) {
    this.initializeLanguage();
  }

  private initializeLanguage(): void {
    // Set available languages
    this.translate.addLangs(['en', 'ar']);

    // Get language from storage or use browser language
    const savedLanguage = localStorage.getItem('language') as SupportedLanguage;
    const browserLang = this.translate.getBrowserLang();
    const defaultLang = savedLanguage || (browserLang === 'ar' ? 'ar' : 'en');

    this.translate.setDefaultLang('en');
    this.setLanguage(defaultLang);
  }

  setLanguage(lang: SupportedLanguage): void {
    this.translate.use(lang);
    localStorage.setItem('language', lang);
    this.currentLanguageSubject.next(lang);

    // Update HTML direction for RTL support
    const htmlTag = document.documentElement;
    if (lang === 'ar') {
      htmlTag.setAttribute('dir', 'rtl');
      htmlTag.setAttribute('lang', 'ar');
    } else {
      htmlTag.setAttribute('dir', 'ltr');
      htmlTag.setAttribute('lang', 'en');
    }
  }

  getCurrentLanguage(): SupportedLanguage {
    return this.currentLanguageSubject.value;
  }

  isRTL(): boolean {
    return this.currentLanguageSubject.value === 'ar';
  }

  toggleLanguage(): void {
    const newLang: SupportedLanguage = this.getCurrentLanguage() === 'en' ? 'ar' : 'en';
    this.setLanguage(newLang);
  }
}
