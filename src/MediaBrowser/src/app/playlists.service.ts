import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SearchRequest } from './common-controls.service';
import { FileReadModel } from './files.service';

@Injectable({
  providedIn: 'root'
})
export class PlaylistsService {

  constructor(private httpClient : HttpClient) { }

  public create(request : CreatePlaylistRequest, thumbnails? : any[]) : Observable<PlaylistReadModel> {
    if (thumbnails === undefined || thumbnails.length === 0) {
      return this.httpClient.post<PlaylistReadModel>('/api/playlists', request);
    }

    let formData = new FormData();

    formData.append('json', JSON.stringify(request));

    for (var index = 0; index < thumbnails.length; index++) {
      formData.append('thumbnail.' + index, thumbnails[index]);
    }

    return this.httpClient.post<PlaylistReadModel>('/api/playlists', formData);
  }

  public deletePlaylistReference(playlistId : string, fileId : string) : Observable<FileReadModel> {
    return this.httpClient.delete<FileReadModel>(`/api/playlists/${playlistId}/files/${fileId}`);
  }
  
  public get(playlistId : string) : Observable<PlaylistReadModel> {
    return this.httpClient.get<PlaylistReadModel>(`/api/playlists/${playlistId}`);
  }
  
  public getByFile(fileId : string) : Observable<PlaylistReadModel[]> {
    return this.httpClient.get<PlaylistReadModel[]>(`/api/files/${fileId}/playlists`);
  }

  public search(query : SearchPlaylistsRequest) : Observable<SearchPlaylistsResponse> {
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
      let observable = this.httpClient.get<SearchPlaylistsResponse>('/api/playlists/search', { params: params } );
    
      observable.subscribe(subscriber);
    });
  }

  public setPlaylistReference(playlistId : string, fileId : string, index? : number) : Observable<FileReadModel> {
    return this.httpClient.patch<FileReadModel>(index === undefined ? `/api/playlists/${playlistId}/files/${fileId}` : `/api/playlists/${playlistId}/files/${fileId}/${index}`, {});
  }

  public update(playlistId : string, request : UpdatePlaylistRequest, thumbnails : any[]) : Observable<PlaylistReadModel> {
    if (thumbnails === undefined || thumbnails.length === 0) {
      return this.httpClient.put<PlaylistReadModel>(`/api/playlists/${playlistId}`, request);
    }

    let formData = new FormData();

    formData.append('json', JSON.stringify(request));

    for (var index = 0; index < thumbnails.length; index++) {
      formData.append('thumbnail.' + index, thumbnails[index]);
    }

    return this.httpClient.put<PlaylistReadModel>(`/api/playlists/${playlistId}`, formData);
  }
}

export class SearchPlaylistsRequest extends SearchRequest {
  constructor(
    request : SearchRequest) {
    super(request.ascending, request.filter, request.keywords, request.skip, request.sort, request.take)
  }
}

export interface SearchPlaylistsResponse extends SearchPlaylistsRequest {
  count : number;
  results : PlaylistReadModel[];
}

export enum PlaylistFilterOptions {
  noFilter = 'noFilter'
}

export enum PlaylistSortOptions {
  createdOn = 'createdOn',
  name = 'name'
}

export interface PlaylistReadModel {
  createdBy : string;
  createdOn : Date;
  description : string;
  id : string;
  name : string;
  readRoles : string[];
  thumbnails : PlaylistThumbnail[];
  updateRoles : string[];
}

export interface PlaylistThumbnail {
  contentLength : number;
  contentType : string;
  createdOn : Date;
  height : number;
  md5 : string;
  url : string;
  width : number;
}

export interface CreatePlaylistRequest {
  description : string;
  name : string;
  readRoles : string[];
  updateRoles : string[];
}

export interface UpdatePlaylistRequest {
  description : string;
  name : string;
  readRoles : string[];
  thumbnailsToRemove : string[];
  updateRoles : string[];
}
