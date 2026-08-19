import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    loadComponent: () => import('./layout/app-layout/app-layout.component').then(m => m.AppLayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/reports/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'pos',
        loadComponent: () => import('./features/pos/pos-terminal.component').then(m => m.PosTerminalComponent)
      },
      {
        path: 'inventory',
        loadComponent: () => import('./features/inventory/inventory-list.component').then(m => m.InventoryListComponent)
      },
      {
        path: 'purchases',
        loadComponent: () => import('./features/purchases/components/purchase-list.component').then(m => m.PurchaseListComponent)
      },
      {
        path: 'purchases/new',
        loadComponent: () => import('./features/purchases/components/purchase-create.component').then(m => m.PurchaseCreateComponent)
      },
      {
        path: 'sales-history',
        loadComponent: () => import('./features/sales-history/sales-history.component').then(m => m.SalesHistoryComponent)
      },
      {
        path: 'expenses',
        loadComponent: () => import('./features/expenses/components/expenses.component').then(m => m.ExpensesComponent)
      },
      {
        path: 'payments',
        loadComponent: () => import('./features/payments/components/payments.component').then(m => m.PaymentsComponent)
      },
      {
        path: 'customers',
        loadComponent: () => import('./features/customers/components/customers.component').then(m => m.CustomersComponent)
      },
      {
        path: 'suppliers',
        loadComponent: () => import('./features/suppliers/components/suppliers.component').then(m => m.SuppliersComponent)
      },
      {
        path: 'master-data',
        loadComponent: () => import('./features/master-data/components/master-data.component').then(m => m.MasterDataComponent)
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
