import { Component, ViewChild, ElementRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatListModule } from '@angular/material/list';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { TransactionService } from '../../services/transaction.service';
import { AccountService } from '../../services/account.service';
import { ImportResult, Account } from '../../models/transaction.model';

@Component({
  selector: 'app-import',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressBarModule,
    MatListModule,
    MatSnackBarModule,
    MatSelectModule,
    MatFormFieldModule
  ],
  templateUrl: './import.component.html',
  styleUrl: './import.component.scss'
})
export class ImportComponent implements OnInit {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;
  selectedFile: File | null = null;
  isImporting = false;
  importResult: ImportResult | null = null;
  dropZoneHovered = false;
  accounts: Account[] = [];
  selectedAccountId: number | null = null;

  constructor(
    private transactionService: TransactionService,
    private accountService: AccountService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadAccounts();
  }

  loadAccounts(): void {
    this.accountService.getAccounts(true).subscribe({
      next: (accounts) => {
        this.accounts = accounts;
        if (accounts.length > 0) {
          this.selectedAccountId = accounts[0].id;
        }
      },
      error: (error) => {
        console.error('Error loading accounts:', error);
        this.snackBar.open('Error loading accounts', 'Close', { duration: 3000 });
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.importResult = null;
    }
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dropZoneHovered = false;
    
    if (event.dataTransfer && event.dataTransfer.files.length > 0) {
      const file = event.dataTransfer.files[0];
      if (file.name.endsWith('.csv')) {
        this.selectedFile = file;
        this.importResult = null;
      } else {
        this.snackBar.open('Please drop a CSV file', 'Close', { duration: 3000 });
      }
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dropZoneHovered = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.dropZoneHovered = false;
  }

  importFile(): void {
    if (!this.selectedFile) {
      this.snackBar.open('Please select a file first', 'Close', { duration: 3000 });
      return;
    }

    if (!this.selectedAccountId) {
      this.snackBar.open('Please select an account', 'Close', { duration: 3000 });
      return;
    }

    this.isImporting = true;
    this.importResult = null;

    this.transactionService.importCsv(this.selectedFile, this.selectedAccountId).subscribe({
      next: (result) => {
        this.importResult = result;
        this.isImporting = false;
        
        if (result.importedCount > 0) {
          this.snackBar.open(
            `Successfully imported ${result.importedCount} transactions`,
            'Close',
            { duration: 5000 }
          );
        } else {
          this.snackBar.open('No new transactions were imported', 'Close', { duration: 3000 });
        }
      },
      error: (error) => {
        console.error('Import error:', error);
        this.isImporting = false;
        this.snackBar.open('Error importing file. Please check the format.', 'Close', { duration: 5000 });
      }
    });
  }

  clearSelection(): void {
    this.selectedFile = null;
    this.importResult = null;
  }

  triggerFileInput(): void {
    this.fileInput.nativeElement.click();
  }
}
