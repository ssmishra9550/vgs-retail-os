import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface StockBalance {
  storeId: string;
  productId: string;
  quantity: number;
  lastUpdated: string;
}

export interface InventoryLedger {
  id: string;
  storeId: string;
  productId: string;
  changeQuantity: number;
  balanceAfter: number;
  transactionType: string;
  referenceId?: string;
  reason?: string;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  constructor(private http: HttpClient) {}

  getAllStockBalances(storeId: string): Observable<StockBalance[]> {
    return this.http.get<StockBalance[]>(`${environment.apiUrl}/inventory/balance/${storeId}`);
  }

  getStockHistory(storeId: string, productId: string): Observable<InventoryLedger[]> {
    return this.http.get<InventoryLedger[]>(`${environment.apiUrl}/inventory/history/${storeId}/${productId}`);
  }
}
