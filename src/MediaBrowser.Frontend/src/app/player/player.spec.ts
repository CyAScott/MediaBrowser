import { ElementRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Navigation, Router } from '@angular/router';
import { Location } from '@angular/common';
import { beforeEach, afterEach, describe, expect, it, vi } from 'vitest';
import { of, throwError } from 'rxjs';
import { MediaReadModel, MediaService } from '../services';
import { PlayerComponent } from './player';

interface PlayerMocks {
  activatedRoute: {
    snapshot: {
      paramMap: {
        get: ReturnType<typeof vi.fn>;
      };
    };
  };
  location: {
    back: ReturnType<typeof vi.fn>;
  };
  mediaService: {
    get: ReturnType<typeof vi.fn>;
  };
  router: {
    currentNavigation: ReturnType<typeof vi.fn>;
    navigate: ReturnType<typeof vi.fn>;
  };
}

function createMediaReadModel(id = 'media-1', mime = 'video/mp4'): MediaReadModel {
  return {
    id,
    path: `/tmp/${id}`,
    title: `Title ${id}`,
    originalTitle: `Original ${id}`,
    description: `Description ${id}`,
    mime,
    md5: `${id}-md5`,
    published: '2024-01-01',
    ctimeMs: '0',
    mtimeMs: '0',
    createdOn: new Date('2024-01-01T00:00:00.000Z'),
    updatedOn: new Date('2024-01-02T00:00:00.000Z'),
    ffprobe: {},
    cast: [],
    directors: [],
    genres: [],
    producers: [],
    writers: [],
    url: `https://example.com/${id}`,
    duration: 120,
    thumbnailUrl: `https://example.com/${id}.jpg`
  };
}

async function createComponent(overrides?: {
  routeId?: string | null;
  navigationMediaData?: MediaReadModel | null;
  serviceMediaData?: MediaReadModel;
}): Promise<{ component: PlayerComponent; mocks: PlayerMocks }> {
  const navigationState = overrides?.navigationMediaData
    ? { extras: { state: { mediaData: overrides.navigationMediaData } } }
    : null;

  const mocks: PlayerMocks = {
    activatedRoute: {
      snapshot: {
        paramMap: {
          get: vi.fn().mockReturnValue(overrides?.routeId ?? null)
        }
      }
    },
    location: {
      back: vi.fn()
    },
    mediaService: {
      get: vi.fn().mockReturnValue(of(overrides?.serviceMediaData ?? createMediaReadModel('service-media')))
    },
    router: {
      currentNavigation: vi.fn().mockReturnValue(navigationState as Navigation | null),
      navigate: vi.fn().mockResolvedValue(true)
    }
  };

  await TestBed.configureTestingModule({
    imports: [PlayerComponent],
    providers: [
      { provide: ActivatedRoute, useValue: mocks.activatedRoute },
      { provide: Location, useValue: mocks.location },
      { provide: MediaService, useValue: mocks.mediaService },
      { provide: Router, useValue: mocks.router }
    ]
  }).compileComponents();

  const fixture = TestBed.createComponent(PlayerComponent);
  return { component: fixture.componentInstance, mocks };
}

describe('PlayerComponent', () => {
  beforeEach(() => {
    let store: Record<string, string> = {};
    const localStorageMock = {
      getItem: vi.fn((key: string) => store[key] || null),
      setItem: vi.fn((key: string, value: string) => {
        store[key] = value.toString();
      }),
      clear: vi.fn(() => {
        store = {};
      }),
      removeItem: vi.fn((key: string) => {
        delete store[key];
      }),
      key: vi.fn(),
      length: 0,
    };
    vi.stubGlobal('localStorage', localStorageMock);
    
    vi.useFakeTimers();
    vi.restoreAllMocks();
  });

  afterEach(() => {
    vi.runOnlyPendingTimers();
    vi.useRealTimers();
  });

  it('creates component and captures current navigation in constructor', async () => {
    const fromNavigation = createMediaReadModel('from-nav');
    const { component, mocks } = await createComponent({ navigationMediaData: fromNavigation });

    expect(component).toBeTruthy();
    expect(mocks.router.currentNavigation).toHaveBeenCalledTimes(1);
  });

  it('loads media from navigation state before calling service', async () => {
    const fromNavigation = createMediaReadModel('from-nav');
    const { component, mocks } = await createComponent({ navigationMediaData: fromNavigation });

    component.mediaId = 'from-nav';
    await component.loadMedia();

    expect(mocks.mediaService.get).not.toHaveBeenCalled();
    expect(component.state?.mediaData?.id).toBe('from-nav');
  });

  it('loads media from MediaService when navigation state is missing', async () => {
    const fromService = createMediaReadModel('from-service');
    const { component, mocks } = await createComponent({ serviceMediaData: fromService });

    component.mediaId = 'from-service';
    await component.loadMedia();

    expect(mocks.mediaService.get).toHaveBeenCalledWith('from-service');
    expect(component.state?.mediaData?.id).toBe('from-service');
  });

  it('handles media loading failures gracefully', async () => {
    const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    const { component, mocks } = await createComponent();
    mocks.mediaService.get.mockReturnValueOnce(throwError(() => new Error('network failed')));

    component.mediaId = 'broken';
    await component.loadMedia();

    expect(component.state?.mediaData).toBeUndefined();
    expect(consoleErrorSpy).toHaveBeenCalled();
  });

  it('goes back using Location service', async () => {
    const { component, mocks } = await createComponent();

    component.goBack();

    expect(mocks.location.back).toHaveBeenCalledTimes(1);
  });

  it('navigates to edit route when mediaId exists', async () => {
    const model = createMediaReadModel('edit-id');
    const { component, mocks } = await createComponent();
    component.mediaId = 'edit-id';
    component.state = { mediaData: model };

    component.editMedia();

    expect(mocks.router.navigate).toHaveBeenCalledWith(['/edit', 'edit-id'], {
      state: { mediaData: model }
    });
  });

  it('does not navigate to edit route when mediaId is missing', async () => {
    const { component, mocks } = await createComponent();

    component.editMedia();

    expect(mocks.router.navigate).not.toHaveBeenCalled();
  });

  it('toggles chapter panel visibility', async () => {
    const { component } = await createComponent();

    component.state = { mediaData: createMediaReadModel() };
    const video = document.createElement('video');
    Object.defineProperty(video, 'duration', { value: 120 });
    // @ts-ignore
    video.pause = vi.fn();
    component.videoElement = new ElementRef(video);
    
    expect(component.chapterPanelVisible).toBe(false);

    component.toggleChapterPanel();
    expect(component.chapterPanelVisible).toBe(true);

    component.toggleChapterPanel();
    expect(component.chapterPanelVisible).toBe(false);
  });

  it('updates chapter range and seeks media element while dragging handles', async () => {
    const { component } = await createComponent();
    component.chapterStart = 10;
    component.chapterEnd = 40;

    const video = document.createElement('video');
    component.videoElement = new ElementRef(video);

    component.onRangeInput('start', {
      target: { value: '15' }
    } as unknown as Event);

    expect(component.chapterStart).toBe(15);
    expect(video.currentTime).toBe(15);

    component.onRangeInput('end', {
      target: { value: '32' }
    } as unknown as Event);

    expect(component.chapterEnd).toBe(32);
    expect(video.currentTime).toBe(32);
  });

  it('navigates to add chapter route with range in query params and state', async () => {
    const mediaData = createMediaReadModel('chapter-parent');
    const { component, mocks } = await createComponent();
    component.mediaId = 'chapter-parent';
    component.state = { mediaData };
    component.chapterStart = 12.5;
    component.chapterEnd = 45.2;

    component.goToAddChapter();

    expect(mocks.router.navigate).toHaveBeenCalledWith(['/edit', 'chapter-parent', 12.5, 45.2, 'chapter'], {
      state: {
        mediaData
      }
    });
  });

  it('does not navigate to add chapter route when range is invalid', async () => {
    const { component, mocks } = await createComponent();
    component.mediaId = 'chapter-parent';
    component.chapterStart = 10;
    component.chapterEnd = 10;

    component.goToAddChapter();

    expect(mocks.router.navigate).not.toHaveBeenCalled();
  });

  it('initializes with route id, loads media, and hides header after timeout', async () => {
    const fromService = createMediaReadModel('route-media');
    const { component, mocks } = await createComponent({
      routeId: 'route-media',
      serviceMediaData: fromService
    });

    await component.ngOnInit();
    await Promise.resolve();

    expect(component.mediaId).toBe('route-media');
    expect(mocks.mediaService.get).toHaveBeenCalledWith('route-media');
    expect(component.hasHistory).toBe(window.history.length > 1);
    expect(component.headerVisible).toBe(true);

    vi.advanceTimersByTime(5000);

    expect(component.headerVisible).toBe(false);
  });

  it('keeps header visible when mouse moves and hide timer is restarted', async () => {
    const { component } = await createComponent();
    component.headerVisible = false;

    component.onMouseMove();

    expect(component.headerVisible).toBe(true);

    vi.advanceTimersByTime(4999);
    expect(component.headerVisible).toBe(true);

    vi.advanceTimersByTime(1);
    expect(component.headerVisible).toBe(false);
  });

  it('restarts hide timer on mouse leave so only the latest timer applies', async () => {
    const { component } = await createComponent();

    component.onMouseLeave();
    vi.advanceTimersByTime(3000);
    component.headerVisible = true;
    component.onMouseLeave();

    vi.advanceTimersByTime(2001);
    expect(component.headerVisible).toBe(true);

    vi.advanceTimersByTime(2999);
    expect(component.headerVisible).toBe(false);
  });

  it('applies saved volume to both video and audio elements', async () => {
    const { component } = await createComponent();
    localStorage.setItem('mediaBrowser_volume', '0.35');

    const video = document.createElement('video');
    const audio = document.createElement('audio');
    component.videoElement = new ElementRef(video);
    component.audioElement = new ElementRef(audio);

    component.applySavedVolume();

    expect(video.volume).toBeCloseTo(0.35);
    expect(audio.volume).toBeCloseTo(0.35);
  });

  it('defaults volume to 1.0 when no saved value exists', async () => {
    const { component } = await createComponent();

    const video = document.createElement('video');
    component.videoElement = new ElementRef(video);

    component.applySavedVolume();

    expect(video.volume).toBe(1);
  });

  it('saves volume when media element volume changes', async () => {
    const { component } = await createComponent();
    const audio = document.createElement('audio');
    audio.volume = 0.6;

    component.onVolumeChange({ target: audio } as unknown as Event);

    expect(localStorage.getItem('mediaBrowser_volume')).toBe('0.6');
  });

  it('applies saved volume in ngAfterViewInit lifecycle hook', async () => {
    const { component } = await createComponent();
    localStorage.setItem('mediaBrowser_volume', '0.25');
    const audio = document.createElement('audio');
    component.audioElement = new ElementRef(audio);

    component.ngAfterViewInit();

    expect(audio.volume).toBeCloseTo(0.25);
  });

  it('clears pending hide timer in ngOnDestroy', async () => {
    const { component } = await createComponent();

    component.onMouseLeave();
    expect((component as any).hideTimeout).toBeDefined();

    component.ngOnDestroy();

    expect((component as any).hideTimeout).toBeUndefined();
  });

  it('navigates to previous item on ArrowLeft for image media', async () => {
    const { component } = await createComponent();
    component.state = {
      mediaData: createMediaReadModel('image-current', 'image/jpeg'),
      searchContext: {
        currentIndex: 1,
        searchParams: {} as any
      }
    };
    component.searchResponse = {
      results: [
        createMediaReadModel('image-previous', 'image/jpeg'),
        createMediaReadModel('image-current', 'image/jpeg'),
        createMediaReadModel('image-next', 'image/jpeg')
      ],
      count: 3
    };

    const goToPreviousSpy = vi.spyOn(component, 'goToPrevious').mockResolvedValue();
    const preventDefault = vi.fn();

    component.onDocumentKeyDown({ key: 'ArrowLeft', preventDefault, target: document.body } as unknown as KeyboardEvent);

    expect(preventDefault).toHaveBeenCalledTimes(1);
    expect(goToPreviousSpy).toHaveBeenCalledTimes(1);
  });

  it('navigates to next item on ArrowRight for image media', async () => {
    const { component } = await createComponent();
    component.state = {
      mediaData: createMediaReadModel('image-current', 'image/jpeg'),
      searchContext: {
        currentIndex: 1,
        searchParams: {} as any
      }
    };
    component.searchResponse = {
      results: [
        createMediaReadModel('image-previous', 'image/jpeg'),
        createMediaReadModel('image-current', 'image/jpeg'),
        createMediaReadModel('image-next', 'image/jpeg')
      ],
      count: 3
    };

    const goToNextSpy = vi.spyOn(component, 'goToNext').mockResolvedValue();
    const preventDefault = vi.fn();

    component.onDocumentKeyDown({ key: 'ArrowRight', preventDefault, target: document.body } as unknown as KeyboardEvent);

    expect(preventDefault).toHaveBeenCalledTimes(1);
    expect(goToNextSpy).toHaveBeenCalledTimes(1);
  });

  it('does not navigate on arrow keys when media is not an image', async () => {
    const { component } = await createComponent();
    component.state = {
      mediaData: createMediaReadModel('video-current', 'video/mp4'),
      searchContext: {
        currentIndex: 1,
        searchParams: {} as any
      }
    };
    component.searchResponse = {
      results: [createMediaReadModel('video-previous'), createMediaReadModel('video-current'), createMediaReadModel('video-next')],
      count: 3
    };

    const goToNextSpy = vi.spyOn(component, 'goToNext').mockResolvedValue();
    const preventDefault = vi.fn();

    component.onDocumentKeyDown({ key: 'ArrowRight', preventDefault, target: document.body } as unknown as KeyboardEvent);

    expect(preventDefault).not.toHaveBeenCalled();
    expect(goToNextSpy).not.toHaveBeenCalled();
  });

  it('does not navigate on arrow keys when focus is on interactive elements', async () => {
    const { component } = await createComponent();
    component.state = {
      mediaData: createMediaReadModel('image-current', 'image/jpeg'),
      searchContext: {
        currentIndex: 1,
        searchParams: {} as any
      }
    };
    component.searchResponse = {
      results: [
        createMediaReadModel('image-previous', 'image/jpeg'),
        createMediaReadModel('image-current', 'image/jpeg'),
        createMediaReadModel('image-next', 'image/jpeg')
      ],
      count: 3
    };

    const goToNextSpy = vi.spyOn(component, 'goToNext').mockResolvedValue();
    const preventDefault = vi.fn();
    const input = document.createElement('input');

    component.onDocumentKeyDown({ key: 'ArrowRight', preventDefault, target: input } as unknown as KeyboardEvent);

    expect(preventDefault).not.toHaveBeenCalled();
    expect(goToNextSpy).not.toHaveBeenCalled();
  });
});
