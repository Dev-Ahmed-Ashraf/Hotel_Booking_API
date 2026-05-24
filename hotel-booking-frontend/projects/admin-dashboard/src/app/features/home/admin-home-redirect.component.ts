import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthStore } from '@hotel/shared/auth';

@Component({
  selector: 'app-admin-home-redirect',
  template: '',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminHomeRedirectComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly authStore = inject(AuthStore);

  ngOnInit(): void {
    const target = this.authStore.role() === 1 ? '/dashboard' : '/bookings';
    void this.router.navigateByUrl(target, { replaceUrl: true });
  }
}
