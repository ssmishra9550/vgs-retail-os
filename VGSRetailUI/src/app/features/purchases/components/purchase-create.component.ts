import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { StoreSelectionService } from '../../../core/services/store.service';
import { PurchaseService, CreatePurchaseRequest, PurchaseItem } from '../services/purchase.service';
import { SupplierService, Supplier } from '../../suppliers/services/supplier.service';
import { ProductService, Product } from '../../pos/services/product.service';

@Component({
  selector: 'app-purchase-create',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="purchase-create-container">
      <div class="page-header">
        <h1>Create Purchase Order</h1>
        <button class="btn btn-secondary" (click)="goBack()">Cancel</button>
      </div>

      <div class="content-body">
        <div class="form-card">
          <form (ngSubmit)="saveDraft()">
            
            <div class="form-row">
              <div class="form-group">
                <label>Supplier</label>
                <select [(ngModel)]="selectedSupplierId" name="supplier" required class="form-control">
                  <option value="">-- Select Supplier --</option>
                  @for (sup of suppliers(); track sup.id) {
                    <option [value]="sup.id">{{ sup.name }}</option>
                  }
                </select>
              </div>
              
              <div class="form-group">
                <label>Invoice Number</label>
                <input type="text" [(ngModel)]="invoiceNumber" name="invoiceNumber" required class="form-control" placeholder="e.g. INV-2023-001" />
              </div>

              <div class="form-group">
                <label>Invoice Date</label>
                <input type="date" [(ngModel)]="invoiceDate" name="invoiceDate" required class="form-control" />
              </div>
            </div>

            <hr class="divider" />
            
            <div class="items-section">
              <div class="section-header">
                <h3>Line Items</h3>
                <div class="add-item-controls">
                  <select [(ngModel)]="selectedProductId" name="product" class="form-control inline-select">
                    <option value="">-- Select Product --</option>
                    @for (prod of products(); track prod.id) {
                      <option [value]="prod.id">{{ prod.name }} (Cost: {{ prod.purchasePrice | currency }})</option>
                    }
                  </select>
                  <button type="button" class="btn btn-primary" (click)="addItem()" [disabled]="!selectedProductId">Add Item</button>
                </div>
              </div>

              <table class="items-table">
                <thead>
                  <tr>
                    <th>Product</th>
                    <th style="width: 120px;">Qty</th>
                    <th style="width: 150px;">Unit Cost</th>
                    <th style="width: 120px;">Tax</th>
                    <th class="text-right">Total</th>
                    <th class="text-center" style="width: 80px;">Act</th>
                  </tr>
                </thead>
                <tbody>
                  @if (items().length === 0) {
                    <tr>
                      <td colspan="6" class="text-center py-4 text-secondary">No items added. Select a product and click "Add Item".</td>
                    </tr>
                  }
                  @for (item of items(); track item.productId; let i = $index) {
                    <tr>
                      <td>{{ getProductName(item.productId) }}</td>
                      <td>
                        <input type="number" [(ngModel)]="item.quantity" name="qty_{{i}}" class="form-control sm" min="1" (change)="recalculateItem(item)" />
                      </td>
                      <td>
                        <input type="number" [(ngModel)]="item.unitCost" name="cost_{{i}}" class="form-control sm" min="0" step="0.01" (change)="recalculateItem(item)" />
                      </td>
                      <td>
                        <input type="number" [(ngModel)]="item.taxAmount" name="tax_{{i}}" class="form-control sm" min="0" step="0.01" (change)="recalculateItem(item)" />
                      </td>
                      <td class="text-right"><strong>{{ itemTotal(item) | currency }}</strong></td>
                      <td class="text-center">
                        <button type="button" class="btn-icon text-danger" (click)="removeItem(i)">✕</button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
            
            <hr class="divider" />
            
            <div class="totals-section">
              <div class="totals-row">
                <span>Subtotal:</span>
                <span>{{ subtotal() | currency }}</span>
              </div>
              <div class="totals-row">
                <span>Header Discount:</span>
                <input type="number" [(ngModel)]="totalDiscount" name="totalDiscount" class="form-control sm text-right" min="0" step="0.01" style="width: 100px; display: inline-block; margin-left: 1rem;" />
              </div>
              <div class="totals-row">
                <span>Total Tax:</span>
                <span>{{ totalTax() | currency }}</span>
              </div>
              <div class="totals-row grand-total">
                <span>Grand Total:</span>
                <span>{{ grandTotal() | currency }}</span>
              </div>
              
              <div class="form-actions mt-4">
                <button type="submit" class="btn btn-success btn-lg" [disabled]="!canSubmit() || saving()">
                  {{ saving() ? 'Saving...' : 'Save Draft PO' }}
                </button>
              </div>
            </div>
          </form>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .purchase-create-container {
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
      padding: var(--spacing-xl);
      overflow-y: auto;
      max-width: 1000px;
      margin: 0 auto;
      width: 100%;
    }
    .form-card {
      background: var(--color-surface);
      border-radius: var(--radius-lg);
      border: 1px solid var(--color-border);
      padding: var(--spacing-xl);
      box-shadow: 0 1px 3px rgba(0,0,0,0.05);
    }
    .form-row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1.5rem;
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
      color: var(--color-text);
      font-size: 0.875rem;
    }
    .form-control.sm {
      padding: 0.5rem;
    }
    .form-control:focus {
      outline: none;
      border-color: var(--color-primary);
      box-shadow: 0 0 0 2px rgba(37,99,235,0.1);
    }
    .divider {
      border: none;
      border-top: 1px solid var(--color-border);
      margin: 2rem 0;
    }
    .section-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
    }
    .section-header h3 {
      font-size: 1.25rem;
      font-weight: 600;
      margin: 0;
    }
    .add-item-controls {
      display: flex;
      gap: 0.5rem;
      align-items: center;
    }
    .inline-select { width: 250px; }
    
    .items-table {
      width: 100%;
      border-collapse: collapse;
      margin-bottom: 1.5rem;
    }
    .items-table th, .items-table td {
      padding: 0.75rem;
      border-bottom: 1px solid var(--color-border);
      text-align: left;
    }
    .items-table th {
      background: var(--color-background);
      font-size: 0.75rem;
      text-transform: uppercase;
      color: var(--color-text-secondary);
      font-weight: 600;
    }
    .text-right { text-align: right; }
    .text-center { text-align: center; }
    .text-danger { color: var(--color-danger); }
    .py-4 { padding-top: 1.5rem; padding-bottom: 1.5rem; }
    .mt-4 { margin-top: 1.5rem; }

    .btn {
      padding: 0.5rem 1rem;
      border-radius: var(--radius-md);
      font-weight: 500;
      cursor: pointer;
      border: none;
      transition: background-color 0.2s;
    }
    .btn:disabled { opacity: 0.5; cursor: not-allowed; }
    .btn-primary { background: var(--color-primary); color: white; }
    .btn-primary:hover:not(:disabled) { background: var(--color-primary-dark); }
    .btn-success { background: #16a34a; color: white; }
    .btn-success:hover:not(:disabled) { background: #15803d; }
    .btn-secondary { background: var(--color-border); color: var(--color-text); }
    .btn-secondary:hover { background: #d1d5db; }
    .btn-lg { padding: 0.75rem 2rem; font-size: 1rem; width: 100%; }
    .btn-icon { background: none; border: none; font-size: 1.25rem; cursor: pointer; padding: 0 0.5rem; }

    .totals-section {
      width: 350px;
      margin-left: auto;
    }
    .totals-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.5rem 0;
      font-size: 0.875rem;
      color: var(--color-text-secondary);
    }
    .totals-row.grand-total {
      font-size: 1.25rem;
      font-weight: 600;
      color: var(--color-text);
      border-top: 2px solid var(--color-border);
      padding-top: 1rem;
      margin-top: 0.5rem;
    }
  `]
})
export class PurchaseCreateComponent implements OnInit {
  suppliers = signal<Supplier[]>([]);
  products = signal<Product[]>([]);
  
  // Form State
  selectedSupplierId = '';
  invoiceNumber = '';
  invoiceDate = new Date().toISOString().split('T')[0];
  items = signal<(Omit<PurchaseItem, 'id'|'total'> & { tempId: number })[]>([]);
  selectedProductId = '';
  totalDiscount = 0;
  saving = signal(false);

  private itemCounter = 0;

  subtotal = computed(() => {
    return this.items().reduce((sum, item) => sum + (item.quantity * item.unitCost), 0);
  });

  totalTax = computed(() => {
    return this.items().reduce((sum, item) => sum + item.taxAmount, 0);
  });

  grandTotal = computed(() => {
    return this.subtotal() - this.totalDiscount + this.totalTax();
  });

  constructor(
    private router: Router,
    private storeService: StoreSelectionService,
    private purchaseService: PurchaseService,
    private supplierService: SupplierService,
    private productService: ProductService
  ) {}

  ngOnInit() {
    this.supplierService.getAllSuppliers().subscribe(sups => this.suppliers.set(sups));
    this.productService.getAllProducts().subscribe(prods => this.products.set(prods));
  }

  addItem() {
    if (!this.selectedProductId) return;
    const prod = this.products().find(p => p.id === this.selectedProductId);
    if (!prod) return;

    // Check if already in list, if so just increment
    const existingIndex = this.items().findIndex(i => i.productId === prod.id);
    if (existingIndex >= 0) {
      const arr = [...this.items()];
      arr[existingIndex].quantity += 1;
      this.items.set(arr);
    } else {
      this.items.update(curr => [
        ...curr,
        {
          tempId: ++this.itemCounter,
          productId: prod.id,
          quantity: 1,
          unitCost: prod.purchasePrice,
          discount: 0,
          taxAmount: 0
        }
      ]);
    }
    this.selectedProductId = '';
  }

  removeItem(index: number) {
    const arr = [...this.items()];
    arr.splice(index, 1);
    this.items.set(arr);
  }

  recalculateItem(item: any) {
    // Angular handles two-way binding on the object properties directly
    // This is just a hook if we need to force signal updates
    this.items.set([...this.items()]);
  }

  itemTotal(item: any): number {
    return (item.quantity * item.unitCost) - item.discount + item.taxAmount;
  }

  getProductName(id: string): string {
    return this.products().find(p => p.id === id)?.name || 'Unknown';
  }

  canSubmit(): boolean {
    return !!this.selectedSupplierId && 
           !!this.invoiceNumber && 
           !!this.invoiceDate && 
           this.items().length > 0;
  }

  saveDraft() {
    if (!this.canSubmit()) return;

    const store = this.storeService.activeStore();
    if (!store) {
      alert('No active store selected');
      return;
    }

    this.saving.set(true);

    const payload: CreatePurchaseRequest = {
      storeId: store.id,
      supplierId: this.selectedSupplierId,
      invoiceNumber: this.invoiceNumber,
      invoiceDate: new Date(this.invoiceDate).toISOString(),
      totalDiscount: this.totalDiscount,
      totalTax: this.totalTax(),
      items: this.items().map(i => ({
        productId: i.productId,
        quantity: i.quantity,
        unitCost: i.unitCost,
        discount: i.discount,
        taxAmount: i.taxAmount
      }))
    };

    this.purchaseService.createDraftPurchase(payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.router.navigate(['/purchases']);
      },
      error: (err) => {
        console.error(err);
        alert('Failed to save Purchase Order');
        this.saving.set(false);
      }
    });
  }

  goBack() {
    this.router.navigate(['/purchases']);
  }
}
