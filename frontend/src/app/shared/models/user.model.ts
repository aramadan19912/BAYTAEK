export enum UserRole {
  Customer = 'Customer',
  ServiceProvider = 'ServiceProvider',
  Admin = 'Admin',
  SupportAgent = 'SupportAgent',
  SuperAdmin = 'SuperAdmin'
}

export enum Language {
  Arabic = 'Arabic',
  English = 'English'
}

export enum Region {
  SaudiArabia = 'SaudiArabia',
  Egypt = 'Egypt'
}

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  role: UserRole;
  isEmailVerified: boolean;
  isPhoneVerified: boolean;
  preferredLanguage: Language;
  region: Region;
  profileImageUrl?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  password: string;
  role: UserRole;
  preferredLanguage: Language;
  region: Region;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  user: User;
}
