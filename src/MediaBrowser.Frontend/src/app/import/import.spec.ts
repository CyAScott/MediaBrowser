import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { of, throwError } from 'rxjs';
import { ImportComponent } from './import';
import { ImportFileInfo, ImportService } from '../services/import.service';

type ImportServiceMock = {
  files: ReturnType<typeof vi.fn>;
  uploadFile: ReturnType<typeof vi.fn>;
};

function getImportServiceMock(): ImportServiceMock {
  return TestBed.inject(ImportService) as unknown as ImportServiceMock;
}

function createImportFileInfo(overrides?: Partial<ImportFileInfo>): ImportFileInfo {
  return {
    createdOn: new Date('2025-01-01T00:00:00.000Z'),
    ctimeMs: 1735689600000,
    mime: 'video/mp4',
    mtimeMs: 1735776000000,
    name: 'movie.mp4',
    size: 1234,
    updatedOn: new Date('2025-01-02T00:00:00.000Z'),
    url: 'https://example.com/movie.mp4',
    ...overrides
  };
}

describe('ImportComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImportComponent],
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([]),
        {
          provide: ImportService,
          useValue: {
            files: vi.fn(),
            uploadFile: vi.fn()
          }
        }
      ]
    }).compileComponents();
  });

  it('creates the component', () => {
    const fixture = TestBed.createComponent(ImportComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('converts ImportFileInfo to MediaReadModel', () => {
    const file = createImportFileInfo({
      name: 'sample.mkv',
      ctimeMs: 100,
      mtimeMs: 200,
      mime: 'video/x-matroska'
    });

    const model = ImportComponent.convertToMediaReadModel(file);

    expect(model.title).toBe('sample.mkv');
    expect(model.originalTitle).toBe('sample.mkv');
    expect(model.mime).toBe('video/x-matroska');
    expect(model.ctimeMs).toBe('100');
    expect(model.mtimeMs).toBe('200');
    expect(model.url).toBe(file.url);
    expect(model.cast).toEqual([]);
    expect(model.directors).toEqual([]);
    expect(model.genres).toEqual([]);
    expect(model.producers).toEqual([]);
    expect(model.writers).toEqual([]);
    expect(model.ffprobe).toEqual({});
  });

  it('loads files on init and maps them to MediaReadModel', async () => {
    const importService = getImportServiceMock();
    const file = createImportFileInfo({ name: 'loaded.mp4' });
    importService.files.mockReturnValue(of([file]));

    const fixture = TestBed.createComponent(ImportComponent);
    const component = fixture.componentInstance;
    const detectChangesSpy = vi.spyOn((component as unknown as { cdr: { detectChanges: () => void } }).cdr, 'detectChanges');

    fixture.detectChanges();
    await fixture.whenStable();

    expect(importService.files).toHaveBeenCalledTimes(1);
    expect(component.files).toHaveLength(1);
    expect(component.files[0].title).toBe('loaded.mp4');
    expect(component.files[0].ctimeMs).toBe(file.ctimeMs.toString());
    expect(detectChangesSpy).toHaveBeenCalled();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelectorAll('tbody tr')).toHaveLength(1);
    expect(compiled.querySelector('.filename')?.textContent).toContain('loaded.mp4');
  });

  it('handles scan errors by clearing files and logging the error', async () => {
    const importService = getImportServiceMock();
    const error = new Error('scan failed');
    importService.files.mockReturnValue(throwError(() => error));

    const fixture = TestBed.createComponent(ImportComponent);
    const component = fixture.componentInstance;
    component.files = [ImportComponent.convertToMediaReadModel(createImportFileInfo())];

    const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    const detectChangesSpy = vi.spyOn((component as unknown as { cdr: { detectChanges: () => void } }).cdr, 'detectChanges');

    fixture.detectChanges();
    await fixture.whenStable();

    expect(importService.files).toHaveBeenCalledTimes(1);
    expect(component.files).toEqual([]);
    expect(consoleErrorSpy).toHaveBeenCalledWith('Error scanning directory:', error);
    expect(detectChangesSpy).toHaveBeenCalled();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.empty-state p')?.textContent).toContain(
      'No media files found in the selected directory.'
    );

    consoleErrorSpy.mockRestore();
  });

  it('uploads a dropped file and refreshes the files list', async () => {
    const importService = getImportServiceMock();
    const initialFiles = [createImportFileInfo({ name: 'initial.mp4' })];
    const uploadedFiles = [createImportFileInfo({ name: 'uploaded.mp4' })];
    importService.files
      .mockReturnValueOnce(of(initialFiles))
      .mockReturnValueOnce(of(uploadedFiles));
    importService.uploadFile.mockReturnValue(of(void 0));

    const fixture = TestBed.createComponent(ImportComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();

    const droppedFile = new File(['video content'], 'uploaded.mp4', { type: 'video/mp4' });
    const preventDefault = vi.fn();
    await component.onDrop({
      preventDefault,
      dataTransfer: {
        files: {
          item: vi.fn().mockReturnValue(droppedFile)
        }
      }
    } as unknown as DragEvent);
    await fixture.whenStable();

    expect(preventDefault).toHaveBeenCalledTimes(1);
    expect(importService.uploadFile).toHaveBeenCalledWith(droppedFile);
    expect(importService.files).toHaveBeenCalledTimes(2);
    expect(component.files).toHaveLength(1);
    expect(component.files[0].title).toBe('uploaded.mp4');
    expect(component.uploadError).toBeNull();
  });

  it('shows an upload error when dropped file upload fails', async () => {
    const importService = getImportServiceMock();
    const uploadError = new Error('upload failed');
    importService.files.mockReturnValue(of([]));
    importService.uploadFile.mockReturnValue(throwError(() => uploadError));

    const fixture = TestBed.createComponent(ImportComponent);
    const component = fixture.componentInstance;
    const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

    fixture.detectChanges();
    await fixture.whenStable();

    const droppedFile = new File(['video content'], 'uploaded.mp4', { type: 'video/mp4' });
    await component.onDrop({
      preventDefault: vi.fn(),
      dataTransfer: {
        files: {
          item: vi.fn().mockReturnValue(droppedFile)
        }
      }
    } as unknown as DragEvent);
    await fixture.whenStable();

    expect(importService.uploadFile).toHaveBeenCalledWith(droppedFile);
    expect(component.uploadError).toBe('Failed to upload file. Please try again.');
    expect(consoleErrorSpy).toHaveBeenCalledWith('Error uploading file:', uploadError);

    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.upload-error')?.textContent).toContain('Failed to upload file. Please try again.');

    consoleErrorSpy.mockRestore();
  });

  it('rejects non-media files before upload', async () => {
    const importService = getImportServiceMock();
    importService.files.mockReturnValue(of([]));

    const fixture = TestBed.createComponent(ImportComponent);
    const component = fixture.componentInstance;

    fixture.detectChanges();
    await fixture.whenStable();

    const invalidFile = new File(['not media'], 'notes.txt', { type: 'text/plain' });
    await component.onDrop({
      preventDefault: vi.fn(),
      dataTransfer: {
        files: {
          item: vi.fn().mockReturnValue(invalidFile)
        }
      }
    } as unknown as DragEvent);
    await fixture.whenStable();

    expect(importService.uploadFile).not.toHaveBeenCalled();
    expect(component.uploadError).toBe('Only audio and video files are allowed.');

    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.upload-error')?.textContent).toContain('Only audio and video files are allowed.');
  });
});
