import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

export interface ApiError {
  error: string;
  message: string;
  statusCode: number;
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly baseUrl = '/api';

  constructor(private http: HttpClient) {}

  delete<T>(endpoint: string, params?: any): Observable<T> {
    const url = `${this.baseUrl}${endpoint}`;
    const httpParams = this.buildHttpParams(params);
    
    return this.http.delete<T>(url, { 
      params: httpParams
    }).pipe(
      catchError(this.handleError)
    );
  }

  get<T>(endpoint: string, params?: any): Observable<T> {
    const url = `${this.baseUrl}${endpoint}`;
    const httpParams = this.buildHttpParams(params);
    
    return this.http.get<T>(url, { 
      params: httpParams
    }).pipe(
      catchError(this.handleError)
    );
  }

  put<T>(endpoint: string, body: any, params?: any): Observable<T> {
    const url = `${this.baseUrl}${endpoint}`;
    const httpParams = this.buildHttpParams(params);

    return this.http.put<T>(url, body, { 
      params: httpParams
    }).pipe(
      catchError(this.handleError)
    );
  }

  post<T>(endpoint: string, body: any, params?: any): Observable<T> {
    const url = `${this.baseUrl}${endpoint}`;
    const httpParams = this.buildHttpParams(params);
    
    return this.http.post<T>(url, body, { 
      params: httpParams
    }).pipe(
      catchError(this.handleError)
    );
  }

  private buildHttpParams(params?: any): HttpParams {
    let httpParams = new HttpParams();
    
    if (params) {
      Object.keys(params).forEach(key => {
        if (params[key] !== null && params[key] !== undefined) {
          if (Array.isArray(params[key])) {
            httpParams = httpParams.set(key, params[key].join(','));
          } else {
            httpParams = httpParams.set(key, params[key].toString());
          }
        }
      });
    }
    
    return httpParams;
  }

  private handleError = (error: any): Observable<never> => {
    let errorMessage = 'An unknown error occurred';
    let statusCode = 0;
    
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = error.error.message;
    } else {
      // Server-side error
      statusCode = error.status;
      errorMessage = error.error?.message || error.message || `Error Code: ${error.status}`;
    }
    
    console.error('API Error:', error);
    
    const apiError: ApiError = {
      error: error.error?.error || 'API_ERROR',
      message: errorMessage,
      statusCode: statusCode
    };
    
    return throwError(() => apiError);
  };
}