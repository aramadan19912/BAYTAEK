import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { AdminService, ServiceListItem } from '../../../core/services/admin.service';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-services-management',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './services-management.component.html',
  styleUrls: ['./services-management.component.scss']
})
export class ServicesManagementComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private searchSubject$ = new Subject<string>();

  services: ServiceListItem[] = [];
  isLoading = true;
  currentLanguage = 'en';

  // Filters
  searchTerm = '';
  selectedCategory = '';
  selectedStatus = '';
  selectedApprovalStatus = '';

  // Pagination
  pageNumber = 1;
  pageSize = 20;
  totalCount = 0;
  hasNextPage = false;
  hasPreviousPage = false;

  // Categories (mock data - should come from API in real implementation)
  categories = [
    { value: 'cleaning', label: 'Cleaning' },
    { value: 'plumbing', label: 'Plumbing' },
    { value: 'electrical', label: 'Electrical' },
    { value: 'painting', label: 'Painting' },
    { value: 'carpentry', label: 'Carpentry' },
    { value: 'hvac', label: 'HVAC' },
    { value: 'landscaping', label: 'Landscaping' }
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

    // Setup search debouncing
    this.searchSubject$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(searchTerm => {
        this.searchTerm = searchTerm;
        this.pageNumber = 1;
        this.loadServices();
      });

    this.loadServices();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadServices(): void {
    this.isLoading = true;

    const params: any = {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    };

    if (this.searchTerm) {
      params.searchTerm = this.searchTerm;
    }

    if (this.selectedCategory) {
      params.category = this.selectedCategory;
    }

    if (this.selectedStatus) {
      params.isActive = this.selectedStatus === 'active';
    }

    if (this.selectedApprovalStatus) {
      params.approvalStatus = this.selectedApprovalStatus;
    }

    this.adminService.getServices(params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.services = result.services;
          this.totalCount = result.totalCount;
          this.hasNextPage = result.hasNextPage;
          this.hasPreviousPage = result.hasPreviousPage;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading services:', error);
          this.isLoading = false;
        }
      });
  }

  onSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchSubject$.next(value);
  }

  onCategoryChange(event: Event): void {
    this.selectedCategory = (event.target as HTMLSelectElement).value;
    this.pageNumber = 1;
    this.loadServices();
  }

  onStatusChange(event: Event): void {
    this.selectedStatus = (event.target as HTMLSelectElement).value;
    this.pageNumber = 1;
    this.loadServices();
  }

  onApprovalStatusChange(event: Event): void {
    this.selectedApprovalStatus = (event.target as HTMLSelectElement).value;
    this.pageNumber = 1;
    this.loadServices();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedCategory = '';
    this.selectedStatus = '';
    this.selectedApprovalStatus = '';
    this.pageNumber = 1;
    this.loadServices();
  }

  nextPage(): void {
    if (this.hasNextPage) {
      this.pageNumber++;
      this.loadServices();
    }
  }

  previousPage(): void {
    if (this.hasPreviousPage) {
      this.pageNumber--;
      this.loadServices();
    }
  }

  approveService(service: ServiceListItem, event: Event): void {
    event.stopPropagation();

    if (!confirm(`Are you sure you want to approve "${service.name}"?`)) {
      return;
    }

    this.adminService.approveService(service.serviceId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadServices();
        },
        error: (error) => {
          console.error('Error approving service:', error);
          alert('Failed to approve service. Please try again.');
        }
      });
  }

  rejectService(service: ServiceListItem, event: Event): void {
    event.stopPropagation();

    const reason = prompt(`Please provide a reason for rejecting "${service.name}":`);

    if (!reason) {
      return;
    }

    this.adminService.rejectService(service.serviceId, { reason, notifyProvider: true })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadServices();
        },
        error: (error) => {
          console.error('Error rejecting service:', error);
          alert('Failed to reject service. Please try again.');
        }
      });
  }

  activateService(service: ServiceListItem, event: Event): void {
    event.stopPropagation();

    this.adminService.activateService(service.serviceId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadServices();
        },
        error: (error) => {
          console.error('Error activating service:', error);
          alert('Failed to activate service. Please try again.');
        }
      });
  }

  deactivateService(service: ServiceListItem, event: Event): void {
    event.stopPropagation();

    const reason = prompt(`Please provide a reason for deactivating "${service.name}":`);

    if (!reason) {
      return;
    }

    this.adminService.deactivateService(service.serviceId, { reason, notifyProvider: true })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadServices();
        },
        error: (error) => {
          console.error('Error deactivating service:', error);
          alert('Failed to deactivate service. Please try again.');
        }
      });
  }

  viewServiceDetails(service: ServiceListItem): void {
    this.router.navigate(['/services', service.serviceId]);
  }

  editService(service: ServiceListItem, event: Event): void {
    event.stopPropagation();
    // Navigate to service edit page (to be implemented)
    this.router.navigate(['/admin/services', service.serviceId, 'edit']);
  }

  getApprovalStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'approved':
        return 'approval-approved';
      case 'pending':
        return 'approval-pending';
      case 'rejected':
        return 'approval-rejected';
      default:
        return 'approval-default';
    }
  }

  backToDashboard(): void {
    this.router.navigate(['/admin/dashboard']);
  }

  formatCurrency(amount: number, currency: string = 'SAR'): string {
    return `${currency} ${amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString(this.currentLanguage === 'ar' ? 'ar-SA' : 'en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  getProviderInitial(name: string): string {
    return name ? name.charAt(0).toUpperCase() : '?';
  }
}
