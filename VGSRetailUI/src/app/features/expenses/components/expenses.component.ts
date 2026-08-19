import { Component, OnInit, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StoreSelectionService } from '../../../core/services/store.service';
import { ExpenseService, Expense, RecordExpenseRequest } from '../services/expense.service';

@Component({
  selector: 'app-expenses',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Operational Expenses</h1>
        <button class="btn btn-primary" (click)="toggleForm()">
          {{ showForm() ? 'Hide Form' : 'Record Expense' }}
        </button>
      </div>

      <div class="content-body" [class.with-form]="showForm()">
        <div class="list-section">
          @if (loading()) {
            <div class="loading-state">Loading expenses...</div>
          } @else if (expenses().length === 0) {
            <div class="empty-state">
              <p>No expenses recorded yet.</p>
            </div>
          } @else {
            <table class="data-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Category</th>
                  <th>Description</th>
                  <th>Payment Method</th>
                  <th class="text-right">Amount</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                @for (expense of expenses(); track expense.id) {
                  <tr>
                    <td>{{ expense.expenseDate | date:'mediumDate' }}</td>
                    <td>
                      <span class="badge">{{ expense.category }}</span>
                    </td>
                    <td>{{ expense.description || '-' }}</td>
                    <td>{{ expense.paymentMethod }}</td>
                    <td class="text-right font-medium">{{ expense.amount | currency }}</td>
                    <td>
                      <span class="status-badge" [class.success]="expense.status === 'Approved'">
                        {{ expense.status }}
                      </span>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </div>

        @if (showForm()) {
          <div class="form-section">
            <div class="form-card">
              <h3>Record New Expense</h3>
              <form (ngSubmit)="submitExpense()" #expenseForm="ngForm">
                
                <div class="form-group">
                  <label>Category</label>
                  <select [(ngModel)]="newExpense.category" name="category" required class="form-control">
                    <option value="">-- Select Category --</option>
                    <option value="Utilities">Utilities (Electricity, Water, Internet)</option>
                    <option value="Supplies">Store Supplies</option>
                    <option value="Rent">Rent</option>
                    <option value="Maintenance">Maintenance & Repairs</option>
                    <option value="Payroll">Payroll</option>
                    <option value="Marketing">Marketing</option>
                    <option value="Other">Other</option>
                  </select>
                </div>

                <div class="form-group">
                  <label>Amount</label>
                  <input type="number" [(ngModel)]="newExpense.amount" name="amount" required class="form-control" min="0.01" step="0.01" />
                </div>

                <div class="form-group">
                  <label>Expense Date</label>
                  <input type="date" [(ngModel)]="newExpense.expenseDate" name="expenseDate" required class="form-control" />
                </div>

                <div class="form-group">
                  <label>Payment Method</label>
                  <select [(ngModel)]="newExpense.paymentMethod" name="paymentMethod" required class="form-control">
                    <option value="Cash">Cash</option>
                    <option value="Credit Card">Credit Card</option>
                    <option value="Bank Transfer">Bank Transfer</option>
                  </select>
                </div>

                <div class="form-group">
                  <label>Description (Optional)</label>
                  <textarea [(ngModel)]="newExpense.description" name="description" class="form-control" rows="3"></textarea>
                </div>

                <div class="form-actions mt-4">
                  <button type="submit" class="btn btn-success" [disabled]="!expenseForm.form.valid || saving()">
                    {{ saving() ? 'Saving...' : 'Save Expense' }}
                  </button>
                </div>

              </form>
            </div>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .page-container {
      display: flex;
      flex-direction: column;
      height: 100%;
      background: var(--color-background);
    }
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: var(--spacing-lg) var(--spacing-xl);
      background: var(--color-surface);
      border-bottom: 1px solid var(--color-border);
    }
    .page-header h1 {
      font-size: 1.5rem;
      font-weight: 600;
      color: var(--color-text);
      margin: 0;
    }
    .content-body {
      display: flex;
      padding: var(--spacing-xl);
      gap: 1.5rem;
      overflow-y: auto;
    }
    .list-section {
      flex: 1;
      background: var(--color-surface);
      border-radius: var(--radius-lg);
      border: 1px solid var(--color-border);
      overflow: hidden;
    }
    .form-section {
      width: 400px;
      flex-shrink: 0;
    }
    .form-card {
      background: var(--color-surface);
      border-radius: var(--radius-lg);
      border: 1px solid var(--color-border);
      padding: 1.5rem;
      position: sticky;
      top: 0;
    }
    .form-card h3 {
      margin-top: 0;
      margin-bottom: 1.5rem;
      font-size: 1.25rem;
    }

    .data-table {
      width: 100%;
      border-collapse: collapse;
    }
    .data-table th, .data-table td {
      padding: 1rem;
      text-align: left;
      border-bottom: 1px solid var(--color-border);
    }
    .data-table th {
      background: #f9fafb;
      font-size: 0.75rem;
      text-transform: uppercase;
      color: var(--color-text-secondary);
      font-weight: 600;
    }
    .text-right { text-align: right; }
    .font-medium { font-weight: 500; }
    
    .badge {
      display: inline-block;
      padding: 0.25rem 0.5rem;
      border-radius: var(--radius-full);
      font-size: 0.75rem;
      font-weight: 500;
      background: #f3f4f6;
      color: #374151;
    }
    .status-badge {
      display: inline-block;
      padding: 0.25rem 0.75rem;
      border-radius: var(--radius-full);
      font-size: 0.75rem;
      font-weight: 500;
      background: #f3f4f6;
    }
    .status-badge.success {
      background: #dcfce7;
      color: #166534;
    }

    .form-group {
      margin-bottom: 1rem;
    }
    .form-group label {
      display: block;
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--color-text-secondary);
      margin-bottom: 0.5rem;
    }
    .form-control {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      background: var(--color-background);
      font-size: 0.875rem;
    }
    .form-control:focus {
      outline: none;
      border-color: var(--color-primary);
    }
    
    .btn {
      padding: 0.5rem 1rem;
      border-radius: var(--radius-md);
      font-weight: 500;
      cursor: pointer;
      border: none;
      transition: all 0.2s;
    }
    .btn:disabled { opacity: 0.5; cursor: not-allowed; }
    .btn-primary { background: var(--color-primary); color: white; }
    .btn-primary:hover:not(:disabled) { background: var(--color-primary-dark); }
    .btn-success { background: #16a34a; color: white; width: 100%; padding: 0.75rem; }
    .btn-success:hover:not(:disabled) { background: #15803d; }
    
    .mt-4 { margin-top: 1rem; }
    .empty-state, .loading-state {
      padding: 3rem;
      text-align: center;
      color: var(--color-text-secondary);
    }
  `]
})
export class ExpensesComponent implements OnInit {
  expenses = signal<Expense[]>([]);
  loading = signal(true);
  showForm = signal(false);
  saving = signal(false);

  newExpense: Partial<RecordExpenseRequest> = {
    category: '',
    amount: null as unknown as number,
    expenseDate: new Date().toISOString().split('T')[0],
    paymentMethod: 'Cash',
    description: ''
  };

  constructor(
    public storeService: StoreSelectionService,
    private expenseService: ExpenseService
  ) {
    effect(() => {
      const store = this.storeService.activeStore();
      if (store) {
        this.loadExpenses();
      }
    });
  }

  ngOnInit() {
    // initial load is handled by effect
  }

  loadExpenses() {
    const store = this.storeService.activeStore();
    if (!store) {
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.expenseService.getAllExpenses(store.id).subscribe({
      next: (data) => {
        this.expenses.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load expenses', err);
        this.loading.set(false);
      }
    });
  }

  toggleForm() {
    this.showForm.update(v => !v);
  }

  submitExpense() {
    const store = this.storeService.activeStore();
    if (!store) {
      alert('Please select a store first');
      return;
    }

    if (!this.newExpense.category || !this.newExpense.amount || !this.newExpense.expenseDate || !this.newExpense.paymentMethod) {
      return;
    }

    this.saving.set(true);
    const req: RecordExpenseRequest = {
      storeId: store.id,
      category: this.newExpense.category,
      amount: this.newExpense.amount,
      expenseDate: new Date(this.newExpense.expenseDate).toISOString(),
      paymentMethod: this.newExpense.paymentMethod,
      description: this.newExpense.description
    };

    this.expenseService.recordExpense(req).subscribe({
      next: (res) => {
        // Add to top of list
        this.expenses.update(curr => [res, ...curr]);
        this.saving.set(false);
        this.showForm.set(false);
        // Reset form
        this.newExpense = {
          category: '',
          amount: null as unknown as number,
          expenseDate: new Date().toISOString().split('T')[0],
          paymentMethod: 'Cash',
          description: ''
        };
      },
      error: (err) => {
        console.error('Failed to save expense', err);
        alert('Failed to save expense');
        this.saving.set(false);
      }
    });
  }
}
