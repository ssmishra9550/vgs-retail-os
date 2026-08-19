import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface Category {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface CreateCategoryRequest {
  name: string;
  description?: string;
}

export interface UpdateCategoryRequest {
  name: string;
  description?: string;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  constructor(private http: HttpClient) {}

  getAllCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${environment.apiUrl}/categories`);
  }

  createCategory(request: CreateCategoryRequest): Observable<Category> {
    return this.http.post<Category>(`${environment.apiUrl}/categories`, request);
  }

  updateCategory(id: string, request: UpdateCategoryRequest): Observable<Category> {
    return this.http.put<Category>(`${environment.apiUrl}/categories/${id}`, request);
  }
}
