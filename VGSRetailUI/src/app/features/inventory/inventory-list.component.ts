import { Component, OnInit, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductService, Product } from '../pos/services/product.service';
import { InventoryService, StockBalance } from './services/inventory.service';
import { StoreSelectionService } from '../../core/services/store.service';
import { forkJoin } from 'rxjs';

interface InventoryRow extends Product {
  currentStock: number;
  lastUpdated?: string;
}

@Component({
  selector: 'app-inventory-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="inventory-container">
      <div class="page-header">
        <h1>Inventory Management</h1>
        <div class="store-info" *ngIf="storeService.activeStore()">
          Active Store: <strong>{{ storeService.activeStore()?.name }}</strong>
        </div>
      </div>

      <div class="content-body">
        @if (loading()) {
          <div class="loading-state">Loading inventory data...</div>
        } @else if (error()) {
          <div class="error-state">{{ error() }}</div>
        } @else {
          <div class="table-responsive">
            <table class="data-table">
              <thead>
                <tr>
                  <th>Product Name</th>
                  <th>SKU</th>
                  <th>Category</th>
                  <th>Unit</th>
                  <th class="text-right">Current Stock</th>
                  <th>Last Updated</th>
                </tr>
              </thead>
              <tbody>
                @if (inventoryData().length === 0) {
                  <tr>
                    <td colspan="6" class="text-center">No products found.</td>
                  </tr>
                }
                @for (row of inventoryData(); track row.id) {
                  <tr>
                    <td>{{ row.name }}</td>
                    <td>{{ row.sku || '-' }}</td>
                    <td>{{ row.categoryName || '-' }}</td>
                    <td>{{ row.unitName || '-' }}</td>
                    <td class="text-right">
                      <span class="stock-badge" [class.low-stock]="row.currentStock <= 5">
                        {{ row.currentStock }}
                      </span>
                    </td>
                    <td>{{ row.lastUpdated ? (row.lastUpdated | date:'short') : 'Never' }}</td>
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
    .inventory-container {
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
    .data-table {
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
    .data-table tbody tr:last-child td {
      border-bottom: none;
    }
    .data-table tbody tr:hover {
      background: var(--color-background);
    }
    .text-right {
      text-align: right;
    }
    .text-center {
      text-align: center;
    }
    .stock-badge {
      display: inline-block;
      padding: 0.25rem 0.75rem;
      border-radius: 999px;
      font-weight: 600;
      background: #dcfce7;
      color: #166534;
    }
    .stock-badge.low-stock {
      background: #fee2e2;
      color: #991b1b;
    }
    .loading-state, .error-state {
      padding: 3rem;
      text-align: center;
      color: var(--color-text-secondary);
      background: var(--color-surface);
      border-radius: var(--radius-lg);
      border: 1px solid var(--color-border);
    }
    .error-state {
      color: var(--color-danger);
    }
  `]
})
export class InventoryListComponent implements OnInit {
  inventoryData = signal<InventoryRow[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  constructor(
    private productService: ProductService,
    private inventoryService: InventoryService,
    public storeService: StoreSelectionService
  ) {
    // Automatically reload data when the active store changes
    effect(() => {
      const activeStore = this.storeService.activeStore();
      if (activeStore) {
        this.loadData(activeStore.id);
      }
    });
  }

  ngOnInit() {
    // Ensure stores are loaded if navigating here directly
    this.storeService.loadStores().subscribe();
  }

  private loadData(storeId: string) {
    this.loading.set(true);
    this.error.set(null);

    forkJoin({
      products: this.productService.getAllProducts(),
      balances: this.inventoryService.getAllStockBalances(storeId)
    }).subscribe({
      next: ({ products, balances }) => {
        const balanceMap = new Map<string, StockBalance>();
        balances.forEach(b => balanceMap.set(b.productId, b));

        const mergedData: InventoryRow[] = products.map(product => {
          const balance = balanceMap.get(product.id);
          return {
            ...product,
            currentStock: balance ? balance.quantity : 0,
            lastUpdated: balance ? balance.lastUpdated : undefined
          };
        });

        this.inventoryData.set(mergedData);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load inventory data');
        this.loading.set(false);
        console.error(err);
      }
    });
  }
}
