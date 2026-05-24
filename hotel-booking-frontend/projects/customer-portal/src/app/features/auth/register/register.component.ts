import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthStore } from '@hotel/shared/auth';
import { AuthApiFacade } from '@hotel/shared/data-access';
import { ApiBusinessError } from '@hotel/shared/core';

@Component({
  selector: 'app-register',
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
          <mat-card-title>{{ 'auth.registerTitle' | transloco }}</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="form" (ngSubmit)="submit()">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>{{ 'auth.firstName' | transloco }}</mat-label>
              <input matInput formControlName="firstName" autocomplete="given-name" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>{{ 'auth.lastName' | transloco }}</mat-label>
              <input matInput formControlName="lastName" autocomplete="family-name" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>{{ 'auth.email' | transloco }}</mat-label>
              <input matInput type="email" formControlName="email" autocomplete="email" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>{{ 'auth.password' | transloco }}</mat-label>
              <input matInput type="password" formControlName="password" autocomplete="new-password" />
            </mat-form-field>
            @if (errorMessage()) {
              <p class="error">{{ errorMessage() }}</p>
            }
            <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid || loading()">
              {{ 'auth.submitRegister' | transloco }}
            </button>
          </form>
          <p class="auth-link">
            {{ 'auth.hasAccount' | transloco }}
            <a routerLink="/auth/login">{{ 'nav.login' | transloco }}</a>
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
    .auth-link {
      margin-top: 1rem;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authApi = inject(AuthApiFacade);
  private readonly authStore = inject(AuthStore);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  submit(): void {
    if (this.form.invalid) {
      return;
    }
    this.loading.set(true);
    this.errorMessage.set(null);
    const { firstName, lastName, email, password } = this.form.getRawValue();

    this.authApi
      .register({
        firstName,
        lastName,
        email,
        password,
        role: 0,
      })
      .subscribe({
        next: (response) => {
          this.authStore.setSession(response);
          void this.router.navigateByUrl('/');
        },
        error: (err: unknown) => {
          this.loading.set(false);
          this.errorMessage.set(err instanceof ApiBusinessError ? err.message : 'Registration failed');
        },
        complete: () => this.loading.set(false),
      });
  }
}
