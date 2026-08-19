import { Component, OnInit, signal, effect, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StoreSelectionService } from '../../core/services/store.service';
import { SalesHistoryService, SaleResponse } from './services/sales-history.service';
import { ProductService, Product } from '../pos/services/product.service';

@Component({
  selector: 'app-sales-history',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="history-container">
      <div class="page-header">
        <h1>Sales History & Reports</h1>
        <div class="store-info" *ngIf="storeService.activeStore()">
          Active Store: <strong>{{ storeService.activeStore()?.name }}</strong>
        </div>
      </div>

      <div class="content-body">
        @if (loading()) {
          <div class="loading-state">Loading sales history...</div>
        } @else if (error()) {
          <div class="error-state">{{ error() }}</div>
        } @else {
          <div class="table-responsive">
            <table class="data-table">
              <thead>
                <tr>
                  <th>Invoice No.</th>
                  <th>Date</th>
                  <th>Status</th>
                  <th class="text-right">Items</th>
                  <th class="text-right">Grand Total</th>
                  <th class="text-center">Action</th>
                </tr>
              </thead>
              <tbody>
                @if (sales().length === 0) {
                  <tr>
                    <td colspan="6" class="text-center">No past sales found.</td>
                  </tr>
                }
                @for (sale of sales(); track sale.id) {
                  <tr [class.expanded]="expandedSaleId() === sale.id">
                    <td><strong>{{ sale.invoiceNumber }}</strong></td>
                    <td>{{ sale.saleDate | date:'medium' }}</td>
                    <td>
                      <span class="status-badge" [ngClass]="sale.status.toLowerCase()">
                        {{ sale.status }}
                      </span>
                    </td>
                    <td class="text-right">{{ sale.items.length }}</td>
                    <td class="text-right">{{ sale.grandTotal | currency }}</td>
                    <td class="text-center">
                      <button class="btn-link" (click)="toggleDetails(sale.id)">
                        {{ expandedSaleId() === sale.id ? 'Hide Details' : 'View Details' }}
                      </button>
                    </td>
                  </tr>
                  
                  <!-- Expanded Details Row -->
                  <tr *ngIf="expandedSaleId() === sale.id" class="details-row">
                    <td colspan="6">
                      <div class="receipt-details">
                        <div class="receipt-header">
                          <h4>Receipt Details</h4>
                          <span class="text-sm text-secondary">Paid: {{ sale.paidAmount | currency }}</span>
                        </div>
                        <table class="items-table">
                          <thead>
                            <tr>
                              <th>Product</th>
                              <th class="text-right">Qty</th>
                              <th class="text-right">Unit Price</th>
                              <th class="text-right">Tax</th>
                              <th class="text-right">Total</th>
                            </tr>
                          </thead>
                          <tbody>
                            @for (item of sale.items; track item.id) {
                              <tr>
                                <td>{{ getProductName(item.productId) }}</td>
                                <td class="text-right">{{ item.quantity }}</td>
                                <td class="text-right">{{ item.unitPrice | currency }}</td>
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
    .history-container {
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
    .text-sm { font-size: 0.875rem; }
    .text-secondary { color: var(--color-text-secondary); }
    
    .status-badge {
      display: inline-block;
      padding: 0.25rem 0.75rem;
      border-radius: 999px;
      font-weight: 600;
      font-size: 0.75rem;
      text-transform: uppercase;
    }
    .status-badge.completed { background: #dcfce7; color: #166534; }
    .status-badge.returned { background: #fef08a; color: #854d0e; }
    .status-badge.cancelled { background: #fee2e2; color: #991b1b; }
    
    .btn-link {
      background: none;
      border: none;
      color: var(--color-primary);
      font-weight: 600;
      cursor: pointer;
    }
    .btn-link:hover {
      text-decoration: underline;
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
    .receipt-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
    }
    .receipt-header h4 { margin: 0; font-size: 1rem; color: var(--color-text); }
    
    .items-table th, .items-table td {
      padding: 0.75rem;
      border-bottom: 1px solid var(--color-border);
      font-size: 0.875rem;
    }
    .items-table th {
      background: transparent;
      border-bottom: 2px solid var(--color-border);
    }

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
export class SalesHistoryComponent implements OnInit {
  sales = signal<SaleResponse[]>([]);
  products = signal<Map<string, Product>>(new Map());
  loading = signal(true);
  error = signal<string | null>(null);
  expandedSaleId = signal<string | null>(null);

  constructor(
    public storeService: StoreSelectionService,
    private salesService: SalesHistoryService,
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
    this.loadProducts();
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
    this.expandedSaleId.set(null);

    this.salesService.getSalesHistory(storeId).subscribe({
      next: (data) => {
        this.sales.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.error.set('Failed to load sales history');
        this.loading.set(false);
      }
    });
  }

  toggleDetails(saleId: string) {
    if (this.expandedSaleId() === saleId) {
      this.expandedSaleId.set(null);
    } else {
      this.expandedSaleId.set(saleId);
    }
  }

  getProductName(productId: string): string {
    return this.products().get(productId)?.name || 'Unknown Product';
  }
}
