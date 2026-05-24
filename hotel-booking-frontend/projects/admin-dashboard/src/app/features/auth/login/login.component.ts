import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Router } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthStore } from '@hotel/shared/auth';
import { AuthApiFacade } from '@hotel/shared/data-access';
import { ApiBusinessError } from '@hotel/shared/core';

@Component({
  selector: 'app-admin-login',
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    TranslocoPipe,
  ],
  template: `
    <div class="auth-page">
      <mat-card class="auth-card">
        <mat-card-header>
          <mat-card-title>{{ 'auth.loginTitle' | transloco }} — {{ 'app.adminDashboard' | transloco }}</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="form" (ngSubmit)="submit()">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>{{ 'auth.email' | transloco }}</mat-label>
              <input matInput type="email" formControlName="email" autocomplete="email" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>{{ 'auth.password' | transloco }}</mat-label>
              <input matInput type="password" formControlName="password" autocomplete="current-password" />
            </mat-form-field>
            @if (errorMessage()) {
              <p class="error">{{ errorMessage() | transloco }}</p>
            }
            <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid || loading()">
              {{ 'auth.submitLogin' | transloco }}
            </button>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: `
    .auth-page {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      padding: 2rem;
    }
    .auth-card {
      width: 100%;
      max-width: 420px;
    }
    .full-width {
      width: 100%;
      display: block;
      margin-bottom: 0.5rem;
    }
    .error {
      color: #c62828;
      margin-bottom: 1rem;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminLoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authApi = inject(AuthApiFacade);
  private readonly authStore = inject(AuthStore);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  submit(): void {
    if (this.form.invalid) {
      return;
    }
    this.loading.set(true);
    this.errorMessage.set(null);
    const { email, password } = this.form.getRawValue();

    this.authApi.login({ email, password }).subscribe({
      next: (response) => {
        this.authStore.setSession(response);
        const role = response.user?.role;
        if (role === 0) {
          this.authStore.clearSession();
          this.errorMessage.set('admin.common.accessDenied');
          this.loading.set(false);
          return;
        }
        if (role === 1) {
          void this.router.navigateByUrl('/dashboard');
        } else if (role === 2) {
          void this.router.navigateByUrl('/bookings');
        } else {
          this.authStore.clearSession();
          this.errorMessage.set('admin.common.accessDenied');
          this.loading.set(false);
        }
      },
      error: (err: unknown) => {
        this.loading.set(false);
        this.errorMessage.set(err instanceof ApiBusinessError ? err.message : 'Login failed');
      },
      complete: () => this.loading.set(false),
    });
  }
}
