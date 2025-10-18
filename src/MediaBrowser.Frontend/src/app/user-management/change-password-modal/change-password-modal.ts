import { ChangeDetectorRef, Component, EventEmitter, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UsersService, ChangePasswordRequest } from '../../services/users.service';
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';

@Component({
  selector: 'app-change-password-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './change-password-modal.html',
  styleUrls: ['./change-password-modal.css']
})
export class ChangePasswordModalComponent {
  @Output() close = new EventEmitter<void>();
  @Output() userUpdating = new EventEmitter<void>();
  @Output() userUpdatingFailed = new EventEmitter<void>();

  private cdr = inject(ChangeDetectorRef);
  private usersService = inject(UsersService);

  oldPassword = '';
  newPassword = '';
  confirmPassword = '';
  isSubmitting = false;
  errorMessage = '';
  successMessage = '';

  async onSubmit(): Promise<void> {
    this.errorMessage = '';
    this.successMessage = '';

    // Validation
    if (!this.oldPassword || !this.newPassword || !this.confirmPassword) {
      this.errorMessage = 'All fields are required';
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.errorMessage = 'New passwords do not match';
      return;
    }

    if (this.newPassword.length < 6) {
      this.errorMessage = 'New password must be at least 6 characters long';
      return;
    }

    this.isSubmitting = true;

    const request: ChangePasswordRequest = {
      oldPassword: this.oldPassword,
      newPassword: this.newPassword
    };


    try {
      this.userUpdating.emit();
      await firstValueFrom(this.usersService.changePassword(request));
      this.successMessage = 'Password changed successfully!';
      this.close.emit();
    } catch (error: any) {
      this.errorMessage = error.error?.message || 'Failed to change password';
      this.userUpdatingFailed.emit();
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