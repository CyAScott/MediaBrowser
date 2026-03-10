import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UsersService, UserReadModel } from '../services/users.service';
import { ChangePasswordModalComponent } from './change-password-modal/change-password-modal';
import { AddUserModalComponent } from './add-user-modal/add-user-modal';
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';
import { SpinnerComponent } from '../spinner/spinner';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ChangePasswordModalComponent, AddUserModalComponent, SpinnerComponent],
  templateUrl: './user-management.html',
  styleUrls: ['./user-management.css']
})
export class UserManagementComponent implements OnInit {
  private cdr = inject(ChangeDetectorRef);
  private usersService = inject(UsersService);
  
  users: UserReadModel[] = [];
  isChangePasswordModalOpen = false;
  isAddUserModalOpen = false;
  isLoading = false;

  async ngOnInit(): Promise<void> {
    await this.loadUsers();
  }

  async loadUsers(): Promise<void> {
    this.isLoading = true;
    try {
      this.users = await firstValueFrom(this.usersService.getUsers());
    } catch (error) {
      console.error('Error loading users:', error);
    } finally {
      this.isLoading = false;
      this.cdr.detectChanges();
    }
  }

  async deleteUser(userId: string): Promise<void> {
    if (confirm('Are you sure you want to delete this user?')) {
      this.isLoading = true;
      try {
        await firstValueFrom(this.usersService.deleteUser(userId));
      } catch (error) {
        console.error('Error deleting user:', error);
      } finally {
        this.loadUsers();
      }
    }
  }

  openChangePasswordModal(): void {
    this.isChangePasswordModalOpen = true;
  }

  closeChangePasswordModal(): void {
    this.isChangePasswordModalOpen = false;
  }

  openAddUserModal(): void {
    this.isAddUserModalOpen = true;
  }

  closeAddUserModal(): void {
    this.isAddUserModalOpen = false;
  }

  async onUserAdded(): Promise<void> {
    this.closeAddUserModal();
    await this.loadUsers();
  }
}