import { ElementRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, ParamMap, Router } from '@angular/router';
import { BehaviorSubject, of, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { SearchComponent } from '../search/search';
import { MediaService } from '../services';
import { MetaComponent } from './meta';

interface MetaMocks {
  mediaService: {
    getAllCast: ReturnType<typeof vi.fn>;
    getAllDirectors: ReturnType<typeof vi.fn>;
    getAllGenres: ReturnType<typeof vi.fn>;
    getAllProducers: ReturnType<typeof vi.fn>;
    getAllWriters: ReturnType<typeof vi.fn>;
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
  const mocks: MetaMocks = {
    mediaService: {
      getAllCast: vi.fn().mockReturnValue(of(overrides?.cast ?? ['Cast One', 'Cast Two'])),
      getAllDirectors: vi.fn().mockReturnValue(of(overrides?.directors ?? ['Director One'])),
      getAllGenres: vi.fn().mockReturnValue(of(overrides?.genres ?? ['Genre One'])),
      getAllProducers: vi.fn().mockReturnValue(of(overrides?.producers ?? ['Producer One'])),
      getAllWriters: vi.fn().mockReturnValue(of(overrides?.writers ?? ['Writer One']))
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
    expect(mocks.mediaService.getAllCast).toHaveBeenCalledTimes(1);
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
      { type: 'directors', expectedPrefix: 'director', serviceSpy: mocks.mediaService.getAllDirectors },
      { type: 'genres', expectedPrefix: 'genre', serviceSpy: mocks.mediaService.getAllGenres },
      { type: 'producers', expectedPrefix: 'producer', serviceSpy: mocks.mediaService.getAllProducers },
      { type: 'writers', expectedPrefix: 'writer', serviceSpy: mocks.mediaService.getAllWriters }
    ] as const;

    for (const testCase of cases) {
      component.type = testCase.type;
      await component.loadMetaInfo();

      expect(testCase.serviceSpy).toHaveBeenCalled();
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

    mocks.mediaService.getAllCast.mockReturnValueOnce(throwError(() => new Error('network failed')));
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
});
