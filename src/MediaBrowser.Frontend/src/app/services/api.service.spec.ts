import { HttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom, throwError } from 'rxjs';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ApiError, ApiService } from './api.service';
import { ToastService } from '../toast/toast.service';

describe('ApiService', () => {
  const httpClient = {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn()
  };
  const toastService = {
    showError: vi.fn()
  };
  let consoleErrorSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    vi.clearAllMocks();
    consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

    TestBed.configureTestingModule({
      providers: [
        ApiService,
        { provide: HttpClient, useValue: httpClient },
        { provide: ToastService, useValue: toastService }
      ]
    });
  });

  afterEach(() => {
    consoleErrorSpy.mockRestore();
  });

  it('shows an error toast and rethrows a normalized api error when a request fails', async () => {
    const service = TestBed.inject(ApiService);
    const requestError = {
      error: { error: 'IMPORT_FAILED', message: 'Upload failed' },
      status: 400,
      message: 'Bad Request'
    };

    httpClient.get.mockReturnValue(throwError(() => requestError));

    await expect(firstValueFrom(service.get('/import/files'))).rejects.toMatchObject<ApiError>({
      error: 'IMPORT_FAILED',
      message: 'Upload failed',
      statusCode: 400
    });

    expect(toastService.showError).toHaveBeenCalledWith('Upload failed');
  });

  it('falls back to the generic message for unknown failures', async () => {
    const service = TestBed.inject(ApiService);
    httpClient.post.mockReturnValue(throwError(() => ({ error: null, status: 0, message: '' })));

    await expect(firstValueFrom(service.post('/users/login', {}))).rejects.toMatchObject<ApiError>({
      error: 'API_ERROR',
      message: 'Error Code: 0',
      statusCode: 0
    });

    expect(toastService.showError).toHaveBeenCalledWith('Error Code: 0');
  });
});