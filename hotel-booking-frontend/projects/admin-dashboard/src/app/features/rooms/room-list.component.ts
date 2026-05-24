import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthStore } from '@hotel/shared/auth';
import {
  HotelsApiFacade,
  RoomsApiFacade,
  type CreateRoomDto,
  type HotelDto,
  type RoomDto,
  type RoomType,
  type UpdateRoomDto,
} from '@hotel/shared/data-access';
import { RoomFormDialogComponent, type RoomFormDialogData } from './room-form-dialog.component';

@Component({
  selector: 'app-room-list',
  imports: [
    MatTableModule,
    MatButtonModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatSelectModule,
    CurrencyPipe,
    TranslocoPipe,
  ],
  template: `
    <h1>{{ 'admin.rooms.title' | transloco }}</h1>

    <mat-form-field appearance="outline" class="hotel-select">
      <mat-label>{{ 'admin.rooms.selectHotel' | transloco }}</mat-label>
      <mat-select [value]="selectedHotelId()" (selectionChange)="onHotelChange($event.value)">
        @for (h of hotels(); track h.id) {
          <mat-option [value]="h.id">{{ h.name }}</mat-option>
        }
      </mat-select>
    </mat-form-field>

    @if (selectedHotelId()) {
      <div class="toolbar">
        <button mat-flat-button color="primary" type="button" (click)="openCreate()">
          {{ 'admin.rooms.create' | transloco }}
        </button>
      </div>
    }

    @if (loading()) {
      <mat-spinner diameter="48" />
    } @else if (!selectedHotelId()) {
      <p>{{ 'admin.rooms.selectHotel' | transloco }}</p>
    } @else if (rooms().length === 0) {
      <p>{{ 'admin.rooms.noRooms' | transloco }}</p>
    } @else {
      <table mat-table [dataSource]="rooms()" class="data-table">
        <ng-container matColumnDef="roomNumber">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.rooms.roomNumber' | transloco }}</th>
          <td mat-cell *matCellDef="let r">{{ r.roomNumber }}</td>
        </ng-container>
        <ng-container matColumnDef="type">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.rooms.type' | transloco }}</th>
          <td mat-cell *matCellDef="let r">{{ r.type }}</td>
        </ng-container>
        <ng-container matColumnDef="capacity">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.rooms.capacity' | transloco }}</th>
          <td mat-cell *matCellDef="let r">{{ r.capacity }}</td>
        </ng-container>
        <ng-container matColumnDef="price">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.rooms.price' | transloco }}</th>
          <td mat-cell *matCellDef="let r">{{ r.price | currency }}</td>
        </ng-container>
        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.rooms.actions' | transloco }}</th>
          <td mat-cell *matCellDef="let r">
            <button mat-button type="button" (click)="openEdit(r)">{{ 'admin.rooms.edit' | transloco }}</button>
            @if (isAdmin()) {
              <button mat-button color="warn" type="button" (click)="confirmDelete(r)">
                {{ 'admin.rooms.delete' | transloco }}
              </button>
            }
          </td>
        </ng-container>
        <tr mat-header-row *matHeaderRowDef="columns"></tr>
        <tr mat-row *matRowDef="let row; columns: columns"></tr>
      </table>
      <mat-paginator
        [length]="totalCount()"
        [pageIndex]="pageIndex()"
        [pageSize]="pageSize()"
        (page)="onPage($event)"
      />
    }
  `,
  styles: `
    .hotel-select {
      width: 100%;
      max-width: 360px;
      display: block;
      margin-bottom: 1rem;
    }
    .toolbar {
      margin-bottom: 1rem;
    }
    .data-table {
      width: 100%;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoomListComponent {
  private readonly hotelsApi = inject(HotelsApiFacade);
  private readonly roomsApi = inject(RoomsApiFacade);
  private readonly authStore = inject(AuthStore);
  private readonly dialog = inject(MatDialog);

  readonly hotels = signal<HotelDto[]>([]);
  readonly rooms = signal<RoomDto[]>([]);
  readonly selectedHotelId = signal<number | null>(null);
  readonly loading = signal(false);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);
  readonly isAdmin = signal(this.authStore.role() === 1);

  readonly columns = ['roomNumber', 'type', 'capacity', 'price', 'actions'];

  constructor() {
    this.hotelsApi.getHotels({ pageSize: 100 }).subscribe({
      next: (result) => {
        const items = result.items ?? [];
        this.hotels.set(items);
        if (items.length > 0 && items[0].id) {
          this.selectedHotelId.set(items[0].id);
          this.loadRooms();
        }
      },
    });
  }

  onHotelChange(hotelId: number): void {
    this.selectedHotelId.set(hotelId);
    this.pageIndex.set(0);
    this.loadRooms();
  }

  onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.loadRooms();
  }

  openCreate(): void {
    const hotelId = this.selectedHotelId();
    if (!hotelId) {
      return;
    }
    this.openDialog({ hotelId });
  }

  openEdit(room: RoomDto): void {
    const hotelId = this.selectedHotelId();
    if (!hotelId) {
      return;
    }
    this.openDialog({ hotelId, room });
  }

  confirmDelete(room: RoomDto): void {
    if (!room.id || !confirm('Delete this room?')) {
      return;
    }
    this.roomsApi.deleteRoom(room.id).subscribe({ next: () => this.loadRooms() });
  }

  private openDialog(data: RoomFormDialogData): void {
    const ref = this.dialog.open(RoomFormDialogComponent, { width: '480px', data });
    ref.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }
      const { id, hotelId, roomNumber, type, capacity, price, description } = result as {
        id?: number;
        hotelId: number;
        roomNumber: string;
        type: RoomType;
        capacity: number;
        price: number;
        description: string;
      };
      const updateBody: UpdateRoomDto = { roomNumber, type, capacity, price, description };
      const createBody: CreateRoomDto = { roomNumber, type, capacity, price, description, hotelId };
      const req$ = id ? this.roomsApi.updateRoom(id, updateBody) : this.roomsApi.createRoom(createBody);
      req$.subscribe({ next: () => this.loadRooms() });
    });
  }

  private loadRooms(): void {
    const hotelId = this.selectedHotelId();
    if (!hotelId) {
      return;
    }
    this.loading.set(true);
    this.roomsApi
      .getRooms({
        hotelId,
        pageNumber: this.pageIndex() + 1,
        pageSize: this.pageSize(),
      })
      .subscribe({
        next: (result) => {
          this.rooms.set(result.items ?? []);
          this.totalCount.set(result.totalCount ?? 0);
          this.loading.set(false);
        },
        error: () => {
          this.rooms.set([]);
          this.loading.set(false);
        },
      });
  }
}
