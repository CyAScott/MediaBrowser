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
    localStorage.clear();
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

    await component.loadMediaById('ignored-id');

    expect(mocks.mediaService.get).not.toHaveBeenCalled();
    expect(component.mediaData?.id).toBe('from-nav');
  });

  it('loads media from MediaService when navigation state is missing', async () => {
    const fromService = createMediaReadModel('from-service');
    const { component, mocks } = await createComponent({ serviceMediaData: fromService });

    await component.loadMediaById('from-service');

    expect(mocks.mediaService.get).toHaveBeenCalledWith('from-service');
    expect(component.mediaData?.id).toBe('from-service');
  });

  it('handles media loading failures gracefully', async () => {
    const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    const { component, mocks } = await createComponent();
    mocks.mediaService.get.mockReturnValueOnce(throwError(() => new Error('network failed')));

    await component.loadMediaById('broken');

    expect(component.mediaData).toBeNull();
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
    component.mediaData = model;

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

    component.onVolumeChange({ target: audio } as Event);

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
    expect((component as any).hideTimeout).not.toBeNull();

    component.ngOnDestroy();

    expect((component as any).hideTimeout).toBeNull();
  });
});
