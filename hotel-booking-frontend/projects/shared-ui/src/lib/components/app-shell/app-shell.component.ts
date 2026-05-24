import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthStore } from '@hotel/shared/auth';
import { LanguageSwitcherComponent } from '@hotel/shared/i18n';
import { ThemeToggleComponent } from '../theme-toggle/theme-toggle.component';

@Component({
  selector: 'hotel-app-shell',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatButtonModule,
    TranslocoPipe,
    LanguageSwitcherComponent,
    ThemeToggleComponent,
  ],
  template: `
    <mat-toolbar color="primary" class="hotel-toolbar">
      <span class="hotel-toolbar__title">{{ titleKey() | transloco }}</span>
      @if (navVisible()) {
        <nav class="hotel-nav">
          @if (isAdminNav()) {
            @if (isAdmin()) {
              <a mat-button routerLink="/dashboard" routerLinkActive="active">
                {{ 'nav.dashboard' | transloco }}
              </a>
            }
            <a mat-button routerLink="/hotels" routerLinkActive="active">
              {{ 'nav.hotels' | transloco }}
            </a>
            <a mat-button routerLink="/rooms" routerLinkActive="active">
              {{ 'admin.nav.rooms' | transloco }}
            </a>
            <a mat-button routerLink="/bookings" routerLinkActive="active">
              {{ 'admin.nav.bookings' | transloco }}
            </a>
            @if (isAdmin()) {
              <a mat-button routerLink="/reviews" routerLinkActive="active">
                {{ 'admin.nav.reviews' | transloco }}
              </a>
            }
          } @else {
            <a mat-button routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">
              {{ 'nav.home' | transloco }}
            </a>
            <a mat-button routerLink="/hotels" routerLinkActive="active">
              {{ 'nav.hotels' | transloco }}
            </a>
            @if (authStore.isAuthenticated()) {
              <a mat-button routerLink="/my-bookings" routerLinkActive="active">
                {{ 'nav.myBookings' | transloco }}
              </a>
              <a mat-button routerLink="/my-reviews" routerLinkActive="active">
                {{ 'nav.myReviews' | transloco }}
              </a>
            }
          }
        </nav>
      }
      <span class="hotel-toolbar__spacer"></span>
      @if (navVisible()) {
        @if (authStore.isAuthenticated()) {
          <button mat-button type="button" (click)="logout()">
            {{ 'nav.logout' | transloco }}
          </button>
        } @else {
          <a mat-button [routerLink]="loginPath()">{{ 'nav.login' | transloco }}</a>
        }
      }
      <hotel-language-switcher />
      <hotel-theme-toggle />
    </mat-toolbar>
    <main class="hotel-main">
      <router-outlet />
    </main>
  `,
  styles: `
    .hotel-toolbar__spacer {
      flex: 1 1 auto;
    }

    .hotel-nav {
      display: flex;
      gap: 0.25rem;
      margin-inline-start: 1rem;
    }

    .hotel-nav a.active {
      font-weight: 600;
      text-decoration: underline;
    }

    .hotel-main {
      padding: 1.5rem;
      min-height: calc(100vh - var(--hotel-toolbar-height, 64px));
      background: var(--hotel-color-background);
      color: var(--hotel-color-text);
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppShellComponent {
  readonly titleKeyInput = input<string | undefined>(undefined, { alias: 'titleKey' });
  readonly showNav = input<boolean | undefined>(undefined);
  readonly navVariant = input<'customer' | 'admin' | undefined>(undefined);

  private readonly route = inject(ActivatedRoute);
  private readonly routeData = toSignal(this.route.data, { initialValue: {} as Record<string, unknown> });

  readonly titleKey = computed(
    () => this.titleKeyInput() ?? (this.routeData()['titleKey'] as string | undefined) ?? 'app.title'
  );

  readonly navVisible = computed(() => this.showNav() ?? !!(this.routeData()['showNav']));

  readonly isAdminNav = computed(() => {
    const variant = this.navVariant();
    if (variant === 'admin') {
      return true;
    }
    if (variant === 'customer') {
      return false;
    }
    return !!(this.routeData()['adminNav']);
  });

  readonly loginPath = computed(
    () => (this.routeData()['loginPath'] as string | undefined) ?? '/auth/login'
  );

  readonly isAdmin = computed(() => this.authStore.role() === 1);

  readonly authStore = inject(AuthStore);
  private readonly router = inject(Router);

  logout(): void {
    this.authStore.clearSession();
    void this.router.navigateByUrl(this.loginPath());
  }
}
