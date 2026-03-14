import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { of, throwError } from 'rxjs';
import { ImportComponent } from './import';
import { ImportFileInfo, ImportService } from '../services/import.service';

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
            files: vi.fn()
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
    const importService = TestBed.inject(ImportService) as { files: ReturnType<typeof vi.fn> };
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
    const importService = TestBed.inject(ImportService) as { files: ReturnType<typeof vi.fn> };
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
});