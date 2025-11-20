import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  get<T>(endpoint: string, params?: any, options?: any): Observable<T> {
    let httpParams = new HttpParams();
    if (params) {
      Object.keys(params).forEach(key => {
        if (params[key] !== null && params[key] !== undefined) {
          httpParams = httpParams.append(key, params[key]);
        }
      });
    }
    const httpOptions = { params: httpParams, ...options };
    return this.http.get(`${this.apiUrl}/${endpoint}`, httpOptions) as Observable<T>;
  }

  post<T>(endpoint: string, data: any): Observable<T> {
    return this.http.post(`${this.apiUrl}/${endpoint}`, data) as Observable<T>;
  }

  put<T>(endpoint: string, data: any): Observable<T> {
    return this.http.put(`${this.apiUrl}/${endpoint}`, data) as Observable<T>;
  }

  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete(`${this.apiUrl}/${endpoint}`) as Observable<T>;
  }
}
