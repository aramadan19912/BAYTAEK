import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, debounceTime } from 'rxjs/operators';

import { ProviderService } from '../../../core/services/provider.service';
import { ServiceService, Service } from '../../../core/services/service.service';
import { CategoryService, Category } from '../../../core/services/category.service';

@Component({
  selector: 'app-provider-services',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './provider-services.component.html',
  styleUrls: ['./provider-services.component.scss']
})
export class ProviderServicesComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private searchSubject$ = new Subject<string>();

  // Services data
  services: Service[] = [];
  categories: Category[] = [];

  // Filters
  searchTerm = '';
  selectedStatus = '';
  selectedCategory = '';
  sortBy = 'createdAt';
  sortOrder: 'asc' | 'desc' = 'desc';

  // Pagination
  currentPage = 1;
  pageSize = 12;
  totalCount = 0;
  totalPages = 0;

  // Loading states
  isLoading = false;
  isDeleting = false;

  // Selected service for actions
  selectedService: Service | null = null;
  showDeleteModal = false;
  showStatusModal = false;
  newStatus = '';

  constructor(
    private providerService: ProviderService,
    private serviceService: ServiceService,
    private categoryService: CategoryService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCategories();
    this.loadServices();

    // Setup debounced search
    this.searchSubject$.pipe(
      debounceTime(500),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.currentPage = 1;
      this.loadServices();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Load categories
  loadCategories(): void {
    this.categoryService.getActiveCategories().pipe(takeUntil(this.destroy$)).subscribe({
      next: (result) => {
        this.categories = result.categories;
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  // Load services
  loadServices(): void {
    this.isLoading = true;

    const params: any = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortBy: this.sortBy,
      sortOrder: this.sortOrder
    };

    if (this.searchTerm) {
      params.searchTerm = this.searchTerm;
    }
    if (this.selectedStatus) {
      params.status = this.selectedStatus;
    }
    if (this.selectedCategory) {
      params.categoryId = this.selectedCategory;
    }

    this.providerService.getProviderServices(params).pipe(takeUntil(this.destroy$)).subscribe({
      next: (result) => {
        this.services = result.services;
        this.totalCount = result.totalCount;
        this.totalPages = Math.ceil(this.totalCount / this.pageSize);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading services:', error);
        this.isLoading = false;
      }
    });
  }

  // Search handling
  onSearchChange(): void {
    this.searchSubject$.next(this.searchTerm);
  }

  // Filter changes
  onStatusChange(): void {
    this.currentPage = 1;
    this.loadServices();
  }

  onCategoryChange(): void {
    this.currentPage = 1;
    this.loadServices();
  }

  onSortChange(): void {
    this.loadServices();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedStatus = '';
    this.selectedCategory = '';
    this.sortBy = 'createdAt';
    this.sortOrder = 'desc';
    this.currentPage = 1;
    this.loadServices();
  }

  // Pagination
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadServices();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  get paginationPages(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);

    if (end - start + 1 < maxVisible) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  // Navigation
  addNewService(): void {
    this.router.navigate(['/provider/services/add']);
  }

  editService(serviceId: string): void {
    this.router.navigate(['/provider/services/edit', serviceId]);
  }

  viewServiceDetail(serviceId: string): void {
    this.router.navigate(['/services', serviceId]);
  }

  // Service actions
  openDeleteModal(service: Service): void {
    this.selectedService = service;
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.selectedService = null;
  }

  confirmDelete(): void {
    if (!this.selectedService) return;

    this.isDeleting = true;
    this.serviceService.deleteService(this.selectedService.serviceId).subscribe({
      next: () => {
        this.loadServices();
        this.closeDeleteModal();
        this.isDeleting = false;
        alert('Service deleted successfully');
      },
      error: (error) => {
        console.error('Error deleting service:', error);
        alert('Failed to delete service. Please try again.');
        this.isDeleting = false;
      }
    });
  }

  openStatusModal(service: Service): void {
    this.selectedService = service;
    this.newStatus = service.status;
    this.showStatusModal = true;
  }

  closeStatusModal(): void {
    this.showStatusModal = false;
    this.selectedService = null;
    this.newStatus = '';
  }

  confirmStatusChange(): void {
    if (!this.selectedService || !this.newStatus) return;

    this.providerService.updateServiceStatus(this.selectedService.serviceId, this.newStatus).subscribe({
      next: () => {
        this.loadServices();
        this.closeStatusModal();
        alert('Service status updated successfully');
      },
      error: (error) => {
        console.error('Error updating service status:', error);
        alert('Failed to update service status. Please try again.');
      }
    });
  }

  duplicateService(service: Service): void {
    // Navigate to add service page with service data as query params
    this.router.navigate(['/provider/services/add'], {
      queryParams: { duplicate: service.serviceId }
    });
  }

  // Utility methods
  getStatusClass(status: string): string {
    const statusMap: Record<string, string> = {
      'Active': 'status-active',
      'Inactive': 'status-inactive',
      'Pending': 'status-pending',
      'Rejected': 'status-rejected',
      'Suspended': 'status-suspended'
    };
    return statusMap[status] || '';
  }

  getStatusLabel(status: string): string {
    const statusMap: Record<string, string> = {
      'Active': 'Active',
      'Inactive': 'Inactive',
      'Pending': 'Pending Review',
      'Rejected': 'Rejected',
      'Suspended': 'Suspended'
    };
    return statusMap[status] || status;
  }

  getCategoryName(categoryId: string): string {
    const category = this.categories.find(c => c.categoryId === categoryId);
    return category?.nameEn || 'Unknown';
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'SAR',
      minimumFractionDigits: 2
    }).format(amount);
  }

  formatDate(date: string): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  }

  getServiceImage(service: Service): string {
    if (service.images && service.images.length > 0) {
      return service.images[0];
    }
    return 'assets/images/default-service.png';
  }

  canEditService(service: Service): boolean {
    return service.status !== 'Pending' && service.status !== 'Suspended';
  }

  canChangeStatus(service: Service): boolean {
    return service.status === 'Active' || service.status === 'Inactive';
  }
}
