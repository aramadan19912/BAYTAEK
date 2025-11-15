import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../../core/services/auth.service';
import { Language, Region, UserRole } from '../../../shared/models/user.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, TranslateModule],
  template: `
    <div class="register-container">
      <div class="register-card">
        <h2>{{ 'auth.register.title' | translate }}</h2>

        <form [formGroup]="registerForm" (ngSubmit)="onSubmit()">
          <div class="form-row">
            <div class="form-group">
              <label for="firstName">{{ 'auth.register.first_name' | translate }}</label>
              <input
                id="firstName"
                type="text"
                formControlName="firstName"
                class="form-control"
              />
            </div>

            <div class="form-group">
              <label for="lastName">{{ 'auth.register.last_name' | translate }}</label>
              <input
                id="lastName"
                type="text"
                formControlName="lastName"
                class="form-control"
              />
            </div>
          </div>

          <div class="form-group">
            <label for="email">{{ 'auth.register.email' | translate }}</label>
            <input
              id="email"
              type="email"
              formControlName="email"
              class="form-control"
            />
          </div>

          <div class="form-group">
            <label for="phoneNumber">{{ 'auth.register.phone' | translate }}</label>
            <input
              id="phoneNumber"
              type="tel"
              formControlName="phoneNumber"
              class="form-control"
              placeholder="+966XXXXXXXXX"
            />
          </div>

          <div class="form-group">
            <label for="region">{{ 'auth.register.region' | translate }}</label>
            <select id="region" formControlName="region" class="form-control">
              <option value="SaudiArabia">Saudi Arabia</option>
              <option value="Egypt">Egypt</option>
            </select>
          </div>

          <div class="form-group">
            <label for="password">{{ 'auth.register.password' | translate }}</label>
            <input
              id="password"
              type="password"
              formControlName="password"
              class="form-control"
            />
          </div>

          <div class="error-message" *ngIf="errorMessage">
            {{ errorMessage }}
          </div>

          <button type="submit" class="btn btn-primary" [disabled]="loading">
            <span *ngIf="loading" class="spinner"></span>
            {{ loading ? ('common.loading' | translate) : ('auth.register.submit' | translate) }}
          </button>
        </form>

        <div class="register-footer">
          <p>
            {{ 'auth.register.have_account' | translate }}
            <a routerLink="/auth/login">{{ 'auth.register.sign_in' | translate }}</a>
          </p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .register-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: calc(100vh - 100px);
      padding: 2rem;
    }

    .register-card {
      background: white;
      padding: 2rem;
      border-radius: 8px;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
      width: 100%;
      max-width: 500px;
    }

    h2 {
      text-align: center;
      margin-bottom: 2rem;
      color: #333;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
    }

    .form-group {
      margin-bottom: 1.5rem;
    }

    label {
      display: block;
      margin-bottom: 0.5rem;
      font-weight: 500;
      color: #555;
    }

    .form-control {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 1rem;
      transition: border-color 0.3s;
    }

    .form-control:focus {
      outline: none;
      border-color: #007bff;
    }

    .error-message {
      color: #dc3545;
      background: #f8d7da;
      padding: 0.75rem;
      border-radius: 4px;
      margin-bottom: 1rem;
      text-align: center;
    }

    .btn {
      width: 100%;
      padding: 0.75rem;
      border: none;
      border-radius: 4px;
      font-size: 1rem;
      cursor: pointer;
      transition: background 0.3s;
    }

    .btn-primary {
      background: #007bff;
      color: white;
    }

    .btn-primary:hover:not(:disabled) {
      background: #0056b3;
    }

    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .spinner {
      display: inline-block;
      width: 16px;
      height: 16px;
      border: 2px solid #ffffff;
      border-top-color: transparent;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
      margin-right: 0.5rem;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .register-footer {
      margin-top: 1.5rem;
      text-align: center;
    }

    .register-footer a {
      color: #007bff;
      text-decoration: none;
    }

    .register-footer a:hover {
      text-decoration: underline;
    }
  `]
})
export class RegisterComponent {
  registerForm: FormGroup;
  loading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(8)]],
      region: ['SaudiArabia', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const formValue = this.registerForm.value;
    const registerData = {
      ...formValue,
      role: UserRole.Customer,
      preferredLanguage: Language.Arabic
    };

    this.authService.register(registerData).subscribe({
      next: (response) => {
        this.router.navigate(['/']);
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error.error?.message || 'Registration failed. Please try again.';
      }
    });
  }
}
