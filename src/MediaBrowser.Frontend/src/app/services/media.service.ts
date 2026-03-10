import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface SearchMediaRequest {
  cast?: string[];
  descending?: boolean;
  directors?: string[];
  genres?: string[];
  keywords?: string;
  producers?: string[];
  sort: 'title' | 'createdOn' | 'duration' | 'userStarRating';
  skip?: number;
  take?: number;
  writers?: string[];
}

export interface MediaReadModel {
  id: string;
  path: string;
  title: string;
  originalTitle: string;
  description: string;
  mime: string;
  size?: number;
  width?: number;
  height?: number;
  duration?: number;
  md5: string;
  rating?: number;
  userStarRating?: number;
  published: string;
  ctimeMs: string;
  mtimeMs: string;
  createdOn: Date;
  updatedOn: Date;
  ffprobe: any;
  cast: string[];
  directors: string[];
  genres: string[];
  producers: string[];
  writers: string[];
  url: string;
  thumbnail?: number;
  thumbnailUrl?: string;
  fanartUrl?: string;
}

export interface SearchResponse {
  results: MediaReadModel[];
  count: number;
}

export interface UpdateMediaRequest {
  title: string;
  originalTitle: string;
  description: string;
  rating?: number;
  userStarRating?: number;
  cast: string[];
  directors: string[];
  genres: string[];
  producers: string[];
  writers: string[];
}

export interface UpdateThumbnailRequest {
  at: number;
}

@Injectable({
  providedIn: 'root'
})
export class MediaService {

  constructor(private apiService: ApiService) {}

  get(id: string): Observable<MediaReadModel> {
    return this.apiService.get<MediaReadModel>(`/media/${id}`);
  }

  update(id: string, request: UpdateMediaRequest): Observable<MediaReadModel> {
    return this.apiService.put<MediaReadModel>(`/media/${id}`, request);
  }

  search(request: SearchMediaRequest): Observable<SearchResponse> {    
    return this.apiService.get<SearchResponse>('/media/search', request);
  }

  getAllCast(): Observable<string[]> {
    return this.apiService.get<string[]>(`/media/cast`);
  }

  getAllDirectors(): Observable<string[]> {
    return this.apiService.get<string[]>(`/media/directors`);
  }

  getAllGenres(): Observable<string[]> {
    return this.apiService.get<string[]>('/media/genres');
  }

  getAllProducers(): Observable<string[]> {
    return this.apiService.get<string[]>('/media/producers');
  }

  getAllWriters(): Observable<string[]> {
    return this.apiService.get<string[]>('/media/writers');
  }

  updateFanartThumbnail(id: string, request: UpdateThumbnailRequest): Observable<void> {
    return this.apiService.post<void>(`/media/${id}/file/thumbnail-fanart`, request);
  }

  updateThumbnail(id: string, request: UpdateThumbnailRequest | File): Observable<void> {
    if (request instanceof File) {
      const formData = new FormData();
      formData.append('thumbnail', request);
      formData.append('is_primary', 'true');
      return this.apiService.post<void>(`/media/${id}/file/thumbnail/file`, formData);
    } else {
      return this.apiService.post<void>(`/media/${id}/file/thumbnail`, request);
    }
  }
}