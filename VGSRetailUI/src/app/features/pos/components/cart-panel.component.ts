import { Component, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PosService } from '../services/pos.service';
import { StoreSelectionService } from '../../../core/services/store.service';

@Component({
  selector: 'app-cart-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="cart-panel">
      <div class="cart-header">
        <h2>Current Sale</h2>
        <span class="active-store" *ngIf="storeService.activeStore()">
          {{ storeService.activeStore()?.name }}
        </span>
      </div>

      <div class="cart-items">
        @if (posService.cartItems().length === 0) {
          <div class="empty-cart">No items in cart</div>
        } @else {
          @for (item of posService.cartItems(); track item.product.id) {
            <div class="cart-item">
              <div class="item-details">
                <span class="item-name">{{ item.product.name }}</span>
                <span class="item-price">{{ item.product.sellingPrice | currency }}</span>
              </div>
              <div class="item-actions">
                <button class="btn-qty" (click)="updateQty(item.product.id, item.quantity - 1)">-</button>
                <input type="number" class="qty-input" [value]="item.quantity" 
                       (change)="onQtyChange(item.product.id, $event)">
                <button class="btn-qty" (click)="updateQty(item.product.id, item.quantity + 1)">+</button>
                <button class="btn-remove" (click)="posService.removeItem(item.product.id)">✕</button>
              </div>
            </div>
          }
        }
      </div>

      <div class="cart-summary">
        <div class="summary-row">
          <span>Subtotal</span>
          <span>{{ posService.subtotal() | currency }}</span>
        </div>
        <div class="summary-row">
          <span>Discount</span>
          <span>{{ posService.totalDiscount() | currency }}</span>
        </div>
        <div class="summary-row">
          <span>Tax (10%)</span>
          <span>{{ posService.totalTax() | currency }}</span>
        </div>
        <div class="summary-row grand-total">
          <span>Total</span>
          <span>{{ posService.grandTotal() | currency }}</span>
        </div>
      </div>

      <div class="cart-actions">
        <button class="btn btn-outline" (click)="saveDraft()" [disabled]="posService.cartItems().length === 0">
          Save Draft
        </button>
        <button class="btn btn-primary" (click)="completeSale()" [disabled]="posService.cartItems().length === 0">
          Complete Sale
        </button>
      </div>
    </div>
  `,
  styles: [`
    .cart-panel {
      display: flex;
      flex-direction: column;
      height: 100%;
      background: var(--color-surface);
      border-left: 1px solid var(--color-border);
    }
    .cart-header {
      padding: 1rem;
      border-bottom: 1px solid var(--color-border);
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .cart-header h2 {
      font-size: 1.125rem;
      font-weight: 600;
    }
    .active-store {
      font-size: 0.875rem;
      color: var(--color-text-secondary);
      background: var(--color-background);
      padding: 0.25rem 0.5rem;
      border-radius: var(--radius-sm);
    }
    .cart-items {
      flex: 1;
      overflow-y: auto;
      padding: 1rem;
    }
    .empty-cart {
      text-align: center;
      color: var(--color-text-secondary);
      padding: 2rem 0;
    }
    .cart-item {
      display: flex;
      flex-direction: column;
      padding: 0.75rem 0;
      border-bottom: 1px dashed var(--color-border);
    }
    .item-details {
      display: flex;
      justify-content: space-between;
      margin-bottom: 0.5rem;
    }
    .item-name {
      font-weight: 500;
    }
    .item-actions {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .btn-qty {
      width: 28px;
      height: 28px;
      border: 1px solid var(--color-border);
      background: var(--color-background);
      border-radius: var(--radius-sm);
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .btn-qty:hover {
      background: var(--color-border);
    }
    .qty-input {
      width: 40px;
      text-align: center;
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      padding: 0.25rem;
      -moz-appearance: textfield;
    }
    .qty-input::-webkit-outer-spin-button,
    .qty-input::-webkit-inner-spin-button {
      -webkit-appearance: none;
      margin: 0;
    }
    .btn-remove {
      margin-left: auto;
      background: transparent;
      border: none;
      color: var(--color-danger);
      cursor: pointer;
      padding: 0.25rem;
    }
    .cart-summary {
      padding: 1rem;
      border-top: 1px solid var(--color-border);
      background: var(--color-background);
    }
    .summary-row {
      display: flex;
      justify-content: space-between;
      margin-bottom: 0.5rem;
      color: var(--color-text-secondary);
      font-size: 0.875rem;
    }
    .grand-total {
      margin-top: 1rem;
      padding-top: 1rem;
      border-top: 1px dashed var(--color-border);
      color: var(--color-text);
      font-size: 1.25rem;
      font-weight: 700;
    }
    .cart-actions {
      padding: 1rem;
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 0.5rem;
      border-top: 1px solid var(--color-border);
    }
  `]
})
export class CartPanelComponent {
  constructor(
    public posService: PosService,
    public storeService: StoreSelectionService
  ) {}

  updateQty(productId: string, qty: number) {
    this.posService.updateQuantity(productId, qty);
  }

  onQtyChange(productId: string, event: Event) {
    const value = (event.target as HTMLInputElement).value;
    const qty = parseInt(value, 10);
    if (!isNaN(qty)) {
      this.posService.updateQuantity(productId, qty);
    }
  }

  saveDraft() {
    this.posService.saveDraft().subscribe({
      next: (res) => {
        alert(`Draft Saved: ${res.invoiceNumber}`);
        this.posService.clearCart();
      },
      error: (err) => alert('Failed to save draft: ' + err.message)
    });
  }

  completeSale() {
    if (confirm(`Complete sale for ${this.posService.grandTotal().toFixed(2)}?`)) {
      this.posService.completeSale().subscribe({
        next: (res) => {
          alert(`Sale Completed: ${res.invoiceNumber}`);
          this.posService.clearCart();
        },
        error: (err) => alert('Failed to complete sale: ' + err.message)
      });
    }
  }
}
