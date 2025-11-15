import { Component, OnInit, Output, EventEmitter, Input, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatRadioModule } from '@angular/material/radio';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { Subject, takeUntil } from 'rxjs';
import { AddressService, Address } from '../../../core/services/address.service';
import { AddressFormDialog, AddressFormDialogData } from '../address-form-dialog/address-form-dialog';

@Component({
  selector: 'app-address-selector',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatRadioModule,
    MatDialogModule,
    TranslateModule
  ],
  templateUrl: './address-selector.html',
  styleUrl: './address-selector.scss'
})
export class AddressSelector implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  @Input() selectedAddressId?: string;
  @Output() addressSelected = new EventEmitter<Address>();

  addresses: Address[] = [];
  loading = false;

  constructor(
    private addressService: AddressService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    // Subscribe to addresses
    this.addressService.addresses$
      .pipe(takeUntil(this.destroy$))
      .subscribe(addresses => {
        this.addresses = addresses;

        // Auto-select default address if none selected
        if (!this.selectedAddressId && addresses.length > 0) {
          const defaultAddr = addresses.find(a => a.isDefault);
          if (defaultAddr) {
            this.selectAddress(defaultAddr);
          }
        }
      });

    // Load addresses
    this.addressService.loadAddresses();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  selectAddress(address: Address): void {
    this.selectedAddressId = address.addressId;
    this.addressSelected.emit(address);
  }

  openAddAddressDialog(): void {
    const dialogRef = this.dialog.open<AddressFormDialog, AddressFormDialogData>(AddressFormDialog, {
      width: '600px',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result?.success) {
        // Addresses will be auto-refreshed via service observable
      }
    });
  }

  getAddressIcon(label: string): string {
    return this.addressService.getAddressLabelIcon(label);
  }

  formatAddress(address: Address): string {
    return this.addressService.formatAddress(address);
  }
}
