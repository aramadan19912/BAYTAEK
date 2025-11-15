import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import { AddressService, Address, CreateAddressRequest } from '../../../core/services/address.service';

export interface AddressFormDialogData {
  address?: Address;
  mode: 'create' | 'edit';
}

@Component({
  selector: 'app-address-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    TranslateModule
  ],
  templateUrl: './address-form-dialog.html',
  styleUrl: './address-form-dialog.scss'
})
export class AddressFormDialog implements OnInit {
  addressForm!: FormGroup;
  loading = false;
  isEditMode = false;
  regions: { value: string; label: string }[] = [];
  labels: { value: string; label: string; icon: string }[] = [];
  useCurrentLocation = false;
  gettingLocation = false;

  constructor(
    private fb: FormBuilder,
    private addressService: AddressService,
    private dialogRef: MatDialogRef<AddressFormDialog>,
    @Inject(MAT_DIALOG_DATA) public data: AddressFormDialogData
  ) {
    this.isEditMode = data.mode === 'edit';
  }

  ngOnInit(): void {
    this.regions = this.addressService.getRegions();
    this.labels = this.addressService.getAddressLabels();

    this.addressForm = this.fb.group({
      label: [this.data.address?.label || 'Home', Validators.required],
      addressLine: [this.data.address?.addressLine || '', Validators.required],
      region: [this.data.address?.region || '', Validators.required],
      city: [this.data.address?.city || '', Validators.required],
      postalCode: [this.data.address?.postalCode || ''],
      additionalDetails: [this.data.address?.additionalDetails || ''],
      latitude: [this.data.address?.latitude || null],
      longitude: [this.data.address?.longitude || null],
      isDefault: [this.data.address?.isDefault || false]
    });
  }

  async getCurrentLocation(): Promise<void> {
    this.gettingLocation = true;

    try {
      const coords = await this.addressService.getCurrentLocation();

      // Update form with coordinates
      this.addressForm.patchValue({
        latitude: coords.latitude,
        longitude: coords.longitude
      });

      // Try to reverse geocode to get address
      this.addressService.reverseGeocode(coords.latitude, coords.longitude).subscribe({
        next: (result) => {
          this.addressForm.patchValue({
            addressLine: result.address,
            city: result.city,
            region: result.region,
            postalCode: result.postalCode
          });
          this.gettingLocation = false;
        },
        error: () => {
          this.gettingLocation = false;
        }
      });
    } catch (error) {
      console.error('Error getting location:', error);
      this.gettingLocation = false;
    }
  }

  onSubmit(): void {
    if (this.addressForm.invalid) {
      Object.keys(this.addressForm.controls).forEach(key => {
        this.addressForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.loading = true;
    const formValue = this.addressForm.value;

    if (this.isEditMode && this.data.address) {
      // Update existing address
      this.addressService.updateAddress(this.data.address.addressId, formValue).subscribe({
        next: () => {
          this.loading = false;
          this.dialogRef.close({ success: true, action: 'updated' });
        },
        error: (error) => {
          console.error('Error updating address:', error);
          this.loading = false;
        }
      });
    } else {
      // Create new address
      this.addressService.createAddress(formValue as CreateAddressRequest).subscribe({
        next: () => {
          this.loading = false;
          this.dialogRef.close({ success: true, action: 'created' });
        },
        error: (error) => {
          console.error('Error creating address:', error);
          this.loading = false;
        }
      });
    }
  }

  onCancel(): void {
    this.dialogRef.close({ success: false });
  }

  getErrorMessage(fieldName: string): string {
    const field = this.addressForm.get(fieldName);
    if (field?.hasError('required')) {
      return 'This field is required';
    }
    return '';
  }
}
