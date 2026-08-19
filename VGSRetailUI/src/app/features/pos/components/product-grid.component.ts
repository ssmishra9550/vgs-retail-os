import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductService, Product } from '../services/product.service';
import { PosService } from '../services/pos.service';

@Component({
  selector: 'app-product-grid',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="product-grid">
      @if (loading()) {
        <div class="loading-state">Loading products...</div>
      } @else if (error()) {
        <div class="error-state">{{ error() }}</div>
      } @else {
        @for (product of products(); track product.id) {
          <div class="product-card" (click)="addToCart(product)">
            <div class="product-info">
              <h3 class="product-name">{{ product.name }}</h3>
              <span class="product-price">{{ product.sellingPrice | currency }}</span>
            </div>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .product-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
      gap: 1rem;
      padding: 1rem;
      height: 100%;
      overflow-y: auto;
    }
    .product-card {
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      padding: 1rem;
      cursor: pointer;
      transition: all var(--transition-fast);
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      min-height: 120px;
    }
    .product-card:hover {
      border-color: var(--color-primary);
      box-shadow: var(--shadow-md);
      transform: translateY(-2px);
    }
    .product-name {
      font-size: 1rem;
      font-weight: 500;
      color: var(--color-text);
      margin-bottom: 0.5rem;
    }
    .product-price {
      font-size: 1.125rem;
      font-weight: 600;
      color: var(--color-primary);
    }
    .loading-state, .error-state {
      grid-column: 1 / -1;
      padding: 2rem;
      text-align: center;
      color: var(--color-text-secondary);
    }
    .error-state {
      color: var(--color-danger);
    }
  `]
})
export class ProductGridComponent implements OnInit {
  products = signal<Product[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  constructor(
    private productService: ProductService,
    private posService: PosService
  ) {}

  ngOnInit() {
    this.productService.getAllProducts().subscribe({
      next: (data) => {
        this.products.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load products');
        this.loading.set(false);
        console.error(err);
      }
    });
  }

  addToCart(product: Product) {
    this.posService.addItem(product);
  }
}
