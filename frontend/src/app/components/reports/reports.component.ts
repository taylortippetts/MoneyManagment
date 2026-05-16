import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { TransactionService } from '../../services/transaction.service';
import { CategoryService } from '../../services/category.service';
import { TransactionSummary, Category, CategoryBreakdown } from '../../models/transaction.model';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatGridListModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressBarModule,
    MatSelectModule
  ],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss'
})
export class ReportsComponent implements OnInit {
  summary: TransactionSummary | null = null;
  categories: Category[] = [];
  loading = true;
  startDate: Date | null = null;
  endDate: Date | null = null;
  selectedCategoryId: number | null = null;

  // Chart data
  pieChartLabels: string[] = [];
  pieChartData: number[] = [];
  pieChartColors: string[] = [];

  barChartLabels: string[] = [];
  barChartData: number[] = [];
  barChartColors: string[] = [];

  constructor(
    private transactionService: TransactionService,
    private categoryService: CategoryService
  ) {}

  ngOnInit(): void {
    this.loadCategories();
    this.loadSummary();
  }

  loadCategories(): void {
    this.categoryService.getCategories(true, false).subscribe({
      next: (categories) => {
        this.categories = categories.filter(c => !c.isIncome);
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  loadSummary(): void {
    this.loading = true;
    const start = this.startDate ? this.formatDate(this.startDate) : undefined;
    const end = this.endDate ? this.formatDate(this.endDate) : undefined;
    
    this.transactionService.getSummary(start, end).subscribe({
      next: (data) => {
        this.summary = data;
        this.loading = false;
        this.updateCharts();
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
    this.selectedCategoryId = null;
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

  updateCharts(): void {
    if (!this.summary) return;

    // Update pie chart
    this.pieChartLabels = this.summary.categoryBreakdown.map(cb => cb.categoryName);
    this.pieChartData = this.summary.categoryBreakdown.map(cb => Math.abs(cb.totalAmount));
    this.pieChartColors = this.summary.categoryBreakdown.map((cb, index) => {
      const colors = ['#4CAF50', '#2196F3', '#FF9800', '#9C27B0', '#F44336', 
                      '#E91E63', '#00BCD4', '#607D8B', '#8BC34A', '#795548'];
      return colors[index % colors.length];
    });

    // Update bar chart - monthly breakdown would require additional API endpoint
    // For now, show category comparison
    this.barChartLabels = this.summary.categoryBreakdown.slice(0, 8).map(cb => cb.categoryName);
    this.barChartData = this.summary.categoryBreakdown.slice(0, 8).map(cb => Math.abs(cb.totalAmount));
  }

  getTopCategories(): CategoryBreakdown[] {
    if (!this.summary) return [];
    return this.summary.categoryBreakdown.slice(0, 5);
  }

  getCategoryPercent(category: CategoryBreakdown): number {
    if (!this.summary || this.summary.totalExpenses === 0) return 0;
    return Math.round((Math.abs(category.totalAmount) / this.summary.totalExpenses) * 100);
  }

  getChartColor(index: number): string {
    const colors = ['#4CAF50', '#2196F3', '#FF9800', '#9C27B0', '#F44336', 
                    '#E91E63', '#00BCD4', '#607D8B', '#8BC34A', '#795548'];
    return colors[index % colors.length];
  }

  getCategoryColor(categoryId: number): string {
    const category = this.categories.find(c => c.id === categoryId);
    return category?.color || this.getChartColor(this.categories.findIndex(c => c.id === categoryId));
  }

  getSegmentPercent(index: number): number {
    const total = this.pieChartData.reduce((a, b) => a + b, 0);
    if (total === 0) return 0;
    return (this.pieChartData[index] / total) * 100;
  }
}
