import { Component, ViewChild, ElementRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatListModule } from '@angular/material/list';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TransactionService } from '../../services/transaction.service';
import { AccountService } from '../../services/account.service';
import { ImportResult, Account } from '../../models/transaction.model';

@Component({
  selector: 'app-import',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressBarModule,
    MatListModule,
    MatSnackBarModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule,
    MatTooltipModule
  ],
  templateUrl: './import.component.html',
  styleUrl: './import.component.scss'
})
export class ImportComponent implements OnInit {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;
  selectedFile: File | null = null;
  isImporting = false;
  isCreatingAccount = false;
  importResult: ImportResult | null = null;
  dropZoneHovered = false;
  accounts: Account[] = [];
  selectedAccountId: number | null = null;
  newAccountName: string = '';

  constructor(
    private transactionService: TransactionService,
    private accountService: AccountService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadAccounts();
  }

  loadAccounts(): void {
    this.accountService.getAccounts(false).subscribe({
      next: (accounts) => {
        this.accounts = accounts;
        if (accounts.length > 0 && !this.selectedAccountId) {
          this.selectedAccountId = accounts[0].id;
        }
      },
      error: (error) => {
        console.error('Error loading accounts:', error);
        this.snackBar.open('Error loading accounts', 'Close', { duration: 3000 });
      }
    });
  }

  createAccount(): void {
    const name = this.newAccountName.trim();
    if (!name) {
      this.snackBar.open('Please enter a valid account name', 'Close', { duration: 3000 });
      return;
    }

    this.isCreatingAccount = true;
    this.accountService.createAccount({ name, isActive: true }).subscribe({
      next: (account) => {
        this.accounts = [...this.accounts, account];
        this.selectedAccountId = account.id;
        this.newAccountName = '';
        this.isCreatingAccount = false;
        this.snackBar.open(`Account "${account.name}" created`, 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error creating account:', error);
        this.isCreatingAccount = false;
        this.snackBar.open('Error creating account. Please try again.', 'Close', { duration: 3000 });
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
