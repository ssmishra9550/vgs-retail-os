import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface SaleItemResponse {
  id: string;
  productId: string;
  quantity: number;
  unitPrice: number;
  discount: number;
  taxAmount: number;
  total: number;
}

export interface SaleResponse {
  id: string;
  storeId: string;
  customerId?: string;
  invoiceNumber: string;
  saleDate: string;
  status: string;
  subTotal: number;
  totalDiscount: number;
  totalTax: number;
  grandTotal: number;
  paidAmount: number;
  items: SaleItemResponse[];
}

@Injectable({
  providedIn: 'root'
})
export class SalesHistoryService {
  constructor(private http: HttpClient) {}

  getSalesHistory(storeId: string): Observable<SaleResponse[]> {
    return this.http.get<SaleResponse[]>(`${environment.apiUrl}/sales/store/${storeId}/history`);
  }

  getDraftSales(storeId: string): Observable<SaleResponse[]> {
    return this.http.get<SaleResponse[]>(`${environment.apiUrl}/sales/store/${storeId}/drafts`);
  }
}
