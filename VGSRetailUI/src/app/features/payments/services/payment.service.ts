import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface Payment {
  id: string;
  storeId: string;
  paymentType: string;
  referenceId: string;
  amount: number;
  paymentDate: string;
  paymentMethod: string;
  referenceNumber?: string;
  notes?: string;
}

export interface RecordPaymentRequest {
  storeId: string;
  paymentType: string;
  referenceId: string;
  amount: number;
  paymentDate: string;
  paymentMethod: string;
  referenceNumber?: string;
  notes?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  constructor(private http: HttpClient) {}

  getAllPayments(storeId: string): Observable<Payment[]> {
    return this.http.get<Payment[]>(`${environment.apiUrl}/payments/store/${storeId}`);
  }

  recordPayment(request: RecordPaymentRequest): Observable<Payment> {
    return this.http.post<Payment>(`${environment.apiUrl}/payments`, request);
  }
}
