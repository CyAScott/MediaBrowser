import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SearchRequest } from './common-controls.service';

@Injectable({
  providedIn: 'root'
})
export class FilesService {

  constructor(private httpClient: HttpClient) { }

  public cache(file : FileReadModel) : Observable<FileReadModel> {
    return this.httpClient.get<FileReadModel>(`/local/cache/${file.id}`);
  }

  public get(fileId : string) : Observable<FileReadModel> {
    return this.httpClient.get<FileReadModel>(`/api/files/${fileId}`);
  }

  public lastSearch : SearchFilesRequest | undefined;
  public lastSearchResponse : SearchFilesResponse | undefined;

  public search(query : SearchFilesRequest) : Observable<SearchFilesResponse> {

    var params = new HttpParams();

    params = params.append('ascending', query.ascending === false ? 'false' : 'true');

    if (query.filter) {
      params = params.append('filter', query.filter);
    }

    if (query.keywords) {
      params = params.append('keywords', query.keywords);
    }

    if (query.sort) {
      params = params.append('sort', query.sort);
    }

    if (query.skip) {
      params = params.append('skip', query.skip.toString());
    }

    if (query.take) {
      params = params.append('take', query.take.toString());
    }

    return new Observable(subscriber => {
      let observable = this.httpClient.get<SearchFilesResponse>('/api/files/search', { params: params } );
    
      observable.subscribe({
        next: response => {
          this.lastSearch = query;
          this.lastSearchResponse = response;
          subscriber.next(response);
        },
        error: subscriber.error
      });
    });
  }

  public uncache(file : FileReadModel) : Observable<FileReadModel> {
    return this.httpClient.delete<FileReadModel>(`/local/cache/${file.id}`);
  }

  public update(fileId : string, request : UploadFileRequest) : Observable<FileReadModel> {
    return this.httpClient.put<FileReadModel>(`/api/files/${fileId}`, request);
  }

  public updateWithThumbnails(fileId : string, request : UpdateFileRequest, thumbnails : any[]) : Observable<FileReadModel> {
    let formData = new FormData();

    formData.append('json', JSON.stringify(request));

    for (var index = 0; index < thumbnails.length; index++) {
      formData.append('thumbnail.' + index, thumbnails[index]);
    }

    return this.httpClient.put<FileReadModel>(`/api/files/${fileId}/thumbnails`, formData);
  }

  public upload(request : UploadFileRequest, file : any, thumbnails : any[]) : Observable<FileReadModel> {
    let formData = new FormData();

    formData.append('json', JSON.stringify(request));
    formData.append('mediaFile', file);

    for (var index = 0; index < thumbnails.length; index++) {
      formData.append('thumbnail.' + index, thumbnails[index]);
    }

    return this.httpClient.post<FileReadModel>('/api/files', formData);
  }
}

export class SearchFilesRequest extends SearchRequest {
  constructor(
    request : SearchRequest) {
    super(request.ascending, request.filter, request.keywords, request.skip, request.sort, request.take)
  }
}

export interface SearchFilesResponse extends SearchFilesRequest {
  count : number;
  results : FileReadModel[];
}

export enum FileFilterOptions {
  AudioFiles = "audioFiles",
  html5Friendly = "html5Friendly",
  noFilter = "noFilter",
  nonHtml5Friendly = "nonHtml5Friendly",
  photos = "photos",
  videos = "videos"
}

export enum FileSortOptions {
  createdOn = "createdOn",
  duration = "duration",
  name = "name",
  type = "type"
}

export interface FileReadModel {
  audioStreams : string[];
  cached : boolean;
  contentLength : number;
  contentType : string;
  description : string;
  duration : number;
  fps : number;
  height : number;
  html5Friendly : boolean;
  id : string;
  md5 : string;
  name : string;
  readRoles : string[];
  thumbnails : Thumbnail[];
  type : string;
  updateRoles : string[];
  uploadedBy : string;
  uploadedOn : Date;
  url : string;
  videoStreams : string[];
  width : number;
}

export interface Thumbnail {
  contentLength : number;
  contentType : string;
  createdOn : Date;
  height : number;
  md5 : string;
  url : string;
  width : number;
}

export interface UpdateFileRequest {
  description : string;
  name : string;
  readRoles : string[];
  thumbnailsToRemove : string[];
  updateRoles : string[];
}

export interface UploadFileRequest {
  description : string;
  name : string;
  readRoles: string[];
  updateRoles: string[];
}