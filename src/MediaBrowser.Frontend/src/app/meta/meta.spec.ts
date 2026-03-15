import { ElementRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, ParamMap, Router } from '@angular/router';
import { BehaviorSubject, of, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { SearchComponent } from '../search/search';
import { MediaService } from '../services';
import { MetaComponent } from './meta';

type MetaTagType = 'cast' | 'directors' | 'genres' | 'producers' | 'writers';

interface MetaMocks {
  mediaService: {
    getAllTags: ReturnType<typeof vi.fn>;
    setThumbnailForTag: ReturnType<typeof vi.fn>;
  };
  routeParamMap$: BehaviorSubject<ParamMap>;
}

async function createComponent(overrides?: {
  cast?: string[];
  directors?: string[];
  genres?: string[];
  producers?: string[];
  writers?: string[];
}): Promise<{ component: MetaComponent; mocks: MetaMocks }> {
  const routeParamMap$ = new BehaviorSubject(convertToParamMap({ type: '' }));
  const valuesByType: Record<MetaTagType, string[]> = {
    cast: overrides?.cast ?? ['Cast One', 'Cast Two'],
    directors: overrides?.directors ?? ['Director One'],
    genres: overrides?.genres ?? ['Genre One'],
    producers: overrides?.producers ?? ['Producer One'],
    writers: overrides?.writers ?? ['Writer One']
  };
  const mocks: MetaMocks = {
    mediaService: {
      getAllTags: vi.fn((tagType: MetaTagType) => of(valuesByType[tagType])),
      setThumbnailForTag: vi.fn(() => of(void 0))
    },
    routeParamMap$
  };

  await TestBed.configureTestingModule({
    imports: [MetaComponent],
    providers: [
      { provide: MediaService, useValue: mocks.mediaService },
      { provide: ActivatedRoute, useValue: { paramMap: routeParamMap$.asObservable() } },
      { provide: Router, useValue: { navigate: vi.fn() } }
    ]
  }).compileComponents();

  const fixture = TestBed.createComponent(MetaComponent);
  return { component: fixture.componentInstance, mocks };
}

describe('MetaComponent', () => {
  beforeEach(() => {
    sessionStorage.clear();
    vi.restoreAllMocks();
  });

  it('creates the component', async () => {
    const { component } = await createComponent();
    expect(component).toBeTruthy();
  });

  it('loads meta info from route changes and restores saved scroll state', async () => {
    sessionStorage.setItem('cast-scroll-position', '135');
    const { component, mocks } = await createComponent({ cast: ['Keanu Reeves'] });

    await component.ngOnInit();
    mocks.routeParamMap$.next(convertToParamMap({ type: 'cast' }));
    await Promise.resolve();

    expect(component.type).toBe('cast');
    expect((component as any).scrollPosition).toBe(135);
    expect(mocks.mediaService.getAllTags).toHaveBeenCalledTimes(1);
    expect(mocks.mediaService.getAllTags).toHaveBeenCalledWith('cast');
    expect(component.metaMembers).toEqual([
      {
        name: 'Keanu Reeves',
        imageUrl: '/api/media/cast/Keanu%20Reeves/thumbnail',
        queryParams: { cast: ['Keanu Reeves'], sort: SearchComponent.DEFAULT_SORT }
      }
    ]);
  });

  it('does not reload metadata when route type stays the same', async () => {
    const { component, mocks } = await createComponent();
    const loadSpy = vi.spyOn(component, 'loadMetaInfo').mockResolvedValue();

    await component.ngOnInit();
    mocks.routeParamMap$.next(convertToParamMap({ type: 'genres' }));
    await Promise.resolve();
    mocks.routeParamMap$.next(convertToParamMap({ type: 'genres' }));
    await Promise.resolve();

    expect(loadSpy).toHaveBeenCalledTimes(1);
  });

  it('attaches scroll listener in ngAfterViewInit and persists scroll position', async () => {
    const { component } = await createComponent();
    const host = document.createElement('div');
    Object.defineProperty(host, 'scrollTop', { value: 0, writable: true });
    component.metaGrid = new ElementRef(host);
    component.type = 'cast';

    component.ngAfterViewInit();

    host.scrollTop = 42;
    host.dispatchEvent(new Event('scroll'));

    expect(sessionStorage.getItem('cast-scroll-position')).toBe('42');
    expect((component as any).scrollPosition).toBe(42);
  });

  it('restores scroll position in ngAfterViewInit when content already exists', async () => {
    const { component } = await createComponent();
    const host = document.createElement('div');
    Object.defineProperty(host, 'scrollTop', { value: 0, writable: true });

    component.metaGrid = new ElementRef(host);
    component.metaMembers = [
      { name: 'A', imageUrl: '/x', queryParams: { cast: ['A'], sort: SearchComponent.DEFAULT_SORT } }
    ];
    (component as any).scrollPosition = 90;

    const rafSpy = vi.fn((callback: FrameRequestCallback): number => {
      callback(0);
      return 1;
    });
    vi.stubGlobal('requestAnimationFrame', rafSpy);

    component.ngAfterViewInit();

    expect(rafSpy).toHaveBeenCalledTimes(1);
    expect(host.scrollTop).toBe(90);

    vi.unstubAllGlobals();
  });

  it('clears scroll position state and resets grid scrollTop', async () => {
    const { component } = await createComponent();
    const host = document.createElement('div');
    Object.defineProperty(host, 'scrollTop', { value: 0, writable: true });

    component.type = 'writers';
    component.metaGrid = new ElementRef(host);
    (component as any).scrollPosition = 77;
    sessionStorage.setItem('writers-scroll-position', '77');

    component.clearScrollPosition();

    expect((component as any).scrollPosition).toBe(0);
    expect(host.scrollTop).toBe(0);
    expect(sessionStorage.getItem('writers-scroll-position')).toBeNull();
  });

  it('loads each supported metadata type and builds expected image prefixes', async () => {
    const { component, mocks } = await createComponent({
      directors: ['Patty Jenkins'],
      genres: ['Action'],
      producers: ['Emma Thomas'],
      writers: ['Christopher Nolan']
    });

    const cases = [
      { type: 'directors', expectedPrefix: 'director' },
      { type: 'genres', expectedPrefix: 'genre' },
      { type: 'producers', expectedPrefix: 'producer' },
      { type: 'writers', expectedPrefix: 'writer' }
    ] as const;

    for (const testCase of cases) {
      component.type = testCase.type;
      await component.loadMetaInfo();

      expect(mocks.mediaService.getAllTags).toHaveBeenCalledWith(testCase.type);
      expect(component.metaMembers[0].imageUrl).toContain(`/api/media/${testCase.expectedPrefix}/`);
      expect(component.metaMembers[0].queryParams).toEqual({
        [testCase.type]: [component.metaMembers[0].name],
        sort: SearchComponent.DEFAULT_SORT
      });
    }
  });

  it('restores scroll after loadMetaInfo when grid exists and position is set', async () => {
    const { component } = await createComponent({ genres: ['Thriller'] });
    const host = document.createElement('div');
    Object.defineProperty(host, 'scrollTop', { value: 0, writable: true });

    component.type = 'genres';
    component.metaGrid = new ElementRef(host);
    (component as any).scrollPosition = 33;

    const rafSpy = vi.fn((callback: FrameRequestCallback): number => {
      callback(0);
      return 1;
    });
    vi.stubGlobal('requestAnimationFrame', rafSpy);

    await component.loadMetaInfo();

    expect(host.scrollTop).toBe(33);
    expect(rafSpy).toHaveBeenCalledTimes(1);
    vi.unstubAllGlobals();
  });

  it('handles unsupported type as empty results and still finalizes loading', async () => {
    const { component } = await createComponent();
    const detectChangesSpy = vi.spyOn((component as any).cdr, 'detectChanges');

    component.type = 'unknown';
    await component.loadMetaInfo();

    expect(component.metaMembers).toEqual([]);
    expect(component.isLoading).toBe(false);
    expect(detectChangesSpy).toHaveBeenCalledTimes(1);
  });

  it('logs load errors and resets loading state', async () => {
    const { component, mocks } = await createComponent();
    const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    const detectChangesSpy = vi.spyOn((component as any).cdr, 'detectChanges');

    mocks.mediaService.getAllTags.mockReturnValueOnce(throwError(() => new Error('network failed')));
    component.type = 'cast';

    await component.loadMetaInfo();

    expect(consoleErrorSpy).toHaveBeenCalledWith('Load error:', expect.any(Error));
    expect(component.isLoading).toBe(false);
    expect(detectChangesSpy).toHaveBeenCalledTimes(1);
  });

  it('cleans up listeners and subscriptions on destroy', async () => {
    const { component } = await createComponent();
    const host = document.createElement('div');
    const removeSpy = vi.spyOn(host, 'removeEventListener');
    const unsubscribe = vi.fn();

    component.metaGrid = new ElementRef(host);
    (component as any).scrollListener = vi.fn();
    (component as any).routeSubscription = { unsubscribe };

    component.ngOnDestroy();

    expect(removeSpy).toHaveBeenCalledWith('scroll', expect.any(Function));
    expect(unsubscribe).toHaveBeenCalledTimes(1);
  });

  it('delegates clearPagePositionState to SearchComponent static helper', async () => {
    const { component } = await createComponent();
    const clearSpy = vi.spyOn(SearchComponent, 'clearPagePositionState').mockImplementation(() => {});

    component.clearPagePositionState();

    expect(clearSpy).toHaveBeenCalledTimes(1);
  });

  it('opens the thumbnail file picker without triggering card navigation', async () => {
    const { component } = await createComponent();
    const input = document.createElement('input');
    const clickSpy = vi.spyOn(input, 'click');
    const preventDefault = vi.fn();
    const stopPropagation = vi.fn();

    component.openThumbnailUpload({ preventDefault, stopPropagation } as unknown as MouseEvent, input);

    expect(preventDefault).toHaveBeenCalledTimes(1);
    expect(stopPropagation).toHaveBeenCalledTimes(1);
    expect(clickSpy).toHaveBeenCalledTimes(1);
  });

  it('uploads selected thumbnail and refreshes image URL for matching member', async () => {
    const { component, mocks } = await createComponent();
    const detectChangesSpy = vi.spyOn((component as any).cdr, 'detectChanges');
    const input = document.createElement('input');
    const file = new File(['thumb'], 'thumb.png', { type: 'image/png' });
    const metaMember = {
      name: 'Keanu Reeves',
      imageUrl: '/api/media/cast/Keanu%20Reeves/thumbnail',
      queryParams: { cast: ['Keanu Reeves'], sort: SearchComponent.DEFAULT_SORT }
    };

    Object.defineProperty(input, 'files', { value: [file], configurable: true });
    component.type = 'cast';

    await component.onThumbnailSelected({ target: input } as unknown as Event, metaMember);

    expect(mocks.mediaService.setThumbnailForTag).toHaveBeenCalledWith('cast', 'Keanu Reeves', file);
    expect(metaMember.imageUrl).toContain('/api/media/cast/Keanu%20Reeves/thumbnail?t=');
    expect(component.isUploading('Keanu Reeves')).toBe(false);
    expect(detectChangesSpy).toHaveBeenCalledTimes(1);
  });

  it('skips thumbnail upload when the current type is unsupported', async () => {
    const { component, mocks } = await createComponent();
    const input = document.createElement('input');
    const file = new File(['thumb'], 'thumb.png', { type: 'image/png' });
    const metaMember = {
      name: 'Keanu Reeves',
      imageUrl: '/api/media/cast/Keanu%20Reeves/thumbnail',
      queryParams: { cast: ['Keanu Reeves'], sort: SearchComponent.DEFAULT_SORT }
    };

    Object.defineProperty(input, 'files', { value: [file], configurable: true });
    component.type = 'unknown';

    await component.onThumbnailSelected({ target: input } as unknown as Event, metaMember);

    expect(mocks.mediaService.setThumbnailForTag).not.toHaveBeenCalled();
    expect(metaMember.imageUrl).toBe('/api/media/cast/Keanu%20Reeves/thumbnail');
  });
});
