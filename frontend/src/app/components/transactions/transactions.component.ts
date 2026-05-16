import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, MatOptionModule } from '@angular/material/core';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { TransactionService } from '../../services/transaction.service';
import { CategoryService } from '../../services/category.service';
import { Transaction, Category, TransactionFilter } from '../../models/transaction.model';

@Component({
  selector: 'app-transactions',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatOptionModule,
    MatDialogModule,
    MatCheckboxModule,
    MatSnackBarModule,
    MatProgressBarModule
  ],
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.scss'
})
export class TransactionsComponent implements OnInit {
  transactions: Transaction[] = [];
  categories: Category[] = [];
  loading = true;
  totalCount = 0;
  pageSize = 20;
  pageIndex = 0;
  
  displayedColumns: string[] = ['date', 'description', 'category', 'amount', 'actions'];
  
  filter: TransactionFilter = {
    page: 1,
    pageSize: 20
  };

  constructor(
    private transactionService: TransactionService,
    private categoryService: CategoryService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadCategories();
    this.loadTransactions();
  }

  loadCategories(): void {
    this.categoryService.getCategories(true).subscribe({
      next: (categories) => {
        this.categories = categories;
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  loadTransactions(): void {
    this.loading = true;
    this.filter.page = this.pageIndex + 1;
    this.filter.pageSize = this.pageSize;
    
    this.transactionService.getTransactions(this.filter).subscribe({
      next: (result) => {
        this.transactions = result.transactions;
        this.totalCount = result.totalCount;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading transactions:', error);
        this.loading = false;
      }
    });
  }

  onPaginatorChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadTransactions();
  }

  onFilterChange(): void {
    this.pageIndex = 0;
    this.loadTransactions();
  }

  clearFilters(): void {
    this.filter = {
      page: 1,
      pageSize: this.pageSize
    };
    this.pageIndex = 0;
    this.loadTransactions();
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  formatDate(date: Date | string): string {
    return new Date(date).toLocaleDateString();
  }

  getCategoryName(categoryId?: number): string {
    if (!categoryId) return 'Uncategorized';
    const category = this.categories.find(c => c.id === categoryId);
    return category ? category.name : 'Unknown';
  }

  getCategoryColor(categoryId?: number): string {
    if (!categoryId) return '#9e9e9e';
    const category = this.categories.find(c => c.id === categoryId);
    return category?.color || '#9e9e9e';
  }

  deleteTransaction(id: number): void {
    if (confirm('Are you sure you want to delete this transaction?')) {
      this.transactionService.deleteTransaction(id).subscribe({
        next: () => {
          this.snackBar.open('Transaction deleted successfully', 'Close', { duration: 3000 });
          this.loadTransactions();
        },
        error: (error) => {
          console.error('Error deleting transaction:', error);
          this.snackBar.open('Error deleting transaction', 'Close', { duration: 3000 });
        }
      });
    }
  }

  updateCategory(transaction: Transaction, categoryId: number): void {
    transaction.categoryId = categoryId;
    this.transactionService.updateTransaction(transaction.id, transaction).subscribe({
      next: () => {
        this.snackBar.open('Category updated successfully', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error updating category:', error);
        this.snackBar.open('Error updating category', 'Close', { duration: 3000 });
      }
    });
  }

  exportTransactions(): void {
    this.transactionService.exportCsv({
      startDate: this.filter.startDate,
      endDate: this.filter.endDate,
      categoryId: this.filter.categoryId
    }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `transactions_${new Date().toISOString().split('T')[0]}.csv`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.snackBar.open('Transactions exported successfully', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error exporting transactions:', error);
        this.snackBar.open('Error exporting transactions', 'Close', { duration: 3000 });
      }
    });
  }
}
