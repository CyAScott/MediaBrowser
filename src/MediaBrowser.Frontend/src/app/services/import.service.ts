import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { MediaReadModel, UpdateMediaRequest } from './media.service';

export interface ImportFileInfo {
  createdOn: Date;
  ctimeMs: number;
  mime: string;
  mtimeMs: number;
  name: string;
  size: number;
  updatedOn: Date;
  url: string;
}

export interface ImportMediaRequest extends UpdateMediaRequest {
  thumbnail?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ImportService {

  constructor(private apiService: ApiService) {}

  files(): Observable<ImportFileInfo[]> {
    return this.apiService.get<ImportFileInfo[]>('/import/files');
  }

  import(name: string, request: ImportMediaRequest): Observable<MediaReadModel> {
    return this.apiService.post<MediaReadModel>(`/import/file/${encodeURIComponent(name)}`, request);
  }
}
