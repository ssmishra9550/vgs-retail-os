import { Component, OnInit, signal, effect, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { StoreSelectionService } from '../../../core/services/store.service';
import { PurchaseService, Purchase } from '../services/purchase.service';
import { SupplierService, Supplier } from '../../suppliers/services/supplier.service';
import { ProductService, Product } from '../../pos/services/product.service';

@Component({
  selector: 'app-purchase-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="purchase-container">
      <div class="page-header">
        <h1>Purchase Orders</h1>
        <div class="header-actions">
          <div class="store-info" *ngIf="storeService.activeStore()">
            Active Store: <strong>{{ storeService.activeStore()?.name }}</strong>
          </div>
          <a routerLink="new" class="btn btn-primary">Create Purchase Order</a>
        </div>
      </div>

      <div class="content-body">
        @if (loading()) {
          <div class="loading-state">Loading purchases...</div>
        } @else if (error()) {
          <div class="error-state">{{ error() }}</div>
        } @else {
          <div class="table-responsive">
            <table class="data-table">
              <thead>
                <tr>
                  <th>Invoice No.</th>
                  <th>Date</th>
                  <th>Supplier</th>
                  <th>Status</th>
                  <th class="text-right">Grand Total</th>
                  <th class="text-center">Actions</th>
                </tr>
              </thead>
              <tbody>
                @if (purchases().length === 0) {
                  <tr>
                    <td colspan="6" class="text-center text-secondary py-4">No purchases found.</td>
                  </tr>
                }
                @for (purchase of purchases(); track purchase.id) {
                  <tr [class.expanded]="expandedPurchaseId() === purchase.id">
                    <td><strong>{{ purchase.invoiceNumber }}</strong></td>
                    <td>{{ purchase.invoiceDate | date:'shortDate' }}</td>
                    <td>{{ getSupplierName(purchase.supplierId) }}</td>
                    <td>
                      <span class="status-badge" [ngClass]="purchase.status.toLowerCase()">
                        {{ purchase.status }}
                      </span>
                    </td>
                    <td class="text-right">{{ purchase.grandTotal | currency }}</td>
                    <td class="text-center actions-cell">
                      <button class="btn-link" (click)="toggleDetails(purchase.id)">
                        {{ expandedPurchaseId() === purchase.id ? 'Hide Details' : 'View Details' }}
                      </button>
                      <button *ngIf="purchase.status === 'Draft'" class="btn btn-success btn-sm ml-2" (click)="receivePurchase(purchase.id)">
                        Receive Items
                      </button>
                    </td>
                  </tr>
                  
                  <!-- Expanded Details Row -->
                  <tr *ngIf="expandedPurchaseId() === purchase.id" class="details-row">
                    <td colspan="6">
                      <div class="receipt-details">
                        <div class="receipt-header">
                          <h4>Order Items</h4>
                        </div>
                        <table class="items-table">
                          <thead>
                            <tr>
                              <th>Product</th>
                              <th class="text-right">Qty</th>
                              <th class="text-right">Unit Cost</th>
                              <th class="text-right">Tax</th>
                              <th class="text-right">Total</th>
                            </tr>
                          </thead>
                          <tbody>
                            @for (item of purchase.items; track item.id) {
                              <tr>
                                <td>{{ getProductName(item.productId) }}</td>
                                <td class="text-right">{{ item.quantity }}</td>
                                <td class="text-right">{{ item.unitCost | currency }}</td>
                                <td class="text-right">{{ item.taxAmount | currency }}</td>
                                <td class="text-right"><strong>{{ item.total | currency }}</strong></td>
                              </tr>
                            }
                          </tbody>
                        </table>
                      </div>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .purchase-container {
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
    .header-actions {
      display: flex;
      gap: 1rem;
      align-items: center;
    }
    .store-info {
      font-size: 0.875rem;
      color: var(--color-text-secondary);
      background: var(--color-background);
      padding: 0.5rem 1rem;
      border-radius: var(--radius-md);
      border: 1px solid var(--color-border);
    }
    .content-body {
      flex: 1;
      padding: var(--spacing-xl);
      overflow-y: auto;
    }
    .table-responsive {
      background: var(--color-surface);
      border-radius: var(--radius-lg);
      border: 1px solid var(--color-border);
      overflow-x: auto;
    }
    .data-table, .items-table {
      width: 100%;
      border-collapse: collapse;
      text-align: left;
    }
    .data-table th, .data-table td {
      padding: 1rem;
      border-bottom: 1px solid var(--color-border);
    }
    .data-table th {
      background: var(--color-background);
      font-weight: 600;
      color: var(--color-text-secondary);
      font-size: 0.875rem;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .data-table tbody tr:hover:not(.details-row) {
      background: var(--color-background);
    }
    .data-table tr.expanded td {
      border-bottom: none;
      background: var(--color-background);
    }
    .text-right { text-align: right; }
    .text-center { text-align: center; }
    .py-4 { padding-top: 1.5rem; padding-bottom: 1.5rem; }
    .ml-2 { margin-left: 0.5rem; }
    
    .status-badge {
      display: inline-block;
      padding: 0.25rem 0.75rem;
      border-radius: 999px;
      font-weight: 600;
      font-size: 0.75rem;
      text-transform: uppercase;
    }
    .status-badge.received { background: #dcfce7; color: #166534; }
    .status-badge.draft { background: #fef08a; color: #854d0e; }
    
    .btn {
      padding: 0.5rem 1rem;
      border-radius: var(--radius-md);
      font-weight: 500;
      cursor: pointer;
      border: none;
      text-decoration: none;
      display: inline-block;
      transition: background-color 0.2s;
    }
    .btn-primary { background: var(--color-primary); color: white; }
    .btn-primary:hover { background: var(--color-primary-dark); }
    .btn-success { background: #16a34a; color: white; }
    .btn-success:hover { background: #15803d; }
    .btn-sm { padding: 0.25rem 0.5rem; font-size: 0.875rem; }

    .btn-link {
      background: none;
      border: none;
      color: var(--color-primary);
      font-weight: 600;
      cursor: pointer;
    }
    .btn-link:hover { text-decoration: underline; }

    .actions-cell {
      white-space: nowrap;
    }

    .details-row td {
      padding: 0 1rem 1rem 1rem !important;
      background: var(--color-background);
    }
    .receipt-details {
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      padding: 1rem;
      box-shadow: 0 1px 3px rgba(0,0,0,0.05);
    }
    .receipt-header h4 { margin: 0 0 1rem 0; font-size: 1rem; color: var(--color-text); }
    
    .items-table th, .items-table td {
      padding: 0.75rem;
      border-bottom: 1px solid var(--color-border);
      font-size: 0.875rem;
    }
    .items-table th { background: transparent; border-bottom: 2px solid var(--color-border); }

    .loading-state, .error-state {
      padding: 3rem;
      text-align: center;
      color: var(--color-text-secondary);
      background: var(--color-surface);
      border-radius: var(--radius-lg);
      border: 1px solid var(--color-border);
    }
    .error-state { color: var(--color-danger); }
  `]
})
export class PurchaseListComponent implements OnInit {
  purchases = signal<Purchase[]>([]);
  suppliers = signal<Map<string, Supplier>>(new Map());
  products = signal<Map<string, Product>>(new Map());
  loading = signal(true);
  error = signal<string | null>(null);
  expandedPurchaseId = signal<string | null>(null);

  constructor(
    public storeService: StoreSelectionService,
    private purchaseService: PurchaseService,
    private supplierService: SupplierService,
    private productService: ProductService
  ) {
    effect(() => {
      const activeStore = this.storeService.activeStore();
      if (activeStore) {
        this.loadData(activeStore.id);
      }
    });
  }

  ngOnInit() {
    this.storeService.loadStores().subscribe();
    this.loadSuppliers();
    this.loadProducts();
  }

  private loadSuppliers() {
    this.supplierService.getAllSuppliers().subscribe(sups => {
      const map = new Map<string, Supplier>();
      sups.forEach(s => map.set(s.id, s));
      this.suppliers.set(map);
    });
  }

  private loadProducts() {
    this.productService.getAllProducts().subscribe(prods => {
      const map = new Map<string, Product>();
      prods.forEach(p => map.set(p.id, p));
      this.products.set(map);
    });
  }

  private loadData(storeId: string) {
    this.loading.set(true);
    this.error.set(null);
    this.expandedPurchaseId.set(null);

    this.purchaseService.getAllPurchases(storeId).subscribe({
      next: (data) => {
        this.purchases.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.error.set('Failed to load purchases');
        this.loading.set(false);
      }
    });
  }

  toggleDetails(id: string) {
    if (this.expandedPurchaseId() === id) {
      this.expandedPurchaseId.set(null);
    } else {
      this.expandedPurchaseId.set(id);
    }
  }

  receivePurchase(purchaseId: string) {
    if (confirm('Are you sure you want to receive this purchase? This will permanently update inventory levels.')) {
      this.purchaseService.receivePurchase(purchaseId).subscribe({
        next: (updated) => {
          this.purchases.update(items => items.map(p => p.id === updated.id ? updated : p));
        },
        error: (err) => {
          console.error(err);
          alert('Failed to receive purchase.');
        }
      });
    }
  }

  getSupplierName(supplierId: string): string {
    return this.suppliers().get(supplierId)?.name || 'Unknown Supplier';
  }

  getProductName(productId: string): string {
    return this.products().get(productId)?.name || 'Unknown Product';
  }
}
