import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface PurchaseItem {
  id?: string;
  productId: string;
  quantity: number;
  unitCost: number;
  discount: number;
  taxAmount: number;
  total: number;
}

export interface Purchase {
  id: string;
  storeId: string;
  supplierId: string;
  invoiceNumber: string;
  invoiceDate: string;
  status: string;
  subTotal: number;
  totalDiscount: number;
  totalTax: number;
  grandTotal: number;
  createdAt: string;
  items: PurchaseItem[];
}

export interface CreatePurchaseRequest {
  storeId: string;
  supplierId: string;
  invoiceNumber: string;
  invoiceDate: string;
  totalDiscount: number;
  totalTax: number;
  items: Omit<PurchaseItem, 'id' | 'total'>[];
}

@Injectable({
  providedIn: 'root'
})
export class PurchaseService {
  constructor(private http: HttpClient) {}

  getAllPurchases(storeId: string): Observable<Purchase[]> {
    return this.http.get<Purchase[]>(`${environment.apiUrl}/purchases/store/${storeId}`);
  }

  createDraftPurchase(request: CreatePurchaseRequest): Observable<Purchase> {
    return this.http.post<Purchase>(`${environment.apiUrl}/purchases/drafts`, request);
  }

  receivePurchase(purchaseId: string): Observable<Purchase> {
    return this.http.post<Purchase>(`${environment.apiUrl}/purchases/${purchaseId}/receive`, {});
  }
}
