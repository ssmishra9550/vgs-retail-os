import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Product } from './product.service';
import { StoreSelectionService } from '../../../core/services/store.service';

export interface CartItem {
  product: Product;
  quantity: number;
  discount: number;
}

export interface SaleResponse {
  id: string;
  invoiceNumber: string;
  totalAmount: number;
  status: string;
}

@Injectable({
  providedIn: 'root'
})
export class PosService {
  private cartItemsSignal = signal<CartItem[]>([]);
  public cartItems = this.cartItemsSignal.asReadonly();

  public subtotal = computed(() => {
    return this.cartItemsSignal().reduce((sum, item) => sum + (item.product.sellingPrice * item.quantity), 0);
  });

  public totalDiscount = computed(() => {
    return this.cartItemsSignal().reduce((sum, item) => sum + item.discount, 0);
  });

  // Flat 10% tax for MVP if applicable, or 0
  public totalTax = computed(() => {
    return this.subtotal() * 0.10; 
  });

  public grandTotal = computed(() => {
    return this.subtotal() - this.totalDiscount() + this.totalTax();
  });

  constructor(private http: HttpClient, private storeService: StoreSelectionService) {}

  addItem(product: Product) {
    this.cartItemsSignal.update(items => {
      const existing = items.find(i => i.product.id === product.id);
      if (existing) {
        return items.map(i => i.product.id === product.id ? { ...i, quantity: i.quantity + 1 } : i);
      }
      return [...items, { product, quantity: 1, discount: 0 }];
    });
  }

  updateQuantity(productId: string, quantity: number) {
    this.cartItemsSignal.update(items => {
      if (quantity <= 0) {
        return items.filter(i => i.product.id !== productId);
      }
      return items.map(i => i.product.id === productId ? { ...i, quantity } : i);
    });
  }

  removeItem(productId: string) {
    this.cartItemsSignal.update(items => items.filter(i => i.product.id !== productId));
  }

  clearCart() {
    this.cartItemsSignal.set([]);
  }

  saveDraft(): Observable<SaleResponse> {
    const storeId = this.storeService.getActiveStoreId();
    if (!storeId) throw new Error('No active store selected');

    const payload = this.buildSalePayload(storeId);
    return this.http.post<SaleResponse>(`${environment.apiUrl}/sales/drafts`, payload);
  }

  completeSale(saleId?: string): Observable<SaleResponse> {
    if (saleId) {
      return this.http.post<SaleResponse>(`${environment.apiUrl}/sales/${saleId}/complete`, {});
    } else {
      // Create draft then complete
      const storeId = this.storeService.getActiveStoreId();
      if (!storeId) throw new Error('No active store selected');

      const payload = this.buildSalePayload(storeId);
      payload.paidAmount = this.grandTotal(); // Fully paid
      
      // In a real app, this might be a single "create completed" endpoint. 
      // Our API supports Create Draft -> Complete. We'll do it in two steps or assume Create Draft saves it.
      // Wait, let's just assume we create a draft and then complete it.
      throw new Error('For MVP, save draft first, then complete it using the returned ID');
    }
  }

  private buildSalePayload(storeId: string) {
    return {
      storeId: storeId,
      invoiceNumber: `INV-${Date.now()}`,
      saleDate: new Date().toISOString(),
      totalDiscount: this.totalDiscount(),
      totalTax: this.totalTax(),
      paidAmount: 0,
      items: this.cartItemsSignal().map(i => ({
        productId: i.product.id,
        quantity: i.quantity,
        unitPrice: i.product.sellingPrice,
        discount: i.discount,
        taxAmount: (i.product.sellingPrice * i.quantity) * 0.10
      }))
    };
  }
}
