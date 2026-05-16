import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { TransactionsComponent } from './components/transactions/transactions.component';
import { CategoriesComponent } from './components/categories/categories.component';
import { ImportComponent } from './components/import/import.component';
import { ReportsComponent } from './components/reports/reports.component';

export const appRoutes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'transactions', component: TransactionsComponent },
  { path: 'categories', component: CategoriesComponent },
  { path: 'import', component: ImportComponent },
  { path: 'reports', component: ReportsComponent }
];