import { Location } from '@angular/common';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ImportService } from '../services/import.service';
import { MediaReadModel, MediaService } from '../services/media.service';
import { ImportComponent } from '../import/import';
import { SearchComponent } from '../search/search';
import { PeopleData, PeopleSectionComponent } from './people-section/people-section.component';
import { ThumbnailData } from './thumbnail-section/thumbnail-section.component';
import { MediaEditorComponent } from './media-editor';

function createMedia(overrides: Partial<MediaReadModel> = {}): MediaReadModel {
  return {
    id: 'media-1',
    path: '/tmp/media-1.mp4',
    title: 'Title One',
    originalTitle: 'Original One',
    description: 'Description One',
    mime: 'video/mp4',
    size: 1024,
    width: 1920,
    height: 1080,
    duration: 3661.125,
    md5: 'md5',
    rating: 4,
    userStarRating: 5,
    published: '2024',
    ctimeMs: '1000',
    mtimeMs: '2000',
    createdOn: new Date('2024-01-01T00:00:00.000Z'),
    updatedOn: new Date('2024-01-02T00:00:00.000Z'),
    ffprobe: {},
    cast: ['Cast A'],
    directors: ['Director A'],
    genres: ['Genre A'],
    producers: ['Producer A'],
    writers: ['Writer A'],
    url: 'https://example.test/media-1.mp4',
    thumbnail: 10,
    thumbnailUrl: 'https://example.test/thumb.jpg',
    fanartUrl: '',
    ...overrides
  };
}

