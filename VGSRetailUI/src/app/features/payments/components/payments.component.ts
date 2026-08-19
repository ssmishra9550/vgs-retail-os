import { Component, OnInit, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StoreSelectionService } from '../../../core/services/store.service';
import { PaymentService, Payment, RecordPaymentRequest } from '../services/payment.service';
import { SupplierService, Supplier } from '../../suppliers/services/supplier.service';

@Component({
  selector: 'app-payments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Payments</h1>
        <button class="btn btn-primary" (click)="toggleForm()">
          {{ showForm() ? 'Hide Form' : 'Record Payment' }}
        </button>
      </div>

      <div class="content-body" [class.with-form]="showForm()">
        <div class="list-section">
          @if (loading()) {
            <div class="loading-state">Loading payments...</div>
          } @else if (payments().length === 0) {
            <div class="empty-state">
              <p>No payments recorded yet.</p>
            </div>
          } @else {
            <table class="data-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Type</th>
                  <th>Reference</th>
                  <th>Method</th>
                  <th class="text-right">Amount</th>
                </tr>
              </thead>
              <tbody>
                @for (payment of payments(); track payment.id) {
                  <tr>
                    <td>{{ payment.paymentDate | date:'mediumDate' }}</td>
                    <td>
                      <span class="badge" [class.customer]="payment.paymentType === 'CustomerReceipt'" [class.supplier]="payment.paymentType === 'SupplierPayment'">
                        {{ payment.paymentType === 'SupplierPayment' ? 'Supplier Payment' : 'Customer Receipt' }}
                      </span>
                    </td>
                    <td>
                      <div class="ref-num">{{ payment.referenceNumber || 'N/A' }}</div>
                      <div class="text-xs text-secondary">Ref ID: {{ payment.referenceId | slice:0:8 }}</div>
                    </td>
                    <td>{{ payment.paymentMethod }}</td>
                    <td class="text-right font-medium">{{ payment.amount | currency }}</td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </div>

        @if (showForm()) {
          <div class="form-section">
            <div class="form-card">
              <h3>Record New Payment</h3>
              <form (ngSubmit)="submitPayment()" #paymentForm="ngForm">
                
                <div class="form-group">
                  <label>Payment Type</label>
                  <select [(ngModel)]="newPayment.paymentType" name="paymentType" required class="form-control" (change)="onPaymentTypeChange()">
                    <option value="SupplierPayment">Supplier Payment</option>
                    <option value="CustomerReceipt">Customer Receipt</option>
                  </select>
                </div>

                @if (newPayment.paymentType === 'SupplierPayment') {
                  <div class="form-group">
                    <label>Supplier</label>
                    <select [(ngModel)]="newPayment.referenceId" name="referenceId" required class="form-control">
                      <option value="">-- Select Supplier --</option>
                      @for (sup of suppliers(); track sup.id) {
                        <option [value]="sup.id">{{ sup.name }}</option>
                      }
                    </select>
                  </div>
                } @else {
                  <div class="form-group">
                    <label>Customer ID</label>
                    <input type="text" [(ngModel)]="newPayment.referenceId" name="referenceId" required class="form-control" placeholder="Enter Customer UUID" />
                    <small class="text-secondary text-xs">Customer dropdown coming soon.</small>
                  </div>
                }

                <div class="form-group">
                  <label>Amount</label>
                  <input type="number" [(ngModel)]="newPayment.amount" name="amount" required class="form-control" min="0.01" step="0.01" />
                </div>

                <div class="form-group">
                  <label>Payment Date</label>
                  <input type="date" [(ngModel)]="newPayment.paymentDate" name="paymentDate" required class="form-control" />
                </div>

                <div class="form-group">
                  <label>Payment Method</label>
                  <select [(ngModel)]="newPayment.paymentMethod" name="paymentMethod" required class="form-control">
                    <option value="Cash">Cash</option>
                    <option value="Credit Card">Credit Card</option>
                    <option value="Bank Transfer">Bank Transfer</option>
                    <option value="Check">Check</option>
                  </select>
                </div>

                <div class="form-group">
                  <label>Reference Number (e.g. Check/Txn #)</label>
                  <input type="text" [(ngModel)]="newPayment.referenceNumber" name="referenceNumber" class="form-control" />
                </div>

                <div class="form-group">
                  <label>Notes</label>
                  <textarea [(ngModel)]="newPayment.notes" name="notes" class="form-control" rows="2"></textarea>
                </div>

                <div class="form-actions mt-4">
                  <button type="submit" class="btn btn-success" [disabled]="!paymentForm.form.valid || saving() || !newPayment.referenceId">
                    {{ saving() ? 'Saving...' : 'Record Payment' }}
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
    .text-xs { font-size: 0.75rem; }
    .text-secondary { color: var(--color-text-secondary); }
    .ref-num { font-weight: 500; }
    
    .badge {
      display: inline-block;
      padding: 0.25rem 0.5rem;
      border-radius: var(--radius-full);
      font-size: 0.75rem;
      font-weight: 500;
    }
    .badge.supplier {
      background: #fee2e2;
      color: #991b1b;
    }
    .badge.customer {
      background: #dbeafe;
      color: #1e40af;
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
export class PaymentsComponent implements OnInit {
  payments = signal<Payment[]>([]);
  suppliers = signal<Supplier[]>([]);
  
  loading = signal(true);
  showForm = signal(false);
  saving = signal(false);

  newPayment: Partial<RecordPaymentRequest> = {
    paymentType: 'SupplierPayment',
    referenceId: '',
    amount: null as unknown as number,
    paymentDate: new Date().toISOString().split('T')[0],
    paymentMethod: 'Bank Transfer',
    referenceNumber: '',
    notes: ''
  };

  constructor(
    public storeService: StoreSelectionService,
    private paymentService: PaymentService,
    private supplierService: SupplierService
  ) {
    effect(() => {
      const store = this.storeService.activeStore();
      if (store) {
        this.loadPayments();
      }
    });
  }

  ngOnInit() {
    this.supplierService.getAllSuppliers().subscribe(res => this.suppliers.set(res));
  }

  loadPayments() {
    const store = this.storeService.activeStore();
    if (!store) {
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.paymentService.getAllPayments(store.id).subscribe({
      next: (data) => {
        this.payments.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load payments', err);
        this.loading.set(false);
      }
    });
  }

  toggleForm() {
    this.showForm.update(v => !v);
  }

  onPaymentTypeChange() {
    this.newPayment.referenceId = '';
  }

  submitPayment() {
    const store = this.storeService.activeStore();
    if (!store) {
      alert('Please select a store first');
      return;
    }

    if (!this.newPayment.paymentType || !this.newPayment.referenceId || !this.newPayment.amount || !this.newPayment.paymentDate || !this.newPayment.paymentMethod) {
      return;
    }

    this.saving.set(true);
    const req: RecordPaymentRequest = {
      storeId: store.id,
      paymentType: this.newPayment.paymentType,
      referenceId: this.newPayment.referenceId,
      amount: this.newPayment.amount,
      paymentDate: new Date(this.newPayment.paymentDate).toISOString(),
      paymentMethod: this.newPayment.paymentMethod,
      referenceNumber: this.newPayment.referenceNumber,
      notes: this.newPayment.notes
    };

    this.paymentService.recordPayment(req).subscribe({
      next: (res) => {
        // Add to top of list
        this.payments.update(curr => [res, ...curr]);
        this.saving.set(false);
        this.showForm.set(false);
        // Reset form
        this.newPayment = {
          paymentType: 'SupplierPayment',
          referenceId: '',
          amount: null as unknown as number,
          paymentDate: new Date().toISOString().split('T')[0],
          paymentMethod: 'Bank Transfer',
          referenceNumber: '',
          notes: ''
        };
      },
      error: (err) => {
        console.error('Failed to save payment', err);
        alert('Failed to save payment');
        this.saving.set(false);
      }
    });
  }
}
