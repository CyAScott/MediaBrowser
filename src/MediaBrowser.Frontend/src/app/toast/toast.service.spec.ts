import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ToastService } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    vi.useFakeTimers();
    TestBed.configureTestingModule({});
    service = TestBed.inject(ToastService);
  });

  afterEach(() => {
    service.dismiss();
    vi.useRealTimers();
  });

  it('shows a success toast and dismisses it after the timeout', () => {
    service.showSuccess('Saved successfully', 1000);

    expect(service.toast()).toEqual({
      id: 1,
      message: 'Saved successfully',
      type: 'success'
    });

    vi.advanceTimersByTime(1000);

    expect(service.toast()).toBeNull();
  });

  it('replaces the active toast and keeps the latest timeout', () => {
    service.showWarning('Heads up', 1000);
    service.showError('Something failed', 2000);

    vi.advanceTimersByTime(1000);

    expect(service.toast()?.message).toBe('Something failed');

    vi.advanceTimersByTime(1000);

    expect(service.toast()).toBeNull();
  });
});