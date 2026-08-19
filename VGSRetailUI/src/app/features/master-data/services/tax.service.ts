import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface Tax {
  id: string;
  name: string;
  rate: number;
  type: string;
  isActive: boolean;
}

export interface CreateTaxRequest {
  name: string;
  rate: number;
  type: string;
}

export interface UpdateTaxRequest {
  name: string;
  rate: number;
  type: string;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class TaxService {
  constructor(private http: HttpClient) {}

  getAllTaxes(): Observable<Tax[]> {
    return this.http.get<Tax[]>(`${environment.apiUrl}/taxes`);
  }

  createTax(request: CreateTaxRequest): Observable<Tax> {
    return this.http.post<Tax>(`${environment.apiUrl}/taxes`, request);
  }

  updateTax(id: string, request: UpdateTaxRequest): Observable<Tax> {
    return this.http.put<Tax>(`${environment.apiUrl}/taxes/${id}`, request);
  }
}
