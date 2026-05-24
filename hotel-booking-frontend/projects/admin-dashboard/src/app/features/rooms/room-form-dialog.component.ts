import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { TranslocoPipe } from '@jsverse/transloco';
import type { RoomDto, RoomType } from '@hotel/shared/data-access';

const ROOM_TYPES: RoomType[] = [0, 1, 2, 3];

export interface RoomFormDialogData {
  hotelId: number;
  room?: RoomDto;
}

@Component({
  selector: 'app-room-form-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    TranslocoPipe,
  ],
  template: `
    <h2 mat-dialog-title>
      {{ (data.room ? 'admin.rooms.edit' : 'admin.rooms.create') | transloco }}
    </h2>
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>{{ 'admin.rooms.roomNumber' | transloco }}</mat-label>
          <input matInput formControlName="roomNumber" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>{{ 'admin.rooms.type' | transloco }}</mat-label>
          <mat-select formControlName="type">
            @for (t of roomTypes; track t) {
              <mat-option [value]="t">{{ roomTypeLabel(t) | transloco }}</mat-option>
            }
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>{{ 'admin.rooms.capacity' | transloco }}</mat-label>
          <input matInput type="number" formControlName="capacity" min="1" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>{{ 'admin.rooms.price' | transloco }}</mat-label>
          <input matInput type="number" formControlName="price" min="0" step="0.01" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>{{ 'admin.rooms.description' | transloco }}</mat-label>
          <textarea matInput formControlName="description" rows="3"></textarea>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" mat-dialog-close>{{ 'admin.common.cancel' | transloco }}</button>
      <button mat-flat-button color="primary" type="button" [disabled]="form.invalid" (click)="save()">
        {{ 'admin.common.save' | transloco }}
      </button>
    </mat-dialog-actions>
  `,
  styles: `
    .full-width {
      width: 100%;
      display: block;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoomFormDialogComponent {
  readonly data = inject<RoomFormDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<RoomFormDialogComponent>);
  private readonly fb = inject(FormBuilder);

  readonly roomTypes = ROOM_TYPES;

  readonly form = this.fb.nonNullable.group({
    roomNumber: [this.data.room?.roomNumber ?? '', Validators.required],
    type: [this.data.room?.type ?? (0 as RoomType), Validators.required],
    capacity: [this.data.room?.capacity ?? 1, [Validators.required, Validators.min(1)]],
    price: [this.data.room?.price ?? 0, [Validators.required, Validators.min(0)]],
    description: [this.data.room?.description ?? ''],
  });

  roomTypeLabel(type: RoomType): string {
    const keys = [
      'admin.rooms.typeStandard',
      'admin.rooms.typeDeluxe',
      'admin.rooms.typeSuite',
      'admin.rooms.typePresidential',
    ];
    return keys[type] ?? keys[0];
  }

  save(): void {
    if (this.form.invalid) {
      return;
    }
    const value = this.form.getRawValue();
    this.dialogRef.close({
      ...value,
      hotelId: this.data.hotelId,
      id: this.data.room?.id,
    });
  }
}
