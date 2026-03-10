import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
}

export interface UserReadModel {
  id: string;
  username: string;
}

export interface UserRegisterRequest {
  username: string;
  password: string;
}

export interface UserLoginRequest {
  username: string;
  password: string;
}

@Injectable({
  providedIn: 'root'
})
export class UsersService {

  constructor(private apiService: ApiService) {}

  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.apiService.put<void>('/users/me/password', request);
  }

  register(request: UserRegisterRequest): Observable<UserReadModel> {
    return this.apiService.post<UserReadModel>('/users/register', request);
  }

  login(request: UserLoginRequest): Observable<UserReadModel> {
    return this.apiService.post<UserReadModel>('/users/login', request);
  }

  getCurrentUser(): Observable<UserReadModel> {
    return this.apiService.get<UserReadModel>('/users/me');
  }

  logout(): Observable<void> {
    return this.apiService.post<void>('/users/logout', {});
  }

  isAuthenticated(): boolean {
    return /(^|;)\s*mb_auth\s*=/.test(document.cookie);
  }

  getUsers(): Observable<UserReadModel[]> {
    return this.apiService.get<UserReadModel[]>('/users');
  }

  deleteUser(userId: string): Observable<void> {
    return this.apiService.delete<void>(`/users/${userId}`);
  }
}