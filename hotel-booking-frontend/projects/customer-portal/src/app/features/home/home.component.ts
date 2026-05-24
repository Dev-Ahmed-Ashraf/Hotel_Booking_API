import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';

@Component({
  selector: 'app-home',
  imports: [MatCardModule, MatButtonModule, RouterLink, TranslocoPipe],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>{{ 'home.welcome' | transloco }}</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <p>{{ 'home.subtitle' | transloco }}</p>
      </mat-card-content>
      <mat-card-actions>
        <a mat-flat-button color="primary" routerLink="/hotels">
          {{ 'home.browseHotels' | transloco }}
        </a>
      </mat-card-actions>
    </mat-card>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent {}
