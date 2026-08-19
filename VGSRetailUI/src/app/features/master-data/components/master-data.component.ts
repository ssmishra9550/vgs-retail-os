import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoryService, Category, CreateCategoryRequest } from '../services/category.service';
import { BrandService, Brand, CreateBrandRequest } from '../services/brand.service';
import { TaxService, Tax, CreateTaxRequest } from '../services/tax.service';
import { UnitService, Unit, CreateUnitRequest } from '../services/unit.service';

type Tab = 'categories' | 'brands' | 'taxes' | 'units';

@Component({
  selector: 'app-master-data',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Master Data Management</h1>
      </div>

      <div class="tabs-container">
        <button class="tab-btn" [class.active]="activeTab() === 'categories'" (click)="setTab('categories')">Categories</button>
        <button class="tab-btn" [class.active]="activeTab() === 'brands'" (click)="setTab('brands')">Brands</button>
        <button class="tab-btn" [class.active]="activeTab() === 'taxes'" (click)="setTab('taxes')">Taxes</button>
        <button class="tab-btn" [class.active]="activeTab() === 'units'" (click)="setTab('units')">Units</button>
      </div>

      <div class="content-body">
        <div class="list-section">
          @if (loading()) {
            <div class="loading-state">Loading data...</div>
          } @else {
            <table class="data-table">
              <thead>
                <!-- Headers depend on active tab -->
                @if (activeTab() === 'categories' || activeTab() === 'brands') {
                  <tr>
                    <th>Name</th>
                    <th>Description</th>
                    <th>Status</th>
                  </tr>
                } @else if (activeTab() === 'taxes') {
                  <tr>
                    <th>Name</th>
                    <th>Rate (%)</th>
                    <th>Type</th>
                    <th>Status</th>
                  </tr>
                } @else if (activeTab() === 'units') {
                  <tr>
                    <th>Name</th>
                    <th>Short Name</th>
                    <th>Status</th>
                  </tr>
                }
              </thead>
              <tbody>
                <!-- Categories -->
                @if (activeTab() === 'categories') {
                  @for (cat of categories(); track cat.id) {
                    <tr>
                      <td class="font-medium">{{ cat.name }}</td>
                      <td class="text-secondary">{{ cat.description || '-' }}</td>
                      <td>
                        <span class="status-badge" [class.active]="cat.isActive">{{ cat.isActive ? 'Active' : 'Inactive' }}</span>
                      </td>
                    </tr>
                  }
                }
                
                <!-- Brands -->
                @if (activeTab() === 'brands') {
                  @for (brand of brands(); track brand.id) {
                    <tr>
                      <td class="font-medium">{{ brand.name }}</td>
                      <td class="text-secondary">{{ brand.description || '-' }}</td>
                      <td>
                        <span class="status-badge" [class.active]="brand.isActive">{{ brand.isActive ? 'Active' : 'Inactive' }}</span>
                      </td>
                    </tr>
                  }
                }

                <!-- Taxes -->
                @if (activeTab() === 'taxes') {
                  @for (tax of taxes(); track tax.id) {
                    <tr>
                      <td class="font-medium">{{ tax.name }}</td>
                      <td>{{ tax.rate }}</td>
                      <td>{{ tax.type }}</td>
                      <td>
                        <span class="status-badge" [class.active]="tax.isActive">{{ tax.isActive ? 'Active' : 'Inactive' }}</span>
                      </td>
                    </tr>
                  }
                }

                <!-- Units -->
                @if (activeTab() === 'units') {
                  @for (unit of units(); track unit.id) {
                    <tr>
                      <td class="font-medium">{{ unit.name }}</td>
                      <td>{{ unit.shortName }}</td>
                      <td>
                        <span class="status-badge" [class.active]="unit.isActive">{{ unit.isActive ? 'Active' : 'Inactive' }}</span>
                      </td>
                    </tr>
                  }
                }

                @if (!hasData()) {
                  <tr>
                    <td colspan="4" class="text-center py-4 text-secondary">No records found.</td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </div>

        <!-- Form Section -->
        <div class="form-section">
          <div class="form-card">
            <h3>Add New {{ getActiveTabTitle() }}</h3>
            <form (ngSubmit)="submitForm()" #mdForm="ngForm">
              
              <!-- Common Name Field -->
              <div class="form-group">
                <label>Name</label>
                <input type="text" [(ngModel)]="formData.name" name="name" required class="form-control" />
              </div>

              <!-- Category / Brand Description -->
              @if (activeTab() === 'categories' || activeTab() === 'brands') {
                <div class="form-group">
                  <label>Description</label>
                  <textarea [(ngModel)]="formData.description" name="description" class="form-control" rows="3"></textarea>
                </div>
              }

              <!-- Tax specific fields -->
              @if (activeTab() === 'taxes') {
                <div class="form-group">
                  <label>Rate (%)</label>
                  <input type="number" [(ngModel)]="formData.rate" name="rate" required class="form-control" min="0" step="0.01" />
                </div>
                <div class="form-group">
                  <label>Type</label>
                  <select [(ngModel)]="formData.type" name="type" required class="form-control">
                    <option value="Percentage">Percentage</option>
                    <option value="FixedAmount">Fixed Amount</option>
                  </select>
                </div>
              }

              <!-- Unit specific fields -->
              @if (activeTab() === 'units') {
                <div class="form-group">
                  <label>Short Name (e.g., kg, pcs)</label>
                  <input type="text" [(ngModel)]="formData.shortName" name="shortName" required class="form-control" />
                </div>
              }

              <div class="form-actions mt-4">
                <button type="submit" class="btn btn-primary" [disabled]="!mdForm.form.valid || saving()">
                  {{ saving() ? 'Saving...' : 'Add ' + getActiveTabTitle() }}
                </button>
              </div>

            </form>
          </div>
        </div>
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
    
    .tabs-container {
      display: flex;
      padding: 0 var(--spacing-xl);
      background: var(--color-surface);
      border-bottom: 1px solid var(--color-border);
      gap: 2rem;
    }
    .tab-btn {
      padding: 1rem 0;
      background: none;
      border: none;
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--color-text-secondary);
      cursor: pointer;
      position: relative;
    }
    .tab-btn:hover { color: var(--color-text); }
    .tab-btn.active { color: var(--color-primary); font-weight: 600; }
    .tab-btn.active::after {
      content: '';
      position: absolute;
      bottom: -1px;
      left: 0;
      right: 0;
      height: 2px;
      background: var(--color-primary);
    }

    .content-body {
      display: flex;
      padding: var(--spacing-xl);
      gap: 1.5rem;
      overflow-y: auto;
      flex: 1;
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
      width: 350px;
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
      font-size: 1.125rem;
    }

    .data-table {
      width: 100%;
      border-collapse: collapse;
    }
    .data-table th, .data-table td {
      padding: 1rem;
      text-align: left;
      border-bottom: 1px solid var(--color-border);
      font-size: 0.875rem;
    }
    .data-table th {
      background: #f9fafb;
      font-size: 0.75rem;
      text-transform: uppercase;
      color: var(--color-text-secondary);
      font-weight: 600;
    }
    .font-medium { font-weight: 500; }
    .text-secondary { color: var(--color-text-secondary); }
    .text-center { text-align: center; }
    .py-4 { padding-top: 1.5rem; padding-bottom: 1.5rem; }
    
    .status-badge {
      display: inline-block;
      padding: 0.25rem 0.5rem;
      border-radius: var(--radius-full);
      font-size: 0.75rem;
      font-weight: 500;
      background: #f3f4f6;
      color: #374151;
    }
    .status-badge.active { background: #dcfce7; color: #166534; }

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
    .form-control:focus { outline: none; border-color: var(--color-primary); }
    
    .btn {
      padding: 0.5rem 1rem;
      border-radius: var(--radius-md);
      font-weight: 500;
      cursor: pointer;
      border: none;
      transition: all 0.2s;
    }
    .btn:disabled { opacity: 0.5; cursor: not-allowed; }
    .btn-primary { background: var(--color-primary); color: white; width: 100%; padding: 0.75rem; }
    .btn-primary:hover:not(:disabled) { background: var(--color-primary-dark); }
    
    .mt-4 { margin-top: 1rem; }
    .loading-state {
      padding: 3rem;
      text-align: center;
      color: var(--color-text-secondary);
    }
  `]
})
export class MasterDataComponent implements OnInit {
  activeTab = signal<Tab>('categories');
  
  categories = signal<Category[]>([]);
  brands = signal<Brand[]>([]);
  taxes = signal<Tax[]>([]);
  units = signal<Unit[]>([]);

  loading = signal(false);
  saving = signal(false);

  // Generic form data object
  formData: any = {};

  constructor(
    private categoryService: CategoryService,
    private brandService: BrandService,
    private taxService: TaxService,
    private unitService: UnitService
  ) {}

  ngOnInit() {
    this.loadData();
  }

  setTab(tab: Tab) {
    this.activeTab.set(tab);
    this.resetForm();
    this.loadData();
  }

  getActiveTabTitle(): string {
    const tab = this.activeTab();
    return tab.charAt(0).toUpperCase() + tab.slice(1, -1); // Remove 's' at the end loosely
  }

  hasData(): boolean {
    const tab = this.activeTab();
    if (tab === 'categories') return this.categories().length > 0;
    if (tab === 'brands') return this.brands().length > 0;
    if (tab === 'taxes') return this.taxes().length > 0;
    if (tab === 'units') return this.units().length > 0;
    return false;
  }

  resetForm() {
    this.formData = {
      name: '',
      description: '',
      rate: null,
      type: 'Percentage',
      shortName: ''
    };
  }

  loadData() {
    this.loading.set(true);
    const tab = this.activeTab();
    
    if (tab === 'categories') {
      this.categoryService.getAllCategories().subscribe({
        next: (data) => { this.categories.set(data); this.loading.set(false); },
        error: () => { this.loading.set(false); }
      });
    } else if (tab === 'brands') {
      this.brandService.getAllBrands().subscribe({
        next: (data) => { this.brands.set(data); this.loading.set(false); },
        error: () => { this.loading.set(false); }
      });
    } else if (tab === 'taxes') {
      this.taxService.getAllTaxes().subscribe({
        next: (data) => { this.taxes.set(data); this.loading.set(false); },
        error: () => { this.loading.set(false); }
      });
    } else if (tab === 'units') {
      this.unitService.getAllUnits().subscribe({
        next: (data) => { this.units.set(data); this.loading.set(false); },
        error: () => { this.loading.set(false); }
      });
    }
  }

  submitForm() {
    this.saving.set(true);
    const tab = this.activeTab();

    if (tab === 'categories') {
      const req: CreateCategoryRequest = { name: this.formData.name, description: this.formData.description };
      this.categoryService.createCategory(req).subscribe({
        next: (res) => { this.categories.update(c => [...c, res]); this.saving.set(false); this.resetForm(); },
        error: () => { alert('Failed to create category'); this.saving.set(false); }
      });
    } else if (tab === 'brands') {
      const req: CreateBrandRequest = { name: this.formData.name, description: this.formData.description };
      this.brandService.createBrand(req).subscribe({
        next: (res) => { this.brands.update(b => [...b, res]); this.saving.set(false); this.resetForm(); },
        error: () => { alert('Failed to create brand'); this.saving.set(false); }
      });
    } else if (tab === 'taxes') {
      const req: CreateTaxRequest = { name: this.formData.name, rate: this.formData.rate, type: this.formData.type };
      this.taxService.createTax(req).subscribe({
        next: (res) => { this.taxes.update(t => [...t, res]); this.saving.set(false); this.resetForm(); },
        error: () => { alert('Failed to create tax'); this.saving.set(false); }
      });
    } else if (tab === 'units') {
      const req: CreateUnitRequest = { name: this.formData.name, shortName: this.formData.shortName };
      this.unitService.createUnit(req).subscribe({
        next: (res) => { this.units.update(u => [...u, res]); this.saving.set(false); this.resetForm(); },
        error: () => { alert('Failed to create unit'); this.saving.set(false); }
      });
    }
  }
}