describe('MediaEditorComponent', () => {
  let routeParams: Record<string, string | null>;
  let currentNavigation: { extras?: { state?: Record<string, unknown> } } | null;

  let mediaServiceMock: {
    get: ReturnType<typeof vi.fn>;
    getAllTags: ReturnType<typeof vi.fn>;
    update: ReturnType<typeof vi.fn>;
    updateThumbnail: ReturnType<typeof vi.fn>;
  };

  let importServiceMock: {
    readFileInfo: ReturnType<typeof vi.fn>;
    import: ReturnType<typeof vi.fn>;
  };

  let locationMock: {
    back: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    routeParams = {};
    currentNavigation = null;

    mediaServiceMock = {
      get: vi.fn().mockReturnValue(of(createMedia())),
      getAllTags: vi.fn().mockReturnValue(of([])),
      update: vi.fn().mockReturnValue(of(createMedia())),
      updateThumbnail: vi.fn().mockReturnValue(of(void 0))
    };

    importServiceMock = {
      readFileInfo: vi.fn().mockReturnValue(of({
        createdOn: new Date('2024-01-01T00:00:00.000Z'),
        ctimeMs: 1000,
        mime: 'video/mp4',
        mtimeMs: 2000,
        name: 'import-file.mp4',
        size: 2048,
        updatedOn: new Date('2024-01-02T00:00:00.000Z'),
        url: 'https://example.test/import.mp4'
      })),
      import: vi.fn().mockReturnValue(of(createMedia({ id: 'imported-id' })))
    };

    locationMock = {
      back: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [MediaEditorComponent],
      providers: [
        { provide: MediaService, useValue: mediaServiceMock },
        { provide: ImportService, useValue: importServiceMock },
        { provide: Location, useValue: locationMock },
        {
          provide: Router,
          useValue: {
            currentNavigation: vi.fn(() => currentNavigation)
          }
        },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: (key: string) => routeParams[key] ?? null
              }
            }
          }
        }
      ]
    }).compileComponents();
  });

  it('creates the component', () => {
    const fixture = TestBed.createComponent(MediaEditorComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('loads media by id on init and sets editable data', async () => {
    routeParams['id'] = 'media-1';

    const fixture = TestBed.createComponent(MediaEditorComponent);
    const component = fixture.componentInstance;
    const detectSpy = vi.spyOn((component as any).cdr, 'detectChanges');

    await component.ngOnInit();

    expect(mediaServiceMock.get).toHaveBeenCalledWith('media-1');
    expect(component.mediaId).toBe('media-1');
    expect(component.filename).toBeNull();
    expect(component.editableData.title).toBe('Title One');
    expect(component.thumbnail).toEqual({
      selectedImageFile: null,
      thumbnail: 10,
      thumbnailPreviewUrl: 'https://example.test/thumb.jpg'
    });
    expect(component.isLoading).toBe(false);
    expect(detectSpy).toHaveBeenCalledTimes(1);
  });

  it('loads media from navigation state for edit flow', async () => {
    routeParams['id'] = 'media-1';
    currentNavigation = {
      extras: {
        state: {
          mediaData: createMedia({ title: 'State Title', thumbnailUrl: '' })
        }
      }
    };

    const fixture = TestBed.createComponent(MediaEditorComponent);
    const component = fixture.componentInstance;

    await component.loadMediaById('media-1');

    expect(mediaServiceMock.get).not.toHaveBeenCalled();
    expect(component.mediaData?.title).toBe('State Title');
    expect(component.thumbnail?.thumbnailPreviewUrl).toBe('');
  });

  it('loads import media data on init when id is not present', async () => {
    routeParams['fileName'] = 'movie.mp4';

    const convertSpy = vi.spyOn(ImportComponent, 'convertToMediaReadModel');

    const fixture = TestBed.createComponent(MediaEditorComponent);
    const component = fixture.componentInstance;

    await component.ngOnInit();

    expect(importServiceMock.readFileInfo).toHaveBeenCalledWith('movie.mp4');
    expect(convertSpy).toHaveBeenCalledTimes(1);
    expect(component.filename).toBe('movie.mp4');
    expect(component.thumbnail).toBeNull();
    expect(component.mediaData?.title).toBe('import-file.mp4');

    convertSpy.mockRestore();
  });

  it('throws when loadMediaToImport has no filename or media data', async () => {
    routeParams['fileName'] = null;

    const fixture = TestBed.createComponent(MediaEditorComponent);
    const component = fixture.componentInstance;

    await expect(component.loadMediaToImport()).rejects.toThrow('No media data or filename found');
  });

  it('logs initialization errors and exits loading state', async () => {
    routeParams['fileName'] = 'movie.mp4';
    importServiceMock.readFileInfo.mockReturnValue(throwError(() => new Error('read failed')));

    const errorSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

    const fixture = TestBed.createComponent(MediaEditorComponent);
    const component = fixture.componentInstance;
    const detectSpy = vi.spyOn((component as any).cdr, 'detectChanges');

    await component.ngOnInit();

    expect(errorSpy).toHaveBeenCalledWith('Error during initialization:', expect.any(Error));
    expect(component.isLoading).toBe(false);
    expect(detectSpy).toHaveBeenCalledTimes(1);

    errorSpy.mockRestore();
  });

  it('returns early from saveChanges when mediaData is missing', async () => {
    const fixture = TestBed.createComponent(MediaEditorComponent);
    const component = fixture.componentInstance;

    component.mediaData = null;

    await component.saveChanges();

    expect(importServiceMock.import).not.toHaveBeenCalled();
    expect(mediaServiceMock.update).not.toHaveBeenCalled();
    expect(locationMock.back).not.toHaveBeenCalled();
  });

  it('imports media and uses thumbnail fallback for videos', async () => {
    const searchCacheSpy = vi.spyOn(SearchComponent, 'clearCachedResults').mockImplementation(() => undefined);
    const peopleCacheSpy = vi.spyOn(PeopleSectionComponent, 'clearCacheIfStale').mockImplementation(() => undefined);

    const fixture = TestBed.createComponent(MediaEditorComponent);
    const component = fixture.componentInstance;

    component.filename = 'movie.mp4';
    component.mediaData = createMedia({ mime: 'video/mp4' });
    component.thumbnail = {
      selectedImageFile: null,
      thumbnail: null,
      thumbnailPreviewUrl: ''
    };

    await component.saveChanges();

    expect(importServiceMock.import).toHaveBeenCalledWith('movie.mp4', {
      ...component.editableData,
      thumbnail: 0
    });
    expect(mediaServiceMock.updateThumbnail).not.toHaveBeenCalled();
    expect(searchCacheSpy).toHaveBeenCalledTimes(1);
    expect(peopleCacheSpy).toHaveBeenCalledWith(component.getPeopleData());
    expect(locationMock.back).toHaveBeenCalledTimes(1);

    searchCacheSpy.mockRestore();
    peopleCacheSpy.mockRestore();
  });

  it('imports media and uploads selected thumbnail file', async () => {
    const fixture = TestBed.createComponent(MediaEditorComponent);
    const component = fixture.componentInstance;

    component.filename = 'movie.mp4';
    component.mediaData = createMedia();
    component.thumbnail = {
      selectedImageFile: { name: 'thumb.jpg' } as File,
      thumbnail: null,
      thumbnailPreviewUrl: 'data:image/jpeg;base64,new'
    };

    await component.saveChanges();

    expect(importServiceMock.import).toHaveBeenCalledTimes(1);
    expect(mediaServiceMock.updateThumbnail).toHaveBeenCalledWith('imported-id', component.thumbnail.selectedImageFile);
  });

  it('updates existing media when editing by id', async () => {
    const fixture = TestBed.createComponent(MediaEditorComponent);
    const component = fixture.componentInstance;

    component.mediaId = 'media-77';
    component.mediaData = createMedia({ id: 'media-77' });

    await component.saveChanges();

    expect(mediaServiceMock.update).toHaveBeenCalledWith('media-77', {
      ...component.editableData
    });
    expect(importServiceMock.import).not.toHaveBeenCalled();
    expect(locationMock.back).toHaveBeenCalledTimes(1);
  });

  it('handles saveChanges errors and always resets saving flag', async () => {
    const errorSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

    const fixture = TestBed.createComponent(MediaEditorComponent);
    const component = fixture.componentInstance;
    const detectSpy = vi.spyOn((component as any).cdr, 'detectChanges');

    component.mediaId = 'media-88';
    component.mediaData = createMedia({ id: 'media-88' });
    mediaServiceMock.update.mockReturnValue(throwError(() => new Error('save failed')));

    await component.saveChanges();

    expect(component.isSaving).toBe(false);
    expect(errorSpy).toHaveBeenCalledWith('Error saving media changes:', expect.any(Error));
    expect(detectSpy).toHaveBeenCalledTimes(1);

    errorSpy.mockRestore();
  });

  it('supports cancel, formatting, getters and event handlers', () => {
    const fixture = TestBed.createComponent(MediaEditorComponent);
    const component = fixture.componentInstance;

    component.mediaData = createMedia();
    component.setEditableData(component.mediaData);

    component.cancel();
    expect(locationMock.back).toHaveBeenCalledTimes(1);

    expect(component.formatDuration()).toBe('00:00:0.000');
    expect(component.formatDuration(3661.5)).toBe('01:01:1.500');

    const localeSpy = vi.spyOn(Date.prototype, 'toLocaleString').mockReturnValue('formatted date');
    expect(component.formatDateTime('12345')).toBe('formatted date');
    localeSpy.mockRestore();

    expect(component.formatFileSize(1048576)).toBe('1.00 MB');

    expect(component.getTitleData()).toEqual({
      title: component.editableData.title,
      originalTitle: component.editableData.originalTitle,
      description: component.editableData.description
    });

    expect(component.getReadOnlyData()).toEqual({
      id: component.mediaData.id,
      duration: component.mediaData.duration,
      size: component.mediaData.size,
      md5: component.mediaData.md5,
      ctimeMs: component.mediaData.ctimeMs,
      mtimeMs: component.mediaData.mtimeMs,
      width: component.mediaData.width,
      height: component.mediaData.height,
      mime: component.mediaData.mime,
      rating: component.mediaData.rating,
      published: component.mediaData.published
    });

    expect(component.getThumbnailMediaData()).toEqual({
      mime: component.mediaData.mime,
      url: component.mediaData.url
    });

    component.onTitleDataChange({ title: 'T2', originalTitle: 'O2', description: 'D2' });
    component.onRatingChange(2);

    const people: PeopleData = {
      cast: ['Cast X'],
      directors: ['Director X'],
      genres: ['Genre X'],
      producers: ['Producer X'],
      writers: ['Writer X']
    };

    component.onPeopleDataChange(people);
    expect(component.getPeopleData()).toEqual(people);

    const thumbnail: ThumbnailData = {
      selectedImageFile: null,
      thumbnail: 12,
      thumbnailPreviewUrl: 'preview'
    };

    component.onThumbnailChange(thumbnail);
    component.onSetThumbnailPreview();

    expect(component.thumbnail).toEqual(thumbnail);
    expect(component.editableData.title).toBe('T2');
    expect(component.editableData.userStarRating).toBe(2);
  });

  it('returns early when onSaveThumbnail has no media id', async () => {
    const fixture = TestBed.createComponent(MediaEditorComponent);
    const component = fixture.componentInstance;

    component.mediaId = null;

    await component.onSaveThumbnail();

    expect(mediaServiceMock.updateThumbnail).not.toHaveBeenCalled();
  });

  it('saves thumbnail from selected file and resets creating state', async () => {
    const fixture = TestBed.createComponent(MediaEditorComponent);
    const component = fixture.componentInstance;
    const detectSpy = vi.spyOn((component as any).cdr, 'detectChanges');

    component.mediaId = 'media-1';
    component.thumbnail = {
      selectedImageFile: { name: 'new-thumb.jpg' } as File,
      thumbnail: 33,
      thumbnailPreviewUrl: 'preview'
    };

    await component.onSaveThumbnail();

    expect(mediaServiceMock.updateThumbnail).toHaveBeenCalledWith('media-1', component.thumbnail.selectedImageFile);
    expect(component.isCreatingThumbnail).toBe(false);
    expect(detectSpy).toHaveBeenCalledTimes(1);
  });

  it('saves thumbnail at timestamp and handles errors', async () => {
    const errorSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

    const fixture = TestBed.createComponent(MediaEditorComponent);
    const component = fixture.componentInstance;

    component.mediaId = 'media-2';
    component.thumbnail = {
      selectedImageFile: null,
      thumbnail: 44,
      thumbnailPreviewUrl: 'preview'
    };

    await component.onSaveThumbnail();

    expect(mediaServiceMock.updateThumbnail).toHaveBeenCalledWith('media-2', { at: 44 });

    mediaServiceMock.updateThumbnail.mockReturnValue(throwError(() => new Error('thumb failed')));
    await component.onSaveThumbnail();

    expect(errorSpy).toHaveBeenCalledWith('Error creating thumbnail:', expect.any(Error));
    expect(component.isCreatingThumbnail).toBe(false);

    errorSpy.mockRestore();
  });
});
