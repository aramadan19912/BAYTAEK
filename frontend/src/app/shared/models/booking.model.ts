import { Currency } from './service.model';

export enum BookingStatus {
  Pending = 'Pending',
  Confirmed = 'Confirmed',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
  Disputed = 'Disputed'
}

export interface Booking {
  id: string;
  customerId: string;
  serviceId: string;
  providerId?: string;
  serviceName: string;
  providerName?: string;
  status: BookingStatus;
  scheduledAt: Date;
  totalAmount: number;
  currency: Currency;
  address: Address;
}

export interface Address {
  id: string;
  label: string;
  street: string;
  city: string;
  state: string;
  region: string;
}

export interface CreateBookingRequest {
  customerId: string;
  serviceId: string;
  addressId: string;
  scheduledAt: Date;
  specialInstructions?: string;
  isRecurring: boolean;
  recurrencePattern?: string;
}
