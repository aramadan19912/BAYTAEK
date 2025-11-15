import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject, takeUntil } from 'rxjs';
import { AddressService, Address } from '../../core/services/address.service';
import { AddressFormDialog, AddressFormDialogData } from '../../shared/components/address-form-dialog/address-form-dialog';

@Component({
  selector: 'app-address-management',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    TranslateModule
  ],
  templateUrl: './address-management.html',
  styleUrl: './address-management.scss'
})
export class AddressManagement implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  addresses: Address[] = [];
  loading = false;

  constructor(
    private addressService: AddressService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    // Subscribe to addresses
    this.addressService.addresses$
      .pipe(takeUntil(this.destroy$))
      .subscribe(addresses => {
        this.addresses = addresses;
        this.loading = false;
      });

    // Load addresses
    this.loading = true;
    this.addressService.loadAddresses();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  openAddDialog(): void {
    const dialogRef = this.dialog.open<AddressFormDialog, AddressFormDialogData>(AddressFormDialog, {
      width: '600px',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result?.success) {
        this.showMessage('address.addressCreated');
      }
    });
  }

  openEditDialog(address: Address): void {
    const dialogRef = this.dialog.open<AddressFormDialog, AddressFormDialogData>(AddressFormDialog, {
      width: '600px',
      data: { mode: 'edit', address }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result?.success) {
        this.showMessage('address.addressUpdated');
      }
    });
  }

  setAsDefault(address: Address): void {
    if (address.isDefault) return;

    this.addressService.setDefaultAddress(address.addressId).subscribe({
      next: () => {
        this.showMessage('address.defaultAddressSet');
      },
      error: (error) => {
        console.error('Error setting default address:', error);
        this.showMessage('address.errorSettingDefault');
      }
    });
  }

  deleteAddress(address: Address): void {
    if (confirm(this.translate.instant('address.confirmDelete'))) {
      this.addressService.deleteAddress(address.addressId).subscribe({
        next: () => {
          this.showMessage('address.addressDeleted');
        },
        error: (error) => {
          console.error('Error deleting address:', error);
          this.showMessage('address.errorDeleting');
        }
      });
    }
  }

  getAddressIcon(label: string): string {
    return this.addressService.getAddressLabelIcon(label);
  }

  formatAddress(address: Address): string {
    return this.addressService.formatAddress(address);
  }

  private showMessage(key: string): void {
    this.snackBar.open(this.translate.instant(key), '', {
      duration: 3000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom'
    });
  }
}
