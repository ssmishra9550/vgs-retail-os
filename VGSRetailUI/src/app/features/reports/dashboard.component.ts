import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportService, DashboardSummaryResponse, TopProductResponse, LowStockAlertResponse } from './services/report.service';
import { StoreSelectionService } from '../../core/services/store.service';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="dashboard-container">
      <div class="dashboard-header">
        <div>
          <h1>Dashboard</h1>
          <p>Control Room - {{ currentDate | date:'fullDate' }}</p>
        </div>
        <button class="btn btn-outline" (click)="loadDashboardData()">Refresh Data</button>
      </div>

      <ng-container *ngIf="loading; else dashboardContent">
        <div class="loading-state">
          <div class="spinner"></div>
          <p>Loading analytics...</p>
        </div>
      </ng-container>

      <ng-template #dashboardContent>
        <!-- KPI Cards -->
        <div class="kpi-grid">
          <div class="kpi-card highlight-blue">
            <div class="kpi-title">Today's Sales</div>
            <div class="kpi-value">{{ summary?.todaySales | currency }}</div>
          </div>
          <div class="kpi-card highlight-green">
            <div class="kpi-title">Today's Purchases</div>
            <div class="kpi-value">{{ summary?.todayPurchases | currency }}</div>
          </div>
          <div class="kpi-card highlight-purple">
            <div class="kpi-title">Receivables</div>
            <div class="kpi-value">{{ summary?.totalReceivables | currency }}</div>
          </div>
          <div class="kpi-card highlight-orange">
            <div class="kpi-title">Payables</div>
            <div class="kpi-value">{{ summary?.totalPayables | currency }}</div>
          </div>
        </div>

        <!-- Charts / Tables section -->
        <div class="dashboard-widgets">
          <!-- Top Products -->
          <div class="widget-card">
            <div class="widget-header">
              <h2>Top Products</h2>
            </div>
            <ng-container *ngIf="topProducts && topProducts.length > 0; else noTopProducts">
              <div class="product-list">
                <div class="product-item" *ngFor="let product of topProducts">
                  <div class="product-info">
                    <span class="product-name">{{ product.productName }}</span>
                  </div>
                  <div class="product-stats">
                    <div class="stat-value">{{ product.totalRevenue | currency }}</div>
                    <div class="stat-subtext">{{ product.totalQuantitySold }} units sold</div>
                  </div>
                </div>
              </div>
            </ng-container>
            <ng-template #noTopProducts>
              <div class="empty-state">No sales data available for top products.</div>
            </ng-template>
          </div>

          <!-- LowStock Alerts -->
          <div class="widget-card">
            <div class="widget-header">
              <h2>Low Stock Alerts</h2>
              <span class="badge warning">{{ lowStockAlerts?.length || 0 }}</span>
            </div>
            <ng-container *ngIf="lowStockAlerts && lowStockAlerts.length > 0; else noAlerts">
              <table class="data-table">
                <thead>
                  <tr>
                    <th>Product</th>
                    <th>Current Stock</th>
                    <th>Min Level</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let alert of lowStockAlerts">
                    <td>
                      <div class="fw-medium">{{ alert.productName }}</div>
                    </td>
                    <td class="text-danger fw-bold">{{ alert.currentStock }}</td>
                    <td>{{ alert.reorderLevel }}</td>
                    <td><span class="status-indicator warning">Reorder</span></td>
                  </tr>
                </tbody>
              </table>
            </ng-container>
            <ng-template #noAlerts>
              <div class="empty-state">
                <i class="icon-success">✓</i>
                <p>Inventory levels are looking good!</p>
              </div>
            </ng-template>
          </div>
        </div>
      </ng-template>
    </div>
  `,
  styles: [`
    .dashboard-container {
      display: flex;
      flex-direction: column;
      gap: 2rem;
    }
    .dashboard-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-end;
    }
    .dashboard-header h1 {
      margin: 0 0 0.5rem 0;
      font-size: 1.75rem;
      color: var(--color-text);
    }
    .dashboard-header p {
      margin: 0;
      color: var(--color-text-secondary);
    }
    .kpi-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 1.5rem;
    }
    .kpi-card {
      background: white;
      border-radius: 12px;
      padding: 1.5rem;
      box-shadow: var(--shadow-sm);
      border-left: 4px solid transparent;
      transition: transform 0.2s;
    }
    .kpi-card:hover {
      transform: translateY(-2px);
      box-shadow: var(--shadow-md);
    }
    .highlight-blue { border-left-color: #3b82f6; }
    .highlight-green { border-left-color: #10b981; }
    .highlight-purple { border-left-color: #8b5cf6; }
    .highlight-orange { border-left-color: #f59e0b; }
    
    .kpi-title {
      font-size: 0.875rem;
      color: var(--color-text-secondary);
      font-weight: 500;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      margin-bottom: 0.5rem;
    }
    .kpi-value {
      font-size: 1.875rem;
      font-weight: 700;
      color: var(--color-text);
    }
    .dashboard-widgets {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.5rem;
    }
    .widget-card {
      background: white;
      border-radius: 12px;
      padding: 1.5rem;
      box-shadow: var(--shadow-sm);
    }
    .widget-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
      padding-bottom: 0.75rem;
      border-bottom: 1px solid var(--color-border);
    }
    .widget-header h2 {
      margin: 0;
      font-size: 1.25rem;
    }
    .badge {
      padding: 0.25rem 0.75rem;
      border-radius: 999px;
      font-size: 0.875rem;
      font-weight: 600;
    }
    .badge.warning {
      background: #fef3c7;
      color: #d97706;
    }
    .product-list {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    .product-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding-bottom: 1rem;
      border-bottom: 1px solid var(--color-border-light);
    }
    .product-item:last-child {
      border-bottom: none;
      padding-bottom: 0;
    }
    .product-info {
      display: flex;
      flex-direction: column;
    }
    .product-name {
      font-weight: 600;
      color: var(--color-text);
    }
    .product-sku {
      font-size: 0.875rem;
      color: var(--color-text-secondary);
    }
    .product-stats {
      text-align: right;
    }
    .stat-value {
      font-weight: 700;
      color: var(--color-text);
    }
    .stat-subtext {
      font-size: 0.875rem;
      color: var(--color-text-secondary);
    }
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 2rem 0;
      color: #10b981;
    }
    .icon-success {
      font-size: 2rem;
      margin-bottom: 0.5rem;
    }
    .loading-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 4rem;
      color: var(--color-text-secondary);
    }
    .spinner {
      border: 3px solid #f3f3f3;
      border-top: 3px solid var(--color-primary);
      border-radius: 50%;
      width: 40px;
      height: 40px;
      animation: spin 1s linear infinite;
      margin-bottom: 1rem;
    }
    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
    
    @media (max-width: 1024px) {
      .dashboard-widgets {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class DashboardComponent implements OnInit {
  currentDate = new Date();
  loading = true;
  
  summary: DashboardSummaryResponse | null = null;
  topProducts: TopProductResponse[] = [];
  lowStockAlerts: LowStockAlertResponse[] = [];
  maxRevenue = 0;

  constructor(
    private reportService: ReportService,
    private storeService: StoreSelectionService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    // 1. Force reload stores first
    this.storeService.loadStores().subscribe({
      next: (stores) => {
        if (stores && stores.length > 0) {
          this.loadDashboardData(stores[0].id);
        } else {
          console.warn('No stores found for user');
          this.loading = false;
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        console.error('Failed to load stores', err);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadDashboardData(storeId?: string) {
    if (!storeId) {
      const activeStore = this.storeService.activeStore();
      if (!activeStore) {
        this.loading = false;
        this.cdr.detectChanges();
        return;
      }
      storeId = activeStore.id;
    }

    this.loading = true;
    this.cdr.detectChanges();

    // Load data concurrently
    forkJoin({
      summary: this.reportService.getDashboardSummary(storeId),
      topProducts: this.reportService.getTopProducts(storeId),
      lowStockAlerts: this.reportService.getLowStockAlerts(storeId)
    }).subscribe({
      next: (results) => {
        this.summary = results.summary;
        this.topProducts = results.topProducts || [];
        this.lowStockAlerts = results.lowStockAlerts || [];
        
        // Calculate max revenue to safely avoid -Infinity
        if (this.topProducts.length > 0) {
          this.maxRevenue = Math.max(...this.topProducts.map(p => p.totalRevenue || 0), 0);
        } else {
          this.maxRevenue = 0;
        }

        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error fetching dashboard data:', err);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
