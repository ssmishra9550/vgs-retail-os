import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface Brand {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface CreateBrandRequest {
  name: string;
  description?: string;
}

export interface UpdateBrandRequest {
  name: string;
  description?: string;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class BrandService {
  constructor(private http: HttpClient) {}

  getAllBrands(): Observable<Brand[]> {
    return this.http.get<Brand[]>(`${environment.apiUrl}/brands`);
  }

  createBrand(request: CreateBrandRequest): Observable<Brand> {
    return this.http.post<Brand>(`${environment.apiUrl}/brands`, request);
  }

  updateBrand(id: string, request: UpdateBrandRequest): Observable<Brand> {
    return this.http.put<Brand>(`${environment.apiUrl}/brands/${id}`, request);
  }
}
