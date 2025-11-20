import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AuthService } from '../../core/services/auth.service';
import { UserService, UpdateProfileRequest, ChangePasswordRequest, User as UserProfileResponse } from '../../core/services/user.service';
import { AddressService, Address } from '../../core/services/address.service';
import { PaymentService, PaymentMethod } from '../../core/services/payment.service';
import { NotificationService, NotificationSettings } from '../../core/services/notification.service';
import { ProviderService, ProviderProfile } from '../../core/services/provider.service';
import { LanguageService } from '../../core/services/language.service';
import { User as AuthUser, UserRole, Language, Region } from '../../shared/models/user.model';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // User data
  currentUser: (UserProfileResponse | AuthUser) | null = null;
  providerProfile: ProviderProfile | null = null;
  isProvider = false;

  // Active tab
  activeTab: 'profile' | 'addresses' | 'payment' | 'notifications' | 'security' | 'provider' = 'profile';

  // Profile editing
  isEditingProfile = false;
  profileForm: UpdateProfileRequest = {
    firstName: '',
    lastName: '',
    phoneNumber: '',
    dateOfBirth: '',
    gender: '',
    bio: '',
    preferredLanguage: ''
  };

  // Password change
  isChangingPassword = false;
  passwordForm: ChangePasswordRequest = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  // Addresses
  addresses: Address[] = [];
  isAddingAddress = false;
  editingAddress: Address | null = null;
  addressForm: (Partial<Address> & { country?: string }) = {
    addressLine: '',
    city: '',
    region: '',
    postalCode: '',
    isDefault: false,
    additionalDetails: '',
    latitude: 0,
    longitude: 0,
    country: 'Saudi Arabia'
  };

  // Payment methods
  paymentMethods: PaymentMethod[] = [];
  isAddingPaymentMethod = false;

  // Notification settings
  notificationSettings: NotificationSettings = {
    emailNotifications: true,
    smsNotifications: true,
    pushNotifications: true,
    bookingUpdates: true,
    promotionalEmails: false,
    newsletterSubscription: false,
    reminderNotifications: true,
    reviewRequests: true,
    messageNotifications: true
  };

  // Loading states
  isLoadingProfile = false;
  isLoadingAddresses = false;
  isLoadingPaymentMethods = false;
  isLoadingNotificationSettings = false;
  isLoadingProviderProfile = false;

  // Profile image upload
  selectedProfileImage: File | null = null;
  profileImagePreview: string | null = null;

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private addressService: AddressService,
    private paymentService: PaymentService,
    private notificationService: NotificationService,
    private providerService: ProviderService,
    private languageService: LanguageService
  ) {}

  ngOnInit(): void {
    this.loadUserProfile();
    this.loadAddresses();
    this.loadPaymentMethods();
    this.loadNotificationSettings();

    // Subscribe to current user
    this.authService.currentUser$.pipe(takeUntil(this.destroy$)).subscribe(user => {
      this.currentUser = user;
      if (user) {
        this.isProvider = this.isProviderRole(user.role);
        if (this.isProvider) {
          this.loadProviderProfile();
        }
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Load user profile
  loadUserProfile(): void {
    this.isLoadingProfile = true;
    this.userService.getUserProfile().subscribe({
      next: (profile) => {
        const normalizedProfile = this.normalizeProfile(profile);
        this.currentUser = normalizedProfile;
        this.authService.updateCurrentUser(this.mapToAuthUser(normalizedProfile));
        this.isProvider = this.isProviderRole(normalizedProfile.role);
        if (this.isProvider) {
          this.loadProviderProfile();
        }
        this.initializeProfileForm();
        this.isLoadingProfile = false;
      },
      error: (error) => {
        console.error('Error loading profile:', error);
        this.isLoadingProfile = false;
      }
    });
  }

  // Initialize profile form with current user data
  initializeProfileForm(): void {
    if (this.currentUser) {
      const profileDetails = this.currentUser as UserProfileResponse;
      this.profileForm = {
        firstName: this.currentUser.firstName,
        lastName: this.currentUser.lastName,
        phoneNumber: this.currentUser.phoneNumber || '',
        dateOfBirth: profileDetails.dateOfBirth || '',
        gender: profileDetails.gender || '',
        bio: profileDetails.bio || '',
        preferredLanguage: this.getPreferredLanguageCode(this.currentUser.preferredLanguage)
      };
    }
  }

  // Edit profile
  startEditingProfile(): void {
    this.isEditingProfile = true;
    this.initializeProfileForm();
  }

  cancelEditingProfile(): void {
    this.isEditingProfile = false;
    this.selectedProfileImage = null;
    this.profileImagePreview = null;
  }

  saveProfile(): void {
    if (!this.validateProfileForm()) {
      return;
    }

    this.isLoadingProfile = true;

    // Update profile
    this.userService.updateProfile(this.profileForm).subscribe({
      next: () => {
        // Upload profile image if selected
        if (this.selectedProfileImage) {
          this.uploadProfileImage();
        } else {
          this.loadUserProfile();
          this.isEditingProfile = false;
          alert('Profile updated successfully!');
        }
      },
      error: (error) => {
        console.error('Error updating profile:', error);
        alert('Failed to update profile. Please try again.');
        this.isLoadingProfile = false;
      }
    });
  }

  validateProfileForm(): boolean {
    if (!this.profileForm.firstName || !this.profileForm.lastName) {
      alert('First name and last name are required');
      return false;
    }
    if (!this.profileForm.phoneNumber) {
      alert('Phone number is required');
      return false;
    }
    return true;
  }

  // Profile image handling
  onProfileImageSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedProfileImage = file;

      // Create preview
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.profileImagePreview = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  uploadProfileImage(): void {
    if (!this.selectedProfileImage) return;

    const formData = new FormData();
    formData.append('profileImage', this.selectedProfileImage);

    this.userService.uploadProfileImage(formData).subscribe({
      next: () => {
        this.loadUserProfile();
        this.isEditingProfile = false;
        this.selectedProfileImage = null;
        this.profileImagePreview = null;
        alert('Profile updated successfully!');
      },
      error: (error) => {
        console.error('Error uploading profile image:', error);
        alert('Profile updated but failed to upload image. Please try again.');
        this.isLoadingProfile = false;
      }
    });
  }

  // Password change
  startChangingPassword(): void {
    this.isChangingPassword = true;
    this.passwordForm = {
      currentPassword: '',
      newPassword: '',
      confirmPassword: ''
    };
  }

  cancelChangingPassword(): void {
    this.isChangingPassword = false;
  }

  changePassword(): void {
    if (!this.validatePasswordForm()) {
      return;
    }

    this.isLoadingProfile = true;
    this.userService.changePassword(this.passwordForm).subscribe({
      next: () => {
        alert('Password changed successfully!');
        this.isChangingPassword = false;
        this.isLoadingProfile = false;
      },
      error: (error) => {
        console.error('Error changing password:', error);
        alert(error.error?.message || 'Failed to change password. Please try again.');
        this.isLoadingProfile = false;
      }
    });
  }

  validatePasswordForm(): boolean {
    if (!this.passwordForm.currentPassword) {
      alert('Current password is required');
      return false;
    }
    if (!this.passwordForm.newPassword) {
      alert('New password is required');
      return false;
    }
    if (this.passwordForm.newPassword.length < 8) {
      alert('New password must be at least 8 characters long');
      return false;
    }
    if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) {
      alert('New passwords do not match');
      return false;
    }
    return true;
  }

  // Addresses
  loadAddresses(): void {
    this.isLoadingAddresses = true;
    this.addressService.getAddresses().subscribe({
      next: (result) => {
        this.addresses = result.addresses;
        this.isLoadingAddresses = false;
      },
      error: (error) => {
        console.error('Error loading addresses:', error);
        this.isLoadingAddresses = false;
      }
    });
  }

  startAddingAddress(): void {
    this.isAddingAddress = true;
    this.editingAddress = null;
    this.addressForm = {
      addressLine: '',
      city: '',
      region: '',
      postalCode: '',
      isDefault: false,
      additionalDetails: '',
      latitude: 0,
      longitude: 0,
      country: 'Saudi Arabia'
    };
  }

  editAddress(address: Address): void {
    this.isAddingAddress = true;
    this.editingAddress = address;
    this.addressForm = {
      ...address,
      additionalDetails: address.additionalDetails,
      country: address.country ?? 'Saudi Arabia'
    };
  }

  cancelAddingAddress(): void {
    this.isAddingAddress = false;
    this.editingAddress = null;
  }

  saveAddress(): void {
    if (!this.validateAddressForm()) {
      return;
    }

    this.isLoadingAddresses = true;

    if (this.editingAddress) {
      // Update existing address
      this.addressService.updateAddress(this.editingAddress.addressId, this.addressForm).subscribe({
        next: () => {
          this.loadAddresses();
          this.isAddingAddress = false;
          this.editingAddress = null;
        },
        error: (error) => {
          console.error('Error updating address:', error);
          alert('Failed to update address. Please try again.');
          this.isLoadingAddresses = false;
        }
      });
    } else {
      // Add new address
      this.addressService.addAddress(this.addressForm).subscribe({
        next: () => {
          this.loadAddresses();
          this.isAddingAddress = false;
        },
        error: (error) => {
          console.error('Error adding address:', error);
          alert('Failed to add address. Please try again.');
          this.isLoadingAddresses = false;
        }
      });
    }
  }

  validateAddressForm(): boolean {
    if (!this.addressForm.addressLine) {
      alert('Address line is required');
      return false;
    }
    if (!this.addressForm.city) {
      alert('City is required');
      return false;
    }
    if (!this.addressForm.region) {
      alert('Region is required');
      return false;
    }
    return true;
  }

  deleteAddress(addressId: string): void {
    if (!confirm('Are you sure you want to delete this address?')) {
      return;
    }

    this.addressService.deleteAddress(addressId).subscribe({
      next: () => {
        this.loadAddresses();
      },
      error: (error) => {
        console.error('Error deleting address:', error);
        alert('Failed to delete address. Please try again.');
      }
    });
  }

  setDefaultAddress(addressId: string): void {
    this.addressService.setDefaultAddress(addressId).subscribe({
      next: () => {
        this.loadAddresses();
      },
      error: (error) => {
        console.error('Error setting default address:', error);
        alert('Failed to set default address. Please try again.');
      }
    });
  }

  // Payment methods
  loadPaymentMethods(): void {
    this.isLoadingPaymentMethods = true;
    this.paymentService.getPaymentMethods().subscribe({
      next: (methods) => {
        this.paymentMethods = methods;
        this.isLoadingPaymentMethods = false;
      },
      error: (error) => {
        console.error('Error loading payment methods:', error);
        this.isLoadingPaymentMethods = false;
      }
    });
  }

  deletePaymentMethod(paymentMethodId: string): void {
    if (!confirm('Are you sure you want to delete this payment method?')) {
      return;
    }

    this.paymentService.deletePaymentMethod(paymentMethodId).subscribe({
      next: () => {
        this.loadPaymentMethods();
      },
      error: (error) => {
        console.error('Error deleting payment method:', error);
        alert('Failed to delete payment method. Please try again.');
      }
    });
  }

  setDefaultPaymentMethod(paymentMethodId: string): void {
    this.paymentService.setDefaultPaymentMethod(paymentMethodId).subscribe({
      next: () => {
        this.loadPaymentMethods();
      },
      error: (error) => {
        console.error('Error setting default payment method:', error);
        alert('Failed to set default payment method. Please try again.');
      }
    });
  }

  // Notification settings
  loadNotificationSettings(): void {
    this.isLoadingNotificationSettings = true;
    this.notificationService.getNotificationSettings().subscribe({
      next: (settings) => {
        this.notificationSettings = settings;
        this.isLoadingNotificationSettings = false;
      },
      error: (error) => {
        console.error('Error loading notification settings:', error);
        this.isLoadingNotificationSettings = false;
      }
    });
  }

  saveNotificationSettings(): void {
    this.isLoadingNotificationSettings = true;
    this.notificationService.updateNotificationSettings(this.notificationSettings).subscribe({
      next: () => {
        alert('Notification settings updated successfully!');
        this.isLoadingNotificationSettings = false;
      },
      error: (error) => {
        console.error('Error updating notification settings:', error);
        alert('Failed to update notification settings. Please try again.');
        this.isLoadingNotificationSettings = false;
      }
    });
  }

  // Provider profile
  loadProviderProfile(): void {
    this.isLoadingProviderProfile = true;
    this.providerService.getProviderProfile().subscribe({
      next: (profile) => {
        this.providerProfile = profile;
        this.isLoadingProviderProfile = false;
      },
      error: (error) => {
        console.error('Error loading provider profile:', error);
        this.isLoadingProviderProfile = false;
      }
    });
  }

  // Tab switching
  switchTab(tab: 'profile' | 'addresses' | 'payment' | 'notifications' | 'security' | 'provider'): void {
    this.activeTab = tab;
  }

  // Utility methods
  formatDate(date: string): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  getVerificationStatusClass(status: string): string {
    const statusMap: Record<string, string> = {
      'Pending': 'status-pending',
      'UnderReview': 'status-review',
      'Verified': 'status-verified',
      'Rejected': 'status-rejected'
    };
    return statusMap[status] || '';
  }

  getVerificationStatusLabel(status: string): string {
    const statusMap: Record<string, string> = {
      'Pending': 'Pending Submission',
      'UnderReview': 'Under Review',
      'Verified': 'Verified',
      'Rejected': 'Rejected'
    };
    return statusMap[status] || status;
  }

  getCurrentLanguage(): string {
    return this.languageService.getCurrentLanguage();
  }

  private normalizeProfile(profile: UserProfileResponse): UserProfileResponse {
    return {
      ...profile,
      phoneNumber: profile.phoneNumber || '',
      profileImageUrl: profile.profileImageUrl || profile.profilePictureUrl,
      profilePictureUrl: profile.profilePictureUrl || profile.profileImageUrl,
      emailVerified: profile.emailVerified ?? profile.isEmailVerified ?? false,
      phoneVerified: profile.phoneVerified ?? profile.isPhoneVerified ?? false,
      emailConfirmed: profile.emailConfirmed ?? profile.emailVerified,
      phoneConfirmed: profile.phoneConfirmed ?? profile.phoneVerified,
      registeredAt: profile.registeredAt ?? profile.createdAt,
      role: this.resolveUserRole(profile.role),
      preferredLanguage: this.resolveLanguage(profile.preferredLanguage),
      region: this.resolveRegion(profile.region)
    };
  }

  private mapToAuthUser(profile: UserProfileResponse): Partial<AuthUser> {
    return {
      id: profile.id || profile.userId || '',
      firstName: profile.firstName,
      lastName: profile.lastName,
      email: profile.email,
      phoneNumber: profile.phoneNumber || '',
      role: this.resolveUserRole(profile.role),
      preferredLanguage: this.resolveLanguage(profile.preferredLanguage),
      region: this.resolveRegion(profile.region),
      profileImageUrl: profile.profileImageUrl || profile.profilePictureUrl,
      isEmailVerified: profile.isEmailVerified ?? profile.emailVerified ?? false,
      isPhoneVerified: profile.isPhoneVerified ?? profile.phoneVerified ?? false
    };
  }

  private isProviderRole(role?: string | UserRole): boolean {
    return this.resolveUserRole(role) === UserRole.ServiceProvider;
  }

  private resolveUserRole(role?: string | UserRole): UserRole {
    if (!role) return UserRole.Customer;
    const value = role.toString();
    return value === UserRole.ServiceProvider || value.toLowerCase() === 'serviceprovider'
      ? UserRole.ServiceProvider
      : UserRole.Customer;
  }

  private resolveLanguage(value?: string | Language): Language {
    if (!value) {
      return this.languageService.getCurrentLanguage() === 'ar' ? Language.Arabic : Language.English;
    }
    const normalized = value.toString().toLowerCase();
    return normalized === 'ar' || normalized === 'arabic' ? Language.Arabic : Language.English;
  }

  private resolveRegion(value?: string | Region): Region {
    if (!value) {
      return Region.SaudiArabia;
    }
    const normalized = value.toString().toLowerCase();
    return normalized.includes('egypt') ? Region.Egypt : Region.SaudiArabia;
  }

  private getPreferredLanguageCode(value?: string | Language): string {
    const resolved = this.resolveLanguage(value);
    return resolved === Language.Arabic ? 'ar' : 'en';
  }
}
