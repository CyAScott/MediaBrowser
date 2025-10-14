import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { UpdateMediaRequest } from './media.service';

export interface ImportMediaRequest extends UpdateMediaRequest {
  thumbnail?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ImportService {

  constructor(private apiService: ApiService) {}

  files(): Observable<string[]> {
    return this.apiService.get<string[]>('/import/files');
  }

  import(name: string, request: ImportMediaRequest): Observable<void> {
    return this.apiService.post<void>(`/import/file/${decodeURIComponent(name)}`, request);
  }
}
