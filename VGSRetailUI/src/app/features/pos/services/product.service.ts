import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface Product {
  id: string;
  name: string;
  sku?: string;
  description?: string;
  purchasePrice: number;
  sellingPrice: number;
  categoryId?: string;
  categoryName?: string;
  brandId?: string;
  brandName?: string;
  unitId: string;
  unitName?: string;
  taxId?: string;
  taxName?: string;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  constructor(private http: HttpClient) {}

  getAllProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${environment.apiUrl}/products`);
  }
}
