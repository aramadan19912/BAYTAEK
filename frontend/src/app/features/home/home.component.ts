import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  template: `
    <div class="home-container">
      <section class="hero">
        <div class="hero-content">
          <h1>{{ 'app.title' | translate }}</h1>
          <p>{{ 'app.description' | translate }}</p>
          <div class="cta-buttons">
            <button routerLink="/services" class="btn btn-primary">
              {{ 'nav.services' | translate }}
            </button>
            <button routerLink="/auth/register" class="btn btn-secondary">
              {{ 'nav.register' | translate }}
            </button>
          </div>
        </div>
      </section>

      <section class="features">
        <h2>{{ 'home.why_choose_us' | translate }}</h2>
        <div class="features-grid">
          <div class="feature-card">
            <div class="icon">🔧</div>
            <h3>{{ 'home.professional_service' | translate }}</h3>
            <p>{{ 'home.professional_service_desc' | translate }}</p>
          </div>
          <div class="feature-card">
            <div class="icon">⚡</div>
            <h3>{{ 'home.fast_booking' | translate }}</h3>
            <p>{{ 'home.fast_booking_desc' | translate }}</p>
          </div>
          <div class="feature-card">
            <div class="icon">💯</div>
            <h3>{{ 'home.quality_guarantee' | translate }}</h3>
            <p>{{ 'home.quality_guarantee_desc' | translate }}</p>
          </div>
          <div class="feature-card">
            <div class="icon">💳</div>
            <h3>{{ 'home.secure_payment' | translate }}</h3>
            <p>{{ 'home.secure_payment_desc' | translate }}</p>
          </div>
        </div>
      </section>
    </div>
  `,
  styles: [`
    .home-container {
      min-height: calc(100vh - 100px);
    }

    .hero {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      padding: 6rem 2rem;
      text-align: center;
    }

    .hero-content h1 {
      font-size: 3rem;
      margin-bottom: 1rem;
      font-weight: bold;
    }

    .hero-content p {
      font-size: 1.5rem;
      margin-bottom: 2rem;
      opacity: 0.9;
    }

    .cta-buttons {
      display: flex;
      gap: 1rem;
      justify-content: center;
      flex-wrap: wrap;
    }

    .btn {
      padding: 1rem 2rem;
      border: none;
      border-radius: 4px;
      font-size: 1.1rem;
      cursor: pointer;
      transition: all 0.3s;
      text-decoration: none;
      display: inline-block;
    }

    .btn-primary {
      background: white;
      color: #667eea;
    }

    .btn-secondary {
      background: transparent;
      color: white;
      border: 2px solid white;
    }

    .btn:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
    }

    .features {
      padding: 4rem 2rem;
      max-width: 1200px;
      margin: 0 auto;
    }

    .features h2 {
      text-align: center;
      font-size: 2.5rem;
      margin-bottom: 3rem;
      color: #333;
    }

    .features-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 2rem;
    }

    .feature-card {
      text-align: center;
      padding: 2rem;
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
      transition: transform 0.3s;
    }

    .feature-card:hover {
      transform: translateY(-5px);
    }

    .icon {
      font-size: 3rem;
      margin-bottom: 1rem;
    }

    .feature-card h3 {
      color: #667eea;
      margin-bottom: 0.5rem;
    }

    .feature-card p {
      color: #666;
    }
  `]
})
export class HomeComponent {}
