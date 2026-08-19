import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface Supplier {
  id: string;
  name: string;
  contactPerson?: string;
  mobile: string;
  email?: string;
  gstNumber?: string;
  address?: string;
  outstandingPayable: number;
  isActive: boolean;
}

export interface CreateSupplierRequest {
  name: string;
  contactPerson?: string;
  mobile: string;
  email?: string;
  gstNumber?: string;
  address?: string;
}

export interface UpdateSupplierRequest {
  id: string;
  name: string;
  contactPerson?: string;
  mobile: string;
  email?: string;
  gstNumber?: string;
  address?: string;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class SupplierService {
  constructor(private http: HttpClient) {}

  getAllSuppliers(): Observable<Supplier[]> {
    return this.http.get<Supplier[]>(`${environment.apiUrl}/suppliers`);
  }

  getSupplierById(id: string): Observable<Supplier> {
    return this.http.get<Supplier>(`${environment.apiUrl}/suppliers/${id}`);
  }

  createSupplier(request: CreateSupplierRequest): Observable<Supplier> {
    return this.http.post<Supplier>(`${environment.apiUrl}/suppliers`, request);
  }

  updateSupplier(id: string, request: UpdateSupplierRequest): Observable<Supplier> {
    return this.http.put<Supplier>(`${environment.apiUrl}/suppliers/${id}`, request);
  }
}
