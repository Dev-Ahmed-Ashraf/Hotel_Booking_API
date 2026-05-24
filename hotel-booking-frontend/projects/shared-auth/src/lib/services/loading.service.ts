import { Injectable, computed, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private readonly activeCount = signal(0);

  readonly isLoading = computed(() => this.activeCount() > 0);

  show(): void {
    this.activeCount.update((count) => count + 1);
  }

  hide(): void {
    this.activeCount.update((count) => Math.max(0, count - 1));
  }
}
