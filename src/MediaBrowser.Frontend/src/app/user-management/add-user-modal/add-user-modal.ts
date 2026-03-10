import { ChangeDetectorRef, Component, EventEmitter, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UsersService, UserRegisterRequest } from '../../services/users.service';
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';

@Component({
  selector: 'app-add-user-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './add-user-modal.html',
  styleUrls: ['./add-user-modal.css']
})
export class AddUserModalComponent {
  @Output() close = new EventEmitter<void>();
  @Output() userAdding = new EventEmitter<void>();
  @Output() userAddingFailed = new EventEmitter<void>();
  @Output() userAdded = new EventEmitter<void>();

  private cdr = inject(ChangeDetectorRef);
  private usersService = inject(UsersService);

  username = '';
  password = '';
  confirmPassword = '';
  isSubmitting = false;
  errorMessage = '';
  successMessage = '';

  async onSubmit(): Promise<void> {
    this.errorMessage = '';
    this.successMessage = '';

    // Validation
    if (!this.username || !this.password || !this.confirmPassword) {
      this.errorMessage = 'All fields are required';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match';
      return;
    }

    if (this.password.length < 6) {
      this.errorMessage = 'Password must be at least 6 characters long';
      return;
    }

    if (this.username.length < 3) {
      this.errorMessage = 'Username must be at least 3 characters long';
      return;
    }

    this.isSubmitting = true;

    const request: UserRegisterRequest = {
      username: this.username,
      password: this.password
    };

    try {
      this.userAdding.emit();
      await firstValueFrom(this.usersService.register(request));
      this.successMessage = 'User added successfully!';
      this.userAdded.emit();
    } catch (error: any) {
      this.errorMessage = error.error?.message || 'Failed to add user';
      this.userAddingFailed.emit();
    } finally {
      this.isSubmitting = false;
      this.cdr.detectChanges();
    }
  }

  onClose(): void {
    this.close.emit();
  }

  onBackdropClick(event: Event): void {
    if (event.target === event.currentTarget) {
      this.onClose();
    }
  }
}