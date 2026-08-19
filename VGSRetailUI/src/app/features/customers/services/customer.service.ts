import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface Customer {
  id: string;
  firstName: string;
  lastName?: string;
  mobile: string;
  email?: string;
  address?: string;
  creditBalance: number;
  isActive: boolean;
}

export interface CreateCustomerRequest {
  firstName: string;
  lastName?: string;
  mobile: string;
  email?: string;
  address?: string;
}

export interface UpdateCustomerRequest {
  id: string;
  firstName: string;
  lastName?: string;
  mobile: string;
  email?: string;
  address?: string;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  constructor(private http: HttpClient) {}

  getAllCustomers(): Observable<Customer[]> {
    return this.http.get<Customer[]>(`${environment.apiUrl}/customers`);
  }

  getCustomerById(id: string): Observable<Customer> {
    return this.http.get<Customer>(`${environment.apiUrl}/customers/${id}`);
  }

  createCustomer(request: CreateCustomerRequest): Observable<Customer> {
    return this.http.post<Customer>(`${environment.apiUrl}/customers`, request);
  }

  updateCustomer(id: string, request: UpdateCustomerRequest): Observable<Customer> {
    return this.http.put<Customer>(`${environment.apiUrl}/customers/${id}`, request);
  }
}
