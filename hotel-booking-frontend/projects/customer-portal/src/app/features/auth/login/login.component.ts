import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthStore } from '@hotel/shared/auth';
import { AuthApiFacade } from '@hotel/shared/data-access';
import { ApiBusinessError } from '@hotel/shared/core';

@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    RouterLink,
    TranslocoPipe,
  ],
  template: `
    <div class="auth-page">
      <mat-card class="auth-card">
        <mat-card-header>
          <mat-card-title>{{ 'auth.loginTitle' | transloco }}</mat-card-title>
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
            @if (sessionExpired()) {
              <p class="info">{{ 'auth.sessionExpired' | transloco }}</p>
            }
            @if (errorMessage()) {
              <p class="error">{{ errorMessage() }}</p>
            }
            <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid || loading()">
              {{ 'auth.submitLogin' | transloco }}
            </button>
          </form>
          <p class="auth-link">
            {{ 'auth.noAccount' | transloco }}
            <a routerLink="/auth/register">{{ 'nav.register' | transloco }}</a>
          </p>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: `
    .auth-page {
      display: flex;
      justify-content: center;
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
    .info {
      color: #b45309;
      margin-bottom: 1rem;
    }
    .auth-link {
      margin-top: 1rem;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authApi = inject(AuthApiFacade);
  private readonly authStore = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly sessionExpired = signal(
    this.route.snapshot.queryParamMap.get('sessionExpired') === 'true'
  );

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
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        void this.router.navigateByUrl(returnUrl && returnUrl.startsWith('/') ? returnUrl : '/');
      },
      error: (err: unknown) => {
        this.loading.set(false);
        this.errorMessage.set(err instanceof ApiBusinessError ? err.message : 'Login failed');
      },
      complete: () => this.loading.set(false),
    });
  }
}
