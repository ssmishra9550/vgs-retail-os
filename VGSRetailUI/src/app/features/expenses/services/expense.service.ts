import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface Expense {
  id: string;
  storeId: string;
  category: string;
  amount: number;
  expenseDate: string;
  paymentMethod: string;
  description?: string;
  status: string;
}

export interface RecordExpenseRequest {
  storeId: string;
  category: string;
  amount: number;
  expenseDate: string;
  paymentMethod: string;
  description?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ExpenseService {
  constructor(private http: HttpClient) {}

  getAllExpenses(storeId: string): Observable<Expense[]> {
    return this.http.get<Expense[]>(`${environment.apiUrl}/expenses/store/${storeId}`);
  }

  recordExpense(request: RecordExpenseRequest): Observable<Expense> {
    return this.http.post<Expense>(`${environment.apiUrl}/expenses`, request);
  }
}
