import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Transaction, TransactionFilter, ImportResult, TransactionSummary } from '../models/transaction.model';

@Injectable({
  providedIn: 'root'
})
export class TransactionService {
  private readonly apiUrl = '/api/transactions';

  constructor(private http: HttpClient) { }

  getTransactions(filter: TransactionFilter): Observable<{ transactions: Transaction[], totalCount: number, pageCount: number }> {
    let params = new HttpParams();
    
    if (filter.startDate) params = params.set('startDate', this.formatDate(filter.startDate));
    if (filter.endDate) params = params.set('endDate', this.formatDate(filter.endDate));
    if (filter.categoryId) params = params.set('categoryId', filter.categoryId.toString());
    if (filter.isReconciled !== undefined) params = params.set('isReconciled', filter.isReconciled.toString());
    if (filter.search) params = params.set('search', filter.search);
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return new Observable(observer => {
      this.http.get<Transaction[]>(this.apiUrl, { params, observe: 'response' }).subscribe({
        next: (response) => {
          const totalCount = parseInt(response.headers.get('X-Total-Count') || '0');
          const pageCount = parseInt(response.headers.get('X-Page-Count') || '0');
          observer.next({
            transactions: response.body || [],
            totalCount,
            pageCount
          });
          observer.complete();
        },
        error: (error) => observer.error(error)
      });
    });
  }

  getTransaction(id: number): Observable<Transaction> {
    return this.http.get<Transaction>(`${this.apiUrl}/${id}`);
  }

  createTransaction(transaction: Transaction): Observable<Transaction> {
    return this.http.post<Transaction>(this.apiUrl, transaction);
  }

  updateTransaction(id: number, transaction: Transaction): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, transaction);
  }

  deleteTransaction(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  importCsv(file: File, accountId: number, formatId?: number): Observable<ImportResult> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('accountId', accountId.toString());
    if (formatId !== undefined && formatId !== null) {
      formData.append('formatId', formatId.toString());
    }
    return this.http.post<ImportResult>(`${this.apiUrl}/import`, formData);
  }

  exportCsv(filter: { startDate?: Date | string; endDate?: Date | string; categoryId?: number; accountId?: number }): Observable<Blob> {
    let params = new HttpParams();
    
    if (filter.startDate) params = params.set('startDate', this.formatDate(filter.startDate));
    if (filter.endDate) params = params.set('endDate', this.formatDate(filter.endDate));
    if (filter.categoryId) params = params.set('categoryId', filter.categoryId.toString());
    if (filter.accountId) params = params.set('accountId', filter.accountId.toString());

    return this.http.get(`${this.apiUrl}/export`, { params, responseType: 'blob' });
  }

  getSummary(startDate?: Date | string, endDate?: Date | string): Observable<TransactionSummary> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', this.formatDate(startDate));
    if (endDate) params = params.set('endDate', this.formatDate(endDate));
    
    return this.http.get<TransactionSummary>(`${this.apiUrl}/summary`, { params });
  }

  bulkCategorize(categoryId: number, descriptionContains?: string): Observable<{ updatedCount: number }> {
    let params = new HttpParams()
      .set('categoryId', categoryId.toString());
    
    if (descriptionContains) params = params.set('descriptionContains', descriptionContains);
    
    return this.http.put<{ updatedCount: number }>(`${this.apiUrl}/bulk-categorize`, null, { params });
  }

  private formatDate(date: Date | string): string {
    if (typeof date === 'string') {
      return date;
    }
    return date.toISOString().split('T')[0];
  }
}