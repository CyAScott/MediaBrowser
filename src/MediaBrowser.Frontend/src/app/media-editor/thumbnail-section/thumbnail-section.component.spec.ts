import { ElementRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ThumbnailData, ThumbnailSectionComponent } from './thumbnail-section.component';

describe('ThumbnailSectionComponent', () => {
  const initialThumbnail: ThumbnailData = {
    thumbnail: 9,
    thumbnailPreviewUrl: 'data:image/jpeg;base64,initial',
    selectedImageFile: null
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ThumbnailSectionComponent]
    }).compileComponents();
  });

  it('creates and returns video URL fallback when mediaData url is missing', () => {
    const fixture = TestBed.createComponent(ThumbnailSectionComponent);
    const component = fixture.componentInstance;
    component.mediaData = { mime: 'video/mp4' };

    expect(component).toBeTruthy();
    expect(component.getVideoUrl()).toBe('');

    component.mediaData = { mime: 'video/mp4', url: 'https://video.test/file.mp4' };
    expect(component.getVideoUrl()).toBe('https://video.test/file.mp4');
  });

  it('loads initial thumbnail in ngAfterViewInit and emits the selection', () => {
    vi.useFakeTimers();

    const fixture = TestBed.createComponent(ThumbnailSectionComponent);
    const component = fixture.componentInstance;
    const emitSpy = vi.spyOn(component.thumbnailChange, 'emit');

    component.initialThumbnail = initialThumbnail;
    component.ngAfterViewInit();

    vi.runAllTimers();

    expect(component.thumbnails).toEqual([initialThumbnail]);
    expect(component.selectedThumbnail).toEqual(initialThumbnail);
    expect(component.selectedThumbnailIndex).toBe(0);
    expect(emitSpy).toHaveBeenCalledWith(initialThumbnail);

    vi.useRealTimers();
  });

  it('handles drag over/leave and drop states', () => {
    const fixture = TestBed.createComponent(ThumbnailSectionComponent);
    const component = fixture.componentInstance;

    const dragOver = {
      preventDefault: vi.fn(),
      stopPropagation: vi.fn()
    } as unknown as DragEvent;

    component.onDragOver(dragOver);
    expect(component.isDragOver).toBe(true);

    const dragLeave = {
      preventDefault: vi.fn(),
      stopPropagation: vi.fn()
    } as unknown as DragEvent;

    component.onDragLeave(dragLeave);
    expect(component.isDragOver).toBe(false);

    const dropNoFiles = {
      preventDefault: vi.fn(),
      stopPropagation: vi.fn(),
      dataTransfer: { files: [] }
    } as unknown as DragEvent;

    component.onDrop(dropNoFiles);
    expect(component.isDragOver).toBe(false);

    const errorSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);
    const dropWrongFile = {
      preventDefault: vi.fn(),
      stopPropagation: vi.fn(),
      dataTransfer: {
        files: [{ type: 'text/plain', size: 128, name: 'note.txt' }]
      }
    } as unknown as DragEvent;

    component.onDrop(dropWrongFile);
    expect(errorSpy).toHaveBeenCalledWith('Please select an image file');
    errorSpy.mockRestore();
  });

  it('validates selected files and handles successful image selection', () => {
    const fixture = TestBed.createComponent(ThumbnailSectionComponent);
    const component = fixture.componentInstance;
    const emitSpy = vi.spyOn(component.thumbnailChange, 'emit');
    const errorSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

    const largeImageFile = {
      type: 'image/png',
      size: 11 * 1024 * 1024,
      name: 'large.png'
    } as File;

    component.onFileSelected({ target: { files: [largeImageFile] } } as unknown as Event);
    expect(errorSpy).toHaveBeenCalledWith('File size must be less than 10MB');

    const detectSpy = vi.spyOn((component as any).cdr, 'detectChanges');
    class MockFileReader {
      onload: ((event: ProgressEvent<FileReader>) => void) | null = null;

      readAsDataURL(): void {
        this.onload?.({
          target: { result: 'data:image/jpeg;base64,new-preview' }
        } as unknown as ProgressEvent<FileReader>);
      }
    }
    vi.stubGlobal('FileReader', MockFileReader as unknown as typeof FileReader);

    const validImage = { type: 'image/jpeg', size: 5120, name: 'thumb.jpg' } as File;
    component.onFileSelected({ target: { files: [validImage] } } as unknown as Event);

    expect(component.selectedThumbnailIndex).toBe(0);
    expect(component.selectedThumbnail?.thumbnail).toBeNull();
    expect(component.selectedThumbnail?.thumbnailPreviewUrl).toBe('data:image/jpeg;base64,new-preview');
    expect(component.selectedThumbnail?.selectedImageFile).toEqual(validImage);
    expect(component.thumbnails).toHaveLength(1);
    expect(emitSpy).toHaveBeenCalledWith(component.selectedThumbnail);
    expect(detectSpy).toHaveBeenCalledTimes(1);

    vi.unstubAllGlobals();
    errorSpy.mockRestore();
  });

  it('mutes/focuses video and applies selected timestamp when metadata loads', () => {
    const fixture = TestBed.createComponent(ThumbnailSectionComponent);
    const component = fixture.componentInstance;

    const video = document.createElement('video');
    video.currentTime = 2;
    (video as HTMLVideoElement).focus = vi.fn();

    component.videoPlayer = new ElementRef(video);
    component.selectedThumbnail = {
      thumbnail: 14,
      thumbnailPreviewUrl: 'preview',
      selectedImageFile: null
    };

    component.onVideoMetadataLoaded();

    expect(video.muted).toBe(true);
    expect(video.volume).toBe(0);
    expect(video.currentTime).toBe(14);
    expect(video.focus).toHaveBeenCalledTimes(1);

    component.selectedThumbnail = {
      thumbnail: null,
      thumbnailPreviewUrl: 'preview',
      selectedImageFile: null
    };
    video.currentTime = 7;

    component.onVideoMetadataLoaded();
    expect(video.currentTime).toBe(7);
  });

  it('opens file browser and navigates thumbnails while emitting updates', () => {
    const fixture = TestBed.createComponent(ThumbnailSectionComponent);
    const component = fixture.componentInstance;
    const emitSpy = vi.spyOn(component.thumbnailChange, 'emit');

    const video = document.createElement('video');
    (video as HTMLVideoElement).focus = vi.fn();
    component.videoPlayer = new ElementRef(video);

    const fileInput = { click: vi.fn() } as unknown as HTMLInputElement;
    component.fileInput = new ElementRef(fileInput);

    component.openFileBrowser();
    expect(fileInput.click).toHaveBeenCalledTimes(1);

    const first = { thumbnail: 1, thumbnailPreviewUrl: 'a', selectedImageFile: null };
    const second = { thumbnail: 2, thumbnailPreviewUrl: 'b', selectedImageFile: null };

    component.thumbnails = [first, second];
    component.selectedThumbnailIndex = 0;
    component.selectedThumbnail = first;

    component.nextThumbnail();
    expect(component.selectedThumbnailIndex).toBe(1);
    expect(component.selectedThumbnail).toEqual(second);

    component.previousThumbnail();
    expect(component.selectedThumbnailIndex).toBe(0);
    expect(component.selectedThumbnail).toEqual(first);

    expect(emitSpy).toHaveBeenCalledTimes(2);
    expect(video.focus).toHaveBeenCalledTimes(2);
  });

  it('saves thumbnail and emits save event even when there is no selected thumbnail', () => {
    const fixture = TestBed.createComponent(ThumbnailSectionComponent);
    const component = fixture.componentInstance;

    const thumbnailEmitSpy = vi.spyOn(component.thumbnailChange, 'emit');
    const saveEmitSpy = vi.spyOn(component.saveThumbnailEvent, 'emit');

    const video = document.createElement('video');
    (video as HTMLVideoElement).focus = vi.fn();
    component.videoPlayer = new ElementRef(video);

    component.selectedThumbnail = null;
    component.saveThumbnail();

    expect(thumbnailEmitSpy).not.toHaveBeenCalled();
    expect(saveEmitSpy).toHaveBeenCalledTimes(1);
    expect(video.focus).toHaveBeenCalledTimes(1);
  });

  it('sets thumbnail preview from video and emits setPreview', () => {
    const fixture = TestBed.createComponent(ThumbnailSectionComponent);
    const component = fixture.componentInstance;

    component.mediaData = { mime: 'video/mp4', url: 'https://video.test/file.mp4' };
    const setPreviewSpy = vi.spyOn(component.setPreview, 'emit');
    const thumbnailEmitSpy = vi.spyOn(component.thumbnailChange, 'emit');

    const video = document.createElement('video');
    video.currentTime = 22;
    (video as HTMLVideoElement).focus = vi.fn();
    component.videoPlayer = new ElementRef(video);

    const previewSpy = vi
      .spyOn(component as any, 'generateThumbnailPreview')
      .mockReturnValue('data:image/jpeg;base64,frame');

    component.setThumbnailPreview();

    expect(component.selectedThumbnailIndex).toBe(0);
    expect(component.selectedThumbnail?.thumbnail).toBe(22);
    expect(component.selectedThumbnail?.selectedImageFile).toBeNull();
    expect(component.selectedThumbnail?.thumbnailPreviewUrl).toBe('data:image/jpeg;base64,frame');
    expect(component.thumbnails).toHaveLength(1);
    expect(thumbnailEmitSpy).toHaveBeenCalledWith(component.selectedThumbnail);
    expect(setPreviewSpy).toHaveBeenCalledTimes(1);
    expect(video.focus).toHaveBeenCalledTimes(1);

    previewSpy.mockRestore();
  });

  it('returns early when creating thumbnail or media data is missing', () => {
    const fixture = TestBed.createComponent(ThumbnailSectionComponent);
    const component = fixture.componentInstance;

    const setPreviewSpy = vi.spyOn(component.setPreview, 'emit');

    component.isCreatingThumbnail = true;
    component.mediaData = { mime: 'video/mp4' };
    component.setThumbnailPreview();

    component.isCreatingThumbnail = false;
    component.mediaData = null as any;
    component.setThumbnailPreview();

    expect(setPreviewSpy).not.toHaveBeenCalled();
  });

  it('falls back to file input click when setting preview without video player', () => {
    const fixture = TestBed.createComponent(ThumbnailSectionComponent);
    const component = fixture.componentInstance;

    component.mediaData = { mime: 'audio/mpeg' };
    const setPreviewSpy = vi.spyOn(component.setPreview, 'emit');

    const fileInput = { click: vi.fn() } as unknown as HTMLInputElement;
    component.fileInput = new ElementRef(fileInput);

    component.setThumbnailPreview();

    expect(fileInput.click).toHaveBeenCalledTimes(1);
    expect(setPreviewSpy).toHaveBeenCalledTimes(1);
  });

  it('generates thumbnail preview error paths when canvas context is unavailable or draw fails', () => {
    const fixture = TestBed.createComponent(ThumbnailSectionComponent);
    const component = fixture.componentInstance;
    const errorSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

    const originalCreateElement = document.createElement;

    const noContextCanvas = {
      getContext: vi.fn().mockReturnValue(null),
      toDataURL: vi.fn(),
      width: 0,
      height: 0
    } as unknown as HTMLCanvasElement;

    const createElementSpyNoContext = vi.spyOn(document, 'createElement').mockImplementation(((tagName: string) => {
      if (tagName === 'canvas') {
        return noContextCanvas;
      }
      return originalCreateElement.call(document, tagName);
    }) as typeof document.createElement);

    const previewWithNoContext = (component as any).generateThumbnailPreview(document.createElement('video'));
    expect(previewWithNoContext).toBe('');
    expect(errorSpy).toHaveBeenCalledWith('Could not get canvas context');

    createElementSpyNoContext.mockRestore();

    const failingCanvas = {
      getContext: vi.fn().mockReturnValue({ drawImage: vi.fn(() => { throw new Error('draw fail'); }) }),
      toDataURL: vi.fn().mockReturnValue('unused'),
      width: 0,
      height: 0
    } as unknown as HTMLCanvasElement;

    const createElementSpyDrawFail = vi.spyOn(document, 'createElement').mockImplementation(((tagName: string) => {
      if (tagName === 'canvas') {
        return failingCanvas;
      }
      return originalCreateElement.call(document, tagName);
    }) as typeof document.createElement);

    const previewWithDrawError = (component as any).generateThumbnailPreview(document.createElement('video'));
    expect(previewWithDrawError).toBe('');
    expect(errorSpy).toHaveBeenCalledWith('Error generating thumbnail preview:', expect.any(Error));

    createElementSpyDrawFail.mockRestore();
    errorSpy.mockRestore();
  });
});
