import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from './api.service';
import { User as SharedUser } from '../../shared/models/user.model';

export type User = SharedUser & {
  userId?: string; // Alias for id
  profilePictureUrl?: string; // Alias for profileImageUrl
  emailVerified?: boolean;
  phoneVerified?: boolean;
  isEmailVerified?: boolean;
  isPhoneVerified?: boolean;
  twoFactorEnabled?: boolean;
  lastLoginAt?: string;
  createdAt?: string;
  dateOfBirth?: string;
  gender?: string;
  bio?: string;
  providerProfile?: ProviderProfileInfo;
  totalBookings?: number;
  completedBookings?: number;
  cancelledBookings?: number;
  emailConfirmed?: boolean;
  phoneConfirmed?: boolean;
  isSuspended?: boolean;
  suspendedUntil?: string;
  registeredAt?: string;
  isActive?: boolean;
};

export interface ProviderProfileInfo {
  providerId: string;
  businessName?: string;
  averageRating: number;
  totalReviews: number;
  completedBookings: number;
  isVerified: boolean;
  licenseNumber?: string;
  certificationDocuments: string[];
  portfolioImages: string[];
  serviceCategories: ServiceCategoryInfo[];
}

export interface ServiceCategoryInfo {
  id: string;
  nameEn: string;
  nameAr: string;
}

export interface UpdateProfileRequest {
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  gender?: string;
  preferredLanguage?: string;
  profilePictureUrl?: string;
  bio?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ChangeEmailRequest {
  newEmail: string;
  password: string;
}

export interface ChangePhoneRequest {
  newPhoneNumber: string;
  password: string;
}

export interface VerifyEmailRequest {
  token: string;
}

export interface VerifyPhoneRequest {
  code: string;
}

export interface NotificationPreferences {
  emailNotifications: boolean;
  smsNotifications: boolean;
  pushNotifications: boolean;
  bookingUpdates: boolean;
  promotionalEmails: boolean;
  newsletterSubscription: boolean;
  reminderNotifications: boolean;
  reviewRequests: boolean;
}

export interface PrivacySettings {
  profileVisibility: string;
  showEmail: boolean;
  showPhone: boolean;
  allowMessagesFromProviders: boolean;
  allowReviewResponses: boolean;
}

export interface AccountStatistics {
  totalBookings: number;
  completedBookings: number;
  cancelledBookings: number;
  totalSpent: number;
  totalReviews: number;
  averageRating: number;
  favoriteServices: number;
  memberSince: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private apiService: ApiService) {
    this.loadCurrentUser();
  }

  // Load current user from API
  loadCurrentUser(): void {
    this.getProfile().subscribe({
      next: (user) => {
        this.currentUserSubject.next(user);
      },
      error: (error) => {
        console.error('Error loading current user:', error);
        this.currentUserSubject.next(null);
      }
    });
  }

  // Get current user value
  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  // Get user profile
  getProfile(): Observable<User> {
    return this.apiService.get<User>('users/profile').pipe(
      tap(user => this.currentUserSubject.next(user))
    );
  }

  // Update user profile
  updateProfile(request: UpdateProfileRequest): Observable<any> {
    return this.apiService.put('users/profile', request).pipe(
      tap(() => this.loadCurrentUser())
    );
  }

  // Upload profile picture
  uploadProfilePicture(file: File): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.apiService.post<{ url: string }>('users/profile/picture', formData);
  }

  // Alias for compatibility
  uploadProfileImage(formData: FormData): Observable<{ url: string }> {
    return this.apiService.post<{ url: string }>('users/profile/picture', formData);
  }

  // Alias for compatibility
  getUserProfile(): Observable<User> {
    return this.getProfile();
  }

  // Delete profile picture
  deleteProfilePicture(): Observable<any> {
    return this.apiService.delete('users/profile/picture').pipe(
      tap(() => this.loadCurrentUser())
    );
  }

  // Change password
  changePassword(request: ChangePasswordRequest): Observable<any> {
    return this.apiService.post('users/change-password', request);
  }

  // Change email
  changeEmail(request: ChangeEmailRequest): Observable<any> {
    return this.apiService.post('users/change-email', request);
  }

  // Change phone number
  changePhone(request: ChangePhoneRequest): Observable<any> {
    return this.apiService.post('users/change-phone', request);
  }

  // Verify email
  verifyEmail(token: string): Observable<any> {
    return this.apiService.post('users/verify-email', { token }).pipe(
      tap(() => this.loadCurrentUser())
    );
  }

  // Resend email verification
  resendEmailVerification(): Observable<any> {
    return this.apiService.post('users/resend-email-verification', {});
  }

  // Verify phone
  verifyPhone(code: string): Observable<any> {
    return this.apiService.post('users/verify-phone', { code }).pipe(
      tap(() => this.loadCurrentUser())
    );
  }

  // Resend phone verification
  resendPhoneVerification(): Observable<any> {
    return this.apiService.post('users/resend-phone-verification', {});
  }

  // Get notification preferences
  getNotificationPreferences(): Observable<NotificationPreferences> {
    return this.apiService.get<NotificationPreferences>('users/preferences/notifications');
  }

  // Update notification preferences
  updateNotificationPreferences(preferences: Partial<NotificationPreferences>): Observable<any> {
    return this.apiService.put('users/preferences/notifications', preferences);
  }

  // Get privacy settings
  getPrivacySettings(): Observable<PrivacySettings> {
    return this.apiService.get<PrivacySettings>('users/preferences/privacy');
  }

  // Update privacy settings
  updatePrivacySettings(settings: Partial<PrivacySettings>): Observable<any> {
    return this.apiService.put('users/preferences/privacy', settings);
  }

  // Get account statistics
  getAccountStatistics(): Observable<AccountStatistics> {
    return this.apiService.get<AccountStatistics>('users/statistics');
  }

  // Deactivate account
  deactivateAccount(password: string, reason?: string): Observable<any> {
    return this.apiService.post('users/deactivate', { password, reason });
  }

  // Delete account (permanent)
  deleteAccount(password: string, reason?: string): Observable<any> {
    return this.apiService.post('users/delete', { password, reason });
  }

  // Export user data (GDPR compliance)
  exportUserData(): Observable<Blob> {
    return this.apiService.get('users/export-data', {}, { responseType: 'blob' }) as Observable<Blob>;
  }

  // Get user's activity log
  getActivityLog(params?: {
    startDate?: string;
    endDate?: string;
    activityType?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<any> {
    return this.apiService.get('users/activity-log', params);
  }

  // Get user's login history
  getLoginHistory(params?: {
    pageNumber?: number;
    pageSize?: number;
  }): Observable<any> {
    return this.apiService.get('users/login-history', params);
  }

  // Block a user
  blockUser(userId: string): Observable<any> {
    return this.apiService.post(`users/${userId}/block`, {});
  }

  // Unblock a user
  unblockUser(userId: string): Observable<any> {
    return this.apiService.post(`users/${userId}/unblock`, {});
  }

  // Get blocked users
  getBlockedUsers(): Observable<{ users: User[] }> {
    return this.apiService.get<{ users: User[] }>('users/blocked');
  }

  // Helper: Get full name
  getFullName(user?: User): string {
    const u = user || this.getCurrentUser();
    if (!u) return '';
    return `${u.firstName} ${u.lastName}`.trim();
  }

  // Helper: Check if email is verified
  isEmailVerified(user?: User): boolean {
    const u = user || this.getCurrentUser();
    return u?.emailConfirmed ?? false;
  }

  // Helper: Check if phone is verified
  isPhoneVerified(user?: User): boolean {
    const u = user || this.getCurrentUser();
    return u?.phoneConfirmed ?? false;
  }

  // Helper: Check if account is suspended
  isAccountSuspended(user?: User): boolean {
    const u = user || this.getCurrentUser();
    if (!u?.isSuspended) return false;

    if (u.suspendedUntil) {
      const suspendedUntil = new Date(u.suspendedUntil);
      return suspendedUntil > new Date();
    }

    return true;
  }

  // Helper: Get role display name
  getRoleDisplayName(role: string): string {
    const roleMap: Record<string, string> = {
      'Customer': 'Customer',
      'Provider': 'Service Provider',
      'Admin': 'Administrator'
    };
    return roleMap[role] || role;
  }

  // Helper: Format registration date
  getMemberSince(user?: User): string {
    const u = user || this.getCurrentUser();
    if (!u?.registeredAt) return '';

    const date = new Date(u.registeredAt);
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'long' });
  }
}
