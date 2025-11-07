import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../../core/services/auth.service';
import { LanguageService } from '../../../core/services/language.service';
import { User } from '../../models/user.model';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  template: `
    <header class="header">
      <div class="container">
        <div class="header-content">
          <div class="logo">
            <a routerLink="/">{{ 'app.title' | translate }}</a>
          </div>

          <nav class="nav">
            <a routerLink="/" routerLinkActive="active">{{ 'nav.home' | translate }}</a>
            <a routerLink="/services" routerLinkActive="active">{{ 'nav.services' | translate }}</a>
            <a *ngIf="currentUser" routerLink="/bookings" routerLinkActive="active">
              {{ 'nav.bookings' | translate }}
            </a>
          </nav>

          <div class="actions">
            <button (click)="toggleLanguage()" class="btn-language">
              {{ currentLanguage === 'en' ? 'العربية' : 'English' }}
            </button>

            <ng-container *ngIf="currentUser; else authButtons">
              <a routerLink="/profile" class="btn-profile">
                {{ 'nav.profile' | translate }}
              </a>
              <button (click)="logout()" class="btn-logout">
                {{ 'nav.logout' | translate }}
              </button>
            </ng-container>

            <ng-template #authButtons>
              <a routerLink="/auth/login" class="btn-login">
                {{ 'nav.login' | translate }}
              </a>
              <a routerLink="/auth/register" class="btn-register">
                {{ 'nav.register' | translate }}
              </a>
            </ng-template>
          </div>
        </div>
      </div>
    </header>
  `,
  styles: [`
    .header {
      background: #fff;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      padding: 1rem 0;
    }

    .container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 0 1rem;
    }

    .header-content {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .logo a {
      font-size: 1.5rem;
      font-weight: bold;
      color: #333;
      text-decoration: none;
    }

    .nav {
      display: flex;
      gap: 1.5rem;
    }

    .nav a {
      color: #666;
      text-decoration: none;
      transition: color 0.3s;
    }

    .nav a:hover,
    .nav a.active {
      color: #007bff;
    }

    .actions {
      display: flex;
      gap: 1rem;
      align-items: center;
    }

    button, a.btn-login, a.btn-register {
      padding: 0.5rem 1rem;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      text-decoration: none;
      transition: background 0.3s;
    }

    .btn-language {
      background: #f0f0f0;
      color: #333;
    }

    .btn-login {
      background: transparent;
      color: #007bff;
      border: 1px solid #007bff;
    }

    .btn-register, .btn-logout {
      background: #007bff;
      color: white;
    }

    button:hover, a:hover {
      opacity: 0.9;
    }
  `]
})
export class HeaderComponent implements OnInit {
  currentUser: User | null = null;
  currentLanguage: string = 'en';

  constructor(
    private authService: AuthService,
    private languageService: LanguageService
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });

    this.languageService.currentLanguage$.subscribe(lang => {
      this.currentLanguage = lang;
    });
  }

  toggleLanguage(): void {
    this.languageService.toggleLanguage();
  }

  logout(): void {
    this.authService.logout();
  }
}
