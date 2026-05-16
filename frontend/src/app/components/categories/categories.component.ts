import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { CategoryService } from '../../services/category.service';
import { Category } from '../../models/transaction.model';

@Component({
  selector: 'app-categories',
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
    MatSelectModule,
    MatDialogModule,
    MatSnackBarModule,
    MatCheckboxModule,
    MatProgressBarModule
  ],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.scss'
})
export class CategoriesComponent implements OnInit {
  categories: Category[] = [];
  loading = true;
  selectedCategory: Category | null = null;
  editingCategory: Partial<Category> = {};
  isEditing = false;

  constructor(
    private categoryService: CategoryService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.loading = true;
    this.categoryService.getCategories().subscribe({
      next: (categories) => {
        this.categories = categories;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.loading = false;
      }
    });
  }

  selectCategory(category: Category): void {
    this.selectedCategory = { ...category };
    this.editingCategory = { ...category };
    this.isEditing = false;
  }

  startEditing(): void {
    this.isEditing = true;
  }

  cancelEditing(): void {
    this.isEditing = false;
    if (this.selectedCategory) {
      this.editingCategory = { ...this.selectedCategory };
    }
  }

  saveCategory(): void {
    if (!this.editingCategory.id) {
      // Create new category
      this.categoryService.createCategory(this.editingCategory as Category).subscribe({
        next: (newCategory) => {
          this.categories.push(newCategory);
          this.selectedCategory = newCategory;
          this.isEditing = false;
          this.snackBar.open('Category created successfully', 'Close', { duration: 3000 });
        },
        error: (error) => {
          console.error('Error creating category:', error);
          this.snackBar.open('Error creating category', 'Close', { duration: 3000 });
        }
      });
    } else {
      // Update existing category
      this.categoryService.updateCategory(this.editingCategory.id, this.editingCategory as Category).subscribe({
        next: () => {
          const index = this.categories.findIndex(c => c.id === this.editingCategory.id);
          if (index !== -1) {
            this.categories[index] = { ...this.editingCategory } as Category;
          }
          this.selectedCategory = { ...this.editingCategory } as Category;
          this.isEditing = false;
          this.snackBar.open('Category updated successfully', 'Close', { duration: 3000 });
        },
        error: (error) => {
          console.error('Error updating category:', error);
          this.snackBar.open('Error updating category', 'Close', { duration: 3000 });
        }
      });
    }
  }

  deleteCategory(): void {
    if (!this.selectedCategory) return;

    if (confirm(`Are you sure you want to delete "${this.selectedCategory.name}"? Transactions will be moved to Uncategorized.`)) {
      this.categoryService.deleteCategory(this.selectedCategory.id).subscribe({
        next: () => {
          this.categories = this.categories.filter(c => c.id !== this.selectedCategory!.id);
          this.selectedCategory = null;
          this.snackBar.open('Category deleted successfully', 'Close', { duration: 3000 });
        },
        error: (error) => {
          console.error('Error deleting category:', error);
          this.snackBar.open('Error deleting category', 'Close', { duration: 3000 });
        }
      });
    }
  }

  createNewCategory(): void {
    this.selectedCategory = null;
    this.editingCategory = {
      name: '',
      color: '#9e9e9e',
      icon: 'label',
      isIncome: false,
      isActive: true,
      autoCategorizeKeywords: ''
    };
    this.isEditing = true;
  }

  getIncomeCategories(): Category[] {
    return this.categories.filter(c => c.isIncome);
  }

  getExpenseCategories(): Category[] {
    return this.categories.filter(c => !c.isIncome);
  }
}