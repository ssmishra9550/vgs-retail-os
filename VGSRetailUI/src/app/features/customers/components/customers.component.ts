import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CustomerService, Customer, CreateCustomerRequest } from '../services/customer.service';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Customers</h1>
        <button class="btn btn-primary" (click)="toggleForm()">
          {{ showForm() ? 'Hide Form' : 'Add New Customer' }}
        </button>
      </div>

      <div class="content-body" [class.with-form]="showForm()">
        <div class="list-section">
          @if (loading()) {
            <div class="loading-state">Loading customers...</div>
          } @else if (customers().length === 0) {
            <div class="empty-state">
              <p>No customers registered yet.</p>
            </div>
          } @else {
            <table class="data-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Mobile</th>
                  <th>Email</th>
                  <th class="text-right">Credit Balance</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                @for (cust of customers(); track cust.id) {
                  <tr>
                    <td class="font-medium">{{ cust.firstName }} {{ cust.lastName }}</td>
                    <td>{{ cust.mobile }}</td>
                    <td class="text-secondary">{{ cust.email || '-' }}</td>
                    <td class="text-right font-medium" [class.credit]="cust.creditBalance > 0">
                      {{ cust.creditBalance | currency }}
                    </td>
                    <td>
                      <span class="status-badge" [class.active]="cust.isActive">
                        {{ cust.isActive ? 'Active' : 'Inactive' }}
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
              <h3>Add New Customer</h3>
              <form (ngSubmit)="submitCustomer()" #custForm="ngForm">
                
                <div class="form-group">
                  <label>First Name *</label>
                  <input type="text" [(ngModel)]="newCustomer.firstName" name="firstName" required class="form-control" />
                </div>

                <div class="form-group">
                  <label>Last Name</label>
                  <input type="text" [(ngModel)]="newCustomer.lastName" name="lastName" class="form-control" />
                </div>

                <div class="form-group">
                  <label>Mobile Number *</label>
                  <input type="tel" [(ngModel)]="newCustomer.mobile" name="mobile" required class="form-control" />
                </div>

                <div class="form-group">
                  <label>Email Address</label>
                  <input type="email" [(ngModel)]="newCustomer.email" name="email" class="form-control" />
                </div>

                <div class="form-group">
                  <label>Address</label>
                  <textarea [(ngModel)]="newCustomer.address" name="address" class="form-control" rows="2"></textarea>
                </div>

                <div class="form-actions mt-4">
                  <button type="submit" class="btn btn-success" [disabled]="!custForm.form.valid || saving()">
                    {{ saving() ? 'Saving...' : 'Save Customer' }}
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
      align-self: flex-start;
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
    .text-secondary { color: var(--color-text-secondary); }
    .credit { color: #16a34a; } /* Highlight credit in green */
    
    .status-badge {
      display: inline-block;
      padding: 0.25rem 0.5rem;
      border-radius: var(--radius-full);
      font-size: 0.75rem;
      font-weight: 500;
      background: #f3f4f6;
      color: #374151;
    }
    .status-badge.active {
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
export class CustomersComponent implements OnInit {
  customers = signal<Customer[]>([]);
  loading = signal(true);
  showForm = signal(false);
  saving = signal(false);

  newCustomer: Partial<CreateCustomerRequest> = {
    firstName: '',
    lastName: '',
    mobile: '',
    email: '',
    address: ''
  };

  constructor(private customerService: CustomerService) {}

  ngOnInit() {
    this.loadCustomers();
  }

  loadCustomers() {
    this.loading.set(true);
    this.customerService.getAllCustomers().subscribe({
      next: (data) => {
        this.customers.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load customers', err);
        this.loading.set(false);
      }
    });
  }

  toggleForm() {
    this.showForm.update(v => !v);
  }

  submitCustomer() {
    if (!this.newCustomer.firstName || !this.newCustomer.mobile) return;

    this.saving.set(true);
    const req = this.newCustomer as CreateCustomerRequest;

    this.customerService.createCustomer(req).subscribe({
      next: (res) => {
        this.customers.update(curr => [...curr, res]);
        this.saving.set(false);
        this.showForm.set(false);
        // Reset form
        this.newCustomer = {
          firstName: '',
          lastName: '',
          mobile: '',
          email: '',
          address: ''
        };
      },
      error: (err) => {
        console.error('Failed to register customer', err);
        alert('Failed to register customer');
        this.saving.set(false);
      }
    });
  }
}
