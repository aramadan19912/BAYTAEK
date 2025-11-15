import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { AdminService, UserListItem } from '../../../core/services/admin.service';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-users-management',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, TranslateModule],
  templateUrl: './users-management.component.html',
  styleUrls: ['./users-management.component.scss']
})
export class UsersManagementComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private searchSubject$ = new Subject<string>();

  users: UserListItem[] = [];
  totalCount = 0;
  isLoading = true;
  currentLanguage = 'en';

  // Filters
  searchTerm = '';
  selectedRole = '';
  selectedStatus = '';
  pageNumber = 1;
  pageSize = 20;

  // Available options
  roles = [
    { value: '', label: 'All Roles' },
    { value: 'Customer', label: 'Customer' },
    { value: 'ServiceProvider', label: 'Service Provider' },
    { value: 'Admin', label: 'Admin' },
    { value: 'SuperAdmin', label: 'Super Admin' }
  ];

  statusOptions = [
    { value: '', label: 'All Status' },
    { value: 'active', label: 'Active' },
    { value: 'inactive', label: 'Inactive' },
    { value: 'suspended', label: 'Suspended' }
  ];

  constructor(
    private adminService: AdminService,
    private languageService: LanguageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentLanguage = this.languageService.getCurrentLanguage();

    this.languageService.currentLanguage$
      .pipe(takeUntil(this.destroy$))
      .subscribe(lang => {
        this.currentLanguage = lang;
      });

    // Setup search debounce
    this.searchSubject$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(searchTerm => {
        this.searchTerm = searchTerm;
        this.pageNumber = 1;
        this.loadUsers();
      });

    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadUsers(): void {
    this.isLoading = true;

    const params: any = {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    };

    if (this.searchTerm) {
      params.searchTerm = this.searchTerm;
    }

    if (this.selectedRole) {
      params.role = this.selectedRole;
    }

    if (this.selectedStatus === 'active') {
      params.isActive = true;
      params.isSuspended = false;
    } else if (this.selectedStatus === 'inactive') {
      params.isActive = false;
    } else if (this.selectedStatus === 'suspended') {
      params.isSuspended = true;
    }

    this.adminService.getUsers(params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.users = result.users;
          this.totalCount = result.totalCount;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading users:', error);
          this.isLoading = false;
        }
      });
  }

  onSearchChange(value: string): void {
    this.searchSubject$.next(value);
  }

  onRoleChange(): void {
    this.pageNumber = 1;
    this.loadUsers();
  }

  onStatusChange(): void {
    this.pageNumber = 1;
    this.loadUsers();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedRole = '';
    this.selectedStatus = '';
    this.pageNumber = 1;
    this.loadUsers();
  }

  viewUserDetails(userId: string): void {
    this.router.navigate(['/admin/users', userId]);
  }

  suspendUser(user: UserListItem, event: Event): void {
    event.stopPropagation();

    const reason = prompt('Please provide a reason for suspension:');
    if (!reason) return;

    const days = prompt('Suspension duration (days, leave empty for indefinite):');
    const suspensionDays = days ? parseInt(days, 10) : undefined;

    this.adminService.suspendUser(user.userId, {
      reason,
      suspensionDays,
      notifyUser: true
    })
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: () => {
        this.loadUsers();
      },
      error: (error) => {
        console.error('Error suspending user:', error);
        alert('Failed to suspend user');
      }
    });
  }

  unsuspendUser(user: UserListItem, event: Event): void {
    event.stopPropagation();

    if (!confirm(`Are you sure you want to unsuspend ${user.fullName}?`)) {
      return;
    }

    this.adminService.unsuspendUser(user.userId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadUsers();
        },
        error: (error) => {
          console.error('Error unsuspending user:', error);
          alert('Failed to unsuspend user');
        }
      });
  }

  deactivateUser(user: UserListItem, event: Event): void {
    event.stopPropagation();

    const reason = prompt(`Are you sure you want to deactivate ${user.fullName}?\nPlease provide a reason:`);
    if (!reason) return;

    this.adminService.deactivateUser(user.userId, reason)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadUsers();
        },
        error: (error) => {
          console.error('Error deactivating user:', error);
          alert('Failed to deactivate user');
        }
      });
  }

  nextPage(): void {
    if (this.hasNextPage()) {
      this.pageNumber++;
      this.loadUsers();
    }
  }

  previousPage(): void {
    if (this.pageNumber > 1) {
      this.pageNumber--;
      this.loadUsers();
    }
  }

  hasNextPage(): boolean {
    return this.pageNumber * this.pageSize < this.totalCount;
  }

  getTotalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  getRoleBadgeClass(role: string): string {
    const roleMap: Record<string, string> = {
      'Customer': 'role-customer',
      'ServiceProvider': 'role-provider',
      'Admin': 'role-admin',
      'SuperAdmin': 'role-superadmin'
    };
    return roleMap[role] || 'role-default';
  }

  getStatusClass(user: UserListItem): string {
    if (user.isSuspended) return 'status-suspended';
    if (!user.isActive) return 'status-inactive';
    return 'status-active';
  }

  getStatusLabel(user: UserListItem): string {
    if (user.isSuspended) return 'Suspended';
    if (!user.isActive) return 'Inactive';
    return 'Active';
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString(this.currentLanguage === 'ar' ? 'ar-SA' : 'en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  trackByUserId(index: number, user: UserListItem): string {
    return user.userId;
  }
}
