export enum Currency {
  SAR = 'SAR',
  EGP = 'EGP',
  USD = 'USD'
}

export interface Service {
  id: string;
  serviceId?: string; // Alias for id
  categoryId: string;
  name: string;
  description: string;
  basePrice: number;
  currency: Currency;
  estimatedDurationMinutes: number;
  isFeatured: boolean;
  imageUrls: string[];
  videoUrl?: string;
}

export interface ServiceCategory {
  id: string;
  name: string;
  description: string;
  iconUrl: string;
  displayOrder: number;
  isActive: boolean;
  parentCategoryId?: string;
  subCategories?: ServiceCategory[];
}
