import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface Unit {
  id: string;
  name: string;
  shortName: string;
  isActive: boolean;
}

export interface CreateUnitRequest {
  name: string;
  shortName: string;
}

export interface UpdateUnitRequest {
  name: string;
  shortName: string;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class UnitService {
  constructor(private http: HttpClient) {}

  getAllUnits(): Observable<Unit[]> {
    return this.http.get<Unit[]>(`${environment.apiUrl}/units`);
  }

  createUnit(request: CreateUnitRequest): Observable<Unit> {
    return this.http.post<Unit>(`${environment.apiUrl}/units`, request);
  }

  updateUnit(id: string, request: UpdateUnitRequest): Observable<Unit> {
    return this.http.put<Unit>(`${environment.apiUrl}/units/${id}`, request);
  }
}
