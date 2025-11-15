import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface Category {
  categoryId: string;
  nameEn: string;
  nameAr: string;
  descriptionEn?: string;
  descriptionAr?: string;
  icon?: string;
  imageUrl?: string;
  order: number;
  isActive: boolean;
  servicesCount?: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCategoryRequest {
  nameEn: string;
  nameAr: string;
  descriptionEn?: string;
  descriptionAr?: string;
  icon?: string;
  imageUrl?: string;
  order?: number;
  isActive?: boolean;
}

export interface UpdateCategoryRequest {
  nameEn?: string;
  nameAr?: string;
  descriptionEn?: string;
  descriptionAr?: string;
  icon?: string;
  imageUrl?: string;
  order?: number;
  isActive?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  constructor(private apiService: ApiService) {}

  // Get all categories
  getCategories(params?: {
    isActive?: boolean;
    includeServicesCount?: boolean;
  }): Observable<{ categories: Category[]; totalCount: number }> {
    return this.apiService.get('categories', params);
  }

  // Get active categories only
  getActiveCategories(): Observable<{ categories: Category[] }> {
    return this.apiService.get('categories', { isActive: true });
  }

  // Get category by ID
  getCategoryById(categoryId: string): Observable<Category> {
    return this.apiService.get<Category>(`categories/${categoryId}`);
  }

  // Create category (admin only)
  createCategory(request: CreateCategoryRequest): Observable<any> {
    return this.apiService.post('categories', request);
  }

  // Update category (admin only)
  updateCategory(categoryId: string, request: UpdateCategoryRequest): Observable<any> {
    return this.apiService.put(`categories/${categoryId}`, request);
  }

  // Delete category (admin only)
  deleteCategory(categoryId: string): Observable<any> {
    return this.apiService.delete(`categories/${categoryId}`);
  }

  // Get localized name
  getLocalizedName(category: Category, language: string): string {
    return language === 'ar' ? category.nameAr : category.nameEn;
  }

  // Get localized description
  getLocalizedDescription(category: Category, language: string): string | undefined {
    return language === 'ar' ? category.descriptionAr : category.descriptionEn;
  }

  // Get category route
  getCategoryRoute(categoryId: string): string {
    return `/services?category=${categoryId}`;
  }

  // Get category icon (default icons if not provided)
  getCategoryIcon(category: Category): string {
    if (category.icon) {
      return category.icon;
    }

    // Default icons based on category name
    const name = category.nameEn.toLowerCase();
    const iconMap: Record<string, string> = {
      'cleaning': '🧹',
      'plumbing': '🔧',
      'electrical': '⚡',
      'carpentry': '🪚',
      'appliance': '🔌',
      'painting': '🎨',
      'pest control': '🐛',
      'ac': '❄️',
      'air conditioning': '❄️',
      'moving': '📦',
      'packing': '📦',
      'landscaping': '🌳',
      'gardening': '🌱',
      'handyman': '🔨',
      'roofing': '🏠',
      'flooring': '🪵',
      'window': '🪟',
      'door': '🚪'
    };

    for (const [key, icon] of Object.entries(iconMap)) {
      if (name.includes(key)) {
        return icon;
      }
    }

    return '🔧'; // Default icon
  }

  // Sort categories by order
  sortCategoriesByOrder(categories: Category[]): Category[] {
    return [...categories].sort((a, b) => a.order - b.order);
  }

  // Filter active categories
  filterActiveCategories(categories: Category[]): Category[] {
    return categories.filter(c => c.isActive);
  }
}
