import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Category } from '../models/transaction.model';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private readonly apiUrl = '/api/categories';

  constructor(private http: HttpClient) { }

  getCategories(isActive?: boolean, isIncome?: boolean): Observable<Category[]> {
    let params = new HttpParams();
    if (isActive !== undefined) params = params.set('isActive', isActive.toString());
    if (isIncome !== undefined) params = params.set('isIncome', isIncome.toString());
    
    return this.http.get<Category[]>(this.apiUrl, { params });
  }

  getCategory(id: number): Observable<Category> {
    return this.http.get<Category>(`${this.apiUrl}/${id}`);
  }

  createCategory(category: Category): Observable<Category> {
    return this.http.post<Category>(this.apiUrl, category);
  }

  updateCategory(id: number, category: Category): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, category);
  }

  deleteCategory(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getCategoryStats(id: number, startDate?: Date | string, endDate?: Date | string): Observable<any> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', this.formatDate(startDate));
    if (endDate) params = params.set('endDate', this.formatDate(endDate));
    
    return this.http.get<any>(`${this.apiUrl}/${id}/stats`, { params });
  }

  private formatDate(date: Date | string): string {
    if (typeof date === 'string') {
      return date;
    }
    return date.toISOString().split('T')[0];
  }
}