import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SearchRequest } from './common-controls.service';

declare var pageInfo : AllRolesReadModel;

@Injectable({
  providedIn: 'root'
})
export class RolesService {

  constructor(private httpClient: HttpClient) {
    this.allRoles = pageInfo.allRoles;
  }

  public allRoles : string[];

  public create(request : CreateRoleRequest) : Observable<RoleReadModel> {
    return this.httpClient.post<RoleReadModel>('/api/roles', request);
  }

  public get(roleId : string) : Observable<RoleReadModel> {
    return this.httpClient.get<RoleReadModel>(`/api/roles/${roleId}`);
  }

  public search(query : SearchRolesRequest) : Observable<SearchRolesResponse> {
    var params = new HttpParams();

    params.append('ascending', query.ascending === false ? 'false' : 'true');

    if (query.keywords) {
      params.append('keywords', query.keywords);
    }

    if (query.sort) {
      params.append('sort', query.sort);
    }

    if (query.skip) {
      params.append('skip', query.skip.toString());
    }

    if (query.take) {
      params.append('take', query.take.toString());
    }
    
    return this.httpClient.get<SearchRolesResponse>('/api/roles/search', { params: params } );
  }

  public update(roleId : string, request : UpdateRoleRequest) : Observable<RoleReadModel> {
    return this.httpClient.put<RoleReadModel>(`/api/roles/${roleId}`, request);
  }
}

export class SearchRolesRequest extends SearchRequest {
  constructor(
    request : SearchRequest) {
    super(request.ascending, request.filter, request.keywords, request.skip, request.sort, request.take)
  }
}

export interface SearchRolesResponse extends SearchRolesRequest {
  count : number;
  results : RoleReadModel[];
}

export enum RoleSortOptions {
  description = "description",
  name = "name"
}

export interface CreateRoleRequest {
  description : string;
  name : string;
}

export interface RoleReadModel {
  id : string;
  description : string;
  name : string;
}

export interface UpdateRoleRequest {
  description : string;
}

interface AllRolesReadModel {
  allRoles : string[];
}
