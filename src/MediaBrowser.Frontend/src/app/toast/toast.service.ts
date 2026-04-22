import { Injectable, signal } from '@angular/core';

export type ToastType = 'error' | 'warning' | 'success';

export interface ToastNotification {
  id: number;
  message: string;
  type: ToastType;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private readonly nextId = signal(0);
  readonly toast = signal<ToastNotification | null>(null);
  private dismissTimer: ReturnType<typeof setTimeout> | null = null;

  show(message: string, type: ToastType, durationMs = 3000): void {
    const id = this.nextId() + 1;
    this.nextId.set(id);
    this.toast.set({ id, message, type });

    this.clearTimer();
    this.dismissTimer = setTimeout(() => {
      if (this.toast()?.id === id) {
        this.toast.set(null);
      }
      this.dismissTimer = null;
    }, durationMs);
  }

  showError(message: string, durationMs?: number): void {
    this.show(message, 'error', durationMs);
  }

  showWarning(message: string, durationMs?: number): void {
    this.show(message, 'warning', durationMs);
  }

  showSuccess(message: string, durationMs?: number): void {
    this.show(message, 'success', durationMs);
  }

  dismiss(): void {
    this.clearTimer();
    this.toast.set(null);
  }

  private clearTimer(): void {
    if (this.dismissTimer !== null) {
      clearTimeout(this.dismissTimer);
      this.dismissTimer = null;
    }
  }
}