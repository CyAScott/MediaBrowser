import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SearchRequest } from './common-controls.service';

declare var viewModel : UserReadModel;

@Injectable({
  providedIn: 'root'
})
export class UsersService {

  constructor(private httpClient: HttpClient) {
  }

  public hasRole(role : string) : boolean {
    return viewModel?.roles && viewModel.roles.includes(role);
  }

  public id() : string {
    return viewModel?.id || '';
  }

  public create(request : CreateUserRequest) : Observable<UserReadModel> {
    return this.httpClient.post<UserReadModel>('/api/users', request);
  }

  public get(userId : string) : Observable<UserReadModel> {
    return this.httpClient.get<UserReadModel>(`/api/users/${userId}`);
  }

  public search(query : SearchUsersRequest) : Observable<SearchUsersResponse> {
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

    if (query.roles && query.roles.length) {
      query.roles.forEach(it => params = params.append('roles[]', it));
    }

    if (query.skip) {
      params = params.append('skip', query.skip.toString());
    }

    if (query.take) {
      params = params.append('take', query.take.toString());
    }
    
    return this.httpClient.get<SearchUsersResponse>('/api/users/search', { params: params } );
  }

  public update(userId : string, request : UpdateUserRequest) : Observable<UserReadModel> {
    return this.httpClient.put<UserReadModel>(`/api/users/${userId}`, request);
  }

  public delete(userId : string) : Observable<UserReadModel> {
    return this.httpClient.delete<UserReadModel>(`/api/users/${userId}`);
  }
}

export enum UserFilterOptions {
  deleted = "deleted",
  nofilter = "nofilter",
  nonDeleted = "nonDeleted"
}

export enum UserSortOptions {
  deletedOn = "deletedOn",
  firstName = "firstName",
  lastName = "lastName",
  userName = "userName"
}

export interface CreateUserRequest {
  firstName : string;
  lastName : string;
  password : string;
  userName : string;
  roles : string[];
}

export class SearchUsersRequest extends SearchRequest {
  constructor(
    request : SearchRequest,
    public readonly roles : string[] = []) {
    super(request.ascending, request.filter, request.keywords, request.skip, request.sort, request.take)
  }
}

export interface SearchUsersResponse extends SearchUsersRequest {
  count : number;
  results : UserReadModel[];
}

export interface UpdateUserRequest {
  firstName : string;
  lastName : string;
  password : string;
  userName : string;
  roles : string[];
}

export interface UserReadModel {
  deletedOn : Date;
  id : string;
  firstName : string;
  lastName : string;
  userName : string;
  roles : string[];
}