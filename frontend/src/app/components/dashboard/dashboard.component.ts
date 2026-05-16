import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { TransactionService } from '../../services/transaction.service';
import { TransactionSummary, CategoryBreakdown } from '../../models/transaction.model';
import { FormsModule } from '@angular/forms';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatNativeDateModule } from '@angular/material/core';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatGridListModule,
    MatIconModule,
    MatProgressBarModule,
    MatSelectModule,
    MatDatepickerModule,
    MatInputModule,
    MatFormFieldModule,
    MatNativeDateModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  summary: TransactionSummary | null = null;
  loading = true;
  startDate: Date | null = null;
  endDate: Date | null = null;

  constructor(private transactionService: TransactionService) {}

  ngOnInit(): void {
    this.loadSummary();
  }

  loadSummary(): void {
    this.loading = true;
    const start = this.startDate ? this.formatDate(this.startDate) : undefined;
    const end = this.endDate ? this.formatDate(this.endDate) : undefined;
    
    this.transactionService.getSummary(start, end).subscribe({
      next: (data) => {
        this.summary = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading summary:', error);
        this.loading = false;
      }
    });
  }

  onFilterChange(): void {
    this.loadSummary();
  }

  clearFilters(): void {
    this.startDate = null;
    this.endDate = null;
    this.loadSummary();
  }

  formatDate(date: Date): string {
    return date.toISOString().split('T')[0];
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(Math.abs(amount));
  }

  getCategoryPercentage(category: CategoryBreakdown): number {
    if (!this.summary || this.summary.totalExpenses === 0) return 0;
    return (Math.abs(category.totalAmount) / this.summary.totalExpenses) * 100;
  }
}