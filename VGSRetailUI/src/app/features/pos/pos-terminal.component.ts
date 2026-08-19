import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductGridComponent } from './components/product-grid.component';
import { CartPanelComponent } from './components/cart-panel.component';
import { StoreSelectionService } from '../../core/services/store.service';

@Component({
  selector: 'app-pos-terminal',
  standalone: true,
  imports: [CommonModule, ProductGridComponent, CartPanelComponent],
  template: `
    <div class="pos-layout">
      @if (storeService.hasActiveStore()) {
        <div class="pos-main">
          <app-product-grid></app-product-grid>
        </div>
        <div class="pos-sidebar">
          <app-cart-panel></app-cart-panel>
        </div>
      } @else {
        <div class="loading-store">
          <h2>Loading Store Selection...</h2>
          <p>Please wait while we initialize your POS terminal.</p>
        </div>
      }
    </div>
  `,
  styles: [`
    .pos-layout {
      display: flex;
      height: calc(100vh - var(--header-height)); /* Assuming header takes some space */
      width: 100%;
      background: var(--color-background);
      overflow: hidden;
    }
    .pos-main {
      flex: 1;
      height: 100%;
      overflow: hidden;
    }
    .pos-sidebar {
      width: 380px;
      height: 100%;
      flex-shrink: 0;
      box-shadow: -4px 0 15px rgba(0,0,0,0.05);
      z-index: 10;
    }
    .loading-store {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      width: 100%;
      height: 100%;
      color: var(--color-text-secondary);
    }
    
    @media (max-width: 768px) {
      .pos-layout {
        flex-direction: column;
        height: auto;
      }
      .pos-main {
        height: 60vh;
      }
      .pos-sidebar {
        width: 100%;
        height: 50vh; /* Allow scrolling */
      }
    }
  `]
})
export class PosTerminalComponent implements OnInit {
  constructor(public storeService: StoreSelectionService) {}

  ngOnInit() {
    // Load stores on initialization to ensure an active store is picked
    this.storeService.loadStores().subscribe();
  }
}
