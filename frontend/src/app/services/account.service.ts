import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Account } from '../models/transaction.model';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private apiUrl = 'api/accounts';

  constructor(private http: HttpClient) { }

  getAccounts(activeOnly: boolean = false): Observable<Account[]> {
    let params = new HttpParams();
    if (activeOnly) {
      params = params.set('activeOnly', 'true');
    }
    return this.http.get<Account[]>(this.apiUrl, { params });
  }

  getAccount(id: number): Observable<Account> {
    return this.http.get<Account>(`${this.apiUrl}/${id}`);
  }

  createAccount(account: Partial<Account>): Observable<Account> {
    return this.http.post<Account>(this.apiUrl, account);
  }

  updateAccount(id: number, account: Account): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, account);
  }

  deleteAccount(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getAccountTotals(): Observable<{ totalAccounts: number; totalBalance: number; accounts: Array<{ id: number; name: string; accountType: string; balance: number; color: string }> }> {
    return this.http.get<{ totalAccounts: number; totalBalance: number; accounts: Array<{ id: number; name: string; accountType: string; balance: number; color: string }> }>(`${this.apiUrl}/summary/totals`);
  }
}