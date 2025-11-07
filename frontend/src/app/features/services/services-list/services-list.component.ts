import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { Service } from '../../../shared/models/service.model';

@Component({
  selector: 'app-services-list',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, FormsModule],
  template: `
    <div class="services-container">
      <div class="services-header">
        <h1>{{ 'services.title' | translate }}</h1>

        <div class="search-box">
          <input
            type="text"
            [(ngModel)]="searchTerm"
            (ngModelChange)="onSearchChange()"
            [placeholder]="'services.search' | translate"
            class="search-input"
          />
        </div>
      </div>

      <div class="filters">
        <button
          *ngFor="let filter of filters"
          (click)="applyFilter(filter)"
          [class.active]="selectedFilter === filter"
          class="filter-btn"
        >
          {{ filter }}
        </button>
      </div>

      <div class="services-grid" *ngIf="!loading">
        <div *ngFor="let service of services" class="service-card">
          <img [src]="service.imageUrls[0] || 'assets/placeholder.jpg'" [alt]="service.name" />
          <div class="service-content">
            <h3>{{ service.name }}</h3>
            <p class="service-description">{{ service.description }}</p>
            <div class="service-footer">
              <span class="price">{{ service.basePrice }} {{ service.currency }}</span>
              <span class="duration">{{ service.estimatedDurationMinutes }} min</span>
            </div>
            <button [routerLink]="['/services', service.id]" class="btn-book">
              {{ 'services.book_now' | translate }}
            </button>
          </div>
        </div>
      </div>

      <app-loading-spinner *ngIf="loading"></app-loading-spinner>

      <div *ngIf="!loading && services.length === 0" class="no-results">
        <p>{{ 'services.no_results' | translate }}</p>
      </div>
    </div>
  `,
  styles: [`
    .services-container {
      padding: 2rem;
      max-width: 1200px;
      margin: 0 auto;
    }

    .services-header {
      margin-bottom: 2rem;
    }

    h1 {
      color: #333;
      margin-bottom: 1rem;
    }

    .search-box {
      margin-bottom: 1.5rem;
    }

    .search-input {
      width: 100%;
      max-width: 500px;
      padding: 0.75rem 1rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 1rem;
    }

    .filters {
      display: flex;
      gap: 1rem;
      margin-bottom: 2rem;
      flex-wrap: wrap;
    }

    .filter-btn {
      padding: 0.5rem 1rem;
      border: 1px solid #ddd;
      background: white;
      border-radius: 20px;
      cursor: pointer;
      transition: all 0.3s;
    }

    .filter-btn:hover,
    .filter-btn.active {
      background: #007bff;
      color: white;
      border-color: #007bff;
    }

    .services-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 2rem;
    }

    .service-card {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
      overflow: hidden;
      transition: transform 0.3s, box-shadow 0.3s;
    }

    .service-card:hover {
      transform: translateY(-5px);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    }

    .service-card img {
      width: 100%;
      height: 200px;
      object-fit: cover;
    }

    .service-content {
      padding: 1.5rem;
    }

    .service-content h3 {
      margin: 0 0 0.5rem 0;
      color: #333;
    }

    .service-description {
      color: #666;
      font-size: 0.9rem;
      margin-bottom: 1rem;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    .service-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
      padding-top: 1rem;
      border-top: 1px solid #eee;
    }

    .price {
      font-weight: bold;
      color: #007bff;
      font-size: 1.2rem;
    }

    .duration {
      color: #666;
      font-size: 0.9rem;
    }

    .btn-book {
      width: 100%;
      padding: 0.75rem;
      background: #007bff;
      color: white;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 1rem;
      transition: background 0.3s;
    }

    .btn-book:hover {
      background: #0056b3;
    }

    .no-results {
      text-align: center;
      padding: 3rem;
      color: #666;
    }
  `]
})
export class ServicesListComponent implements OnInit {
  services: Service[] = [];
  loading = false;
  searchTerm = '';
  selectedFilter = 'All';
  filters = ['All', 'Cleaning', 'Plumbing', 'Electrical', 'Carpentry'];

  constructor(private apiService: ApiService) {}

  ngOnInit(): void {
    this.loadServices();
  }

  loadServices(): void {
    this.loading = true;
    this.apiService.get<any>('services', {
      searchTerm: this.searchTerm,
      pageSize: 20
    }).subscribe({
      next: (response) => {
        this.services = response.data?.items || [];
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading services', error);
        this.loading = false;
      }
    });
  }

  onSearchChange(): void {
    this.loadServices();
  }

  applyFilter(filter: string): void {
    this.selectedFilter = filter;
    this.loadServices();
  }
}
