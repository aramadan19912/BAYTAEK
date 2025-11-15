import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface ContentItem {
  contentId: string;
  type: string;
  slug: string;
  titleEn: string;
  titleAr: string;
  bodyEn: string;
  bodyAr: string;
  metaTitleEn?: string;
  metaTitleAr?: string;
  metaDescriptionEn?: string;
  metaDescriptionAr?: string;
  isPublished: boolean;
  order: number;
  category?: string;
  viewCount: number;
  lastPublishedAt?: string;
  createdAt: string;
  updatedAt: string;
}

export interface FAQItem {
  contentId: string;
  questionEn: string;
  questionAr: string;
  answerEn: string;
  answerAr: string;
  category?: string;
  order: number;
  viewCount: number;
}

export interface FAQCategory {
  category: string;
  faqs: FAQItem[];
}

export enum ContentType {
  FAQ = 'FAQ',
  TermsAndConditions = 'TermsAndConditions',
  PrivacyPolicy = 'PrivacyPolicy',
  AboutUs = 'AboutUs',
  HelpArticle = 'HelpArticle',
  ContactInfo = 'ContactInfo',
  ServiceAgreement = 'ServiceAgreement',
  RefundPolicy = 'RefundPolicy',
  CancellationPolicy = 'CancellationPolicy'
}

@Injectable({
  providedIn: 'root'
})
export class ContentService {
  constructor(private apiService: ApiService) {}

  // Get content by type
  getContentByType(type: string, language?: string): Observable<ContentItem[]> {
    return this.apiService.get<ContentItem[]>(`content/type/${type}`, { language });
  }

  // Get content by slug
  getContentBySlug(slug: string, language?: string): Observable<ContentItem> {
    return this.apiService.get<ContentItem>(`content/${slug}`, { language });
  }

  // Get FAQs (with optional category filter)
  getFAQs(category?: string, language?: string): Observable<FAQCategory[]> {
    return this.apiService.get<FAQCategory[]>('content/faqs', { category, language });
  }

  // Get Terms and Conditions
  getTermsAndConditions(language?: string): Observable<ContentItem> {
    return this.apiService.get<ContentItem>(`content/type/${ContentType.TermsAndConditions}`, { language });
  }

  // Get Privacy Policy
  getPrivacyPolicy(language?: string): Observable<ContentItem> {
    return this.apiService.get<ContentItem>(`content/type/${ContentType.PrivacyPolicy}`, { language });
  }

  // Get About Us
  getAboutUs(language?: string): Observable<ContentItem> {
    return this.apiService.get<ContentItem>(`content/type/${ContentType.AboutUs}`, { language });
  }

  // Get Contact Info
  getContactInfo(language?: string): Observable<ContentItem> {
    return this.apiService.get<ContentItem>(`content/type/${ContentType.ContactInfo}`, { language });
  }

  // Get Service Agreement
  getServiceAgreement(language?: string): Observable<ContentItem> {
    return this.apiService.get<ContentItem>(`content/type/${ContentType.ServiceAgreement}`, { language });
  }

  // Get Refund Policy
  getRefundPolicy(language?: string): Observable<ContentItem> {
    return this.apiService.get<ContentItem>(`content/type/${ContentType.RefundPolicy}`, { language });
  }

  // Get Cancellation Policy
  getCancellationPolicy(language?: string): Observable<ContentItem> {
    return this.apiService.get<ContentItem>(`content/type/${ContentType.CancellationPolicy}`, { language });
  }

  // Get Help Articles
  getHelpArticles(language?: string): Observable<ContentItem[]> {
    return this.apiService.get<ContentItem[]>(`content/type/${ContentType.HelpArticle}`, { language });
  }

  // Search content
  searchContent(searchTerm: string, type?: string, language?: string): Observable<ContentItem[]> {
    return this.apiService.get<ContentItem[]>('content/search', {
      searchTerm,
      type,
      language
    });
  }

  // Increment view count (called when user views content)
  incrementViewCount(contentId: string): Observable<any> {
    return this.apiService.post(`content/${contentId}/view`, {});
  }

  // Get available content types
  getContentTypes(): { value: string; label: string }[] {
    return [
      { value: ContentType.FAQ, label: 'FAQ' },
      { value: ContentType.TermsAndConditions, label: 'Terms and Conditions' },
      { value: ContentType.PrivacyPolicy, label: 'Privacy Policy' },
      { value: ContentType.AboutUs, label: 'About Us' },
      { value: ContentType.HelpArticle, label: 'Help Article' },
      { value: ContentType.ContactInfo, label: 'Contact Information' },
      { value: ContentType.ServiceAgreement, label: 'Service Agreement' },
      { value: ContentType.RefundPolicy, label: 'Refund Policy' },
      { value: ContentType.CancellationPolicy, label: 'Cancellation Policy' }
    ];
  }

  // Helper method to get localized title
  getLocalizedTitle(content: ContentItem, language: string): string {
    return language === 'ar' ? content.titleAr : content.titleEn;
  }

  // Helper method to get localized body
  getLocalizedBody(content: ContentItem, language: string): string {
    return language === 'ar' ? content.bodyAr : content.bodyEn;
  }

  // Helper method to get localized meta title
  getLocalizedMetaTitle(content: ContentItem, language: string): string | undefined {
    return language === 'ar' ? content.metaTitleAr : content.metaTitleEn;
  }

  // Helper method to get localized meta description
  getLocalizedMetaDescription(content: ContentItem, language: string): string | undefined {
    return language === 'ar' ? content.metaDescriptionAr : content.metaDescriptionEn;
  }
}
