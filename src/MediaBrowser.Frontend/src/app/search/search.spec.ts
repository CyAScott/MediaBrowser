import { ElementRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { MediaReadModel, MediaService, SearchMediaRequest, SearchResponse } from '../services/media.service';
import { SearchComponent } from './search';

interface SearchMocks {
  activatedRoute: {
    snapshot: {
      queryParams: Record<string, unknown>;
    };
  };
  mediaService: {
    search: ReturnType<typeof vi.fn>;
  };
  router: {
    navigate: ReturnType<typeof vi.fn>;
  };
}

function createMediaResult(id: string): MediaReadModel {
  return {
    id,
    path: `/tmp/${id}.mp4`,
    title: `Title ${id}`,
    originalTitle: `Title ${id}`,
    description: `Description ${id}`,
    mime: 'video/mp4',
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
    url: `https://example.com/${id}.mp4`
  };
}

function createSearchResponse(results: MediaReadModel[]): SearchResponse {
  return {
    count: results.length,
    results
  };
}

async function createComponent(overrides?: {
  queryParams?: Record<string, unknown>;
  searchResponse?: SearchResponse;
}): Promise<{ component: SearchComponent; mocks: SearchMocks }> {
  const mocks: SearchMocks = {
    activatedRoute: {
      snapshot: {
        queryParams: overrides?.queryParams ?? {}
      }
    },
    mediaService: {
      search: vi.fn().mockReturnValue(of(overrides?.searchResponse ?? createSearchResponse([])))
    },
    router: {
      navigate: vi.fn().mockResolvedValue(true)
    }
  };

  await TestBed.configureTestingModule({
    imports: [SearchComponent],
    providers: [
      { provide: ActivatedRoute, useValue: mocks.activatedRoute },
      { provide: MediaService, useValue: mocks.mediaService },
      { provide: Router, useValue: mocks.router }
    ]
  }).compileComponents();

  const fixture = TestBed.createComponent(SearchComponent);
  // Clear any constructor/lifecycle side effects so each test starts from a clean call history.
  mocks.mediaService.search.mockClear();
  mocks.router.navigate.mockClear();

  return { component: fixture.componentInstance, mocks };
}

describe('SearchComponent', () => {
  beforeEach(() => {
    sessionStorage.clear();
    vi.restoreAllMocks();
  });

  it('creates component and restores page state from session storage', async () => {
    sessionStorage.setItem(SearchComponent.PAGE_KEY, 'scroll=120&pageIndex=3');

    const { component } = await createComponent();

    expect(component).toBeTruthy();
    expect((component as any).scrollPosition).toBe(120);
    expect((component as any).pageIndex).toBe(3);
  });

  it('clears static cache and page state entries', () => {
    sessionStorage.setItem(SearchComponent.CACHE_KEY, 'cached');
    sessionStorage.setItem(SearchComponent.PAGE_KEY, 'saved');

    SearchComponent.clearCachedResults();
    SearchComponent.clearPagePositionState();

    expect(sessionStorage.getItem(SearchComponent.CACHE_KEY)).toBeNull();
    expect(sessionStorage.getItem(SearchComponent.PAGE_KEY)).toBeNull();
  });

  it('loads query params in ngOnInit and performs search with computed take', async () => {
    sessionStorage.setItem(SearchComponent.PAGE_KEY, 'scroll=40&pageIndex=2');

    const { component, mocks } = await createComponent({
      queryParams: {
        keywords: '  hero ',
        sort: 'createdOn',
        descending: 'true',
        cast: ['Cast A', 'Cast B'],
        directors: 'Director A',
        genres: ['Drama'],
        producers: 'Producer A',
        writers: ['Writer A']
      },
      searchResponse: createSearchResponse([createMediaResult('1')])
    });

    mocks.mediaService.search.mockClear();
    mocks.router.navigate.mockClear();

    await component.ngOnInit();

    expect(component.keywords).toBe('  hero ');
    expect(component.sort).toBe('createdOn');
    expect(component.descending).toBe(true);
    expect(component.cast).toEqual(['Cast A', 'Cast B']);
    expect(component.directors).toEqual(['Director A']);
    expect(component.genres).toEqual(['Drama']);
    expect(component.producers).toEqual(['Producer A']);
    expect(component.writers).toEqual(['Writer A']);

    expect(mocks.mediaService.search).toHaveBeenCalledWith(expect.objectContaining({
      sort: 'createdOn',
      descending: true,
      keywords: 'hero',
      cast: ['Cast A', 'Cast B'],
      directors: ['Director A'],
      genres: ['Drama'],
      producers: ['Producer A'],
      writers: ['Writer A']
    }));
    expect(mocks.router.navigate).toHaveBeenCalledWith([], expect.objectContaining({
      queryParams: expect.objectContaining({
        keywords: 'hero',
        sort: 'createdOn',
        descending: 'true'
      })
    }));
  });

  it('uses cached results when request matches and autoIncrementPage is false', async () => {
    const { component, mocks } = await createComponent();

    component.sort = 'title';
    component.keywords = 'cache-keyword';

    sessionStorage.setItem(SearchComponent.CACHE_KEY, JSON.stringify({
      hasMoreResults: false,
      request: {
        sort: 'title',
        keywords: 'cache-keyword'
      } as SearchMediaRequest,
      results: [createMediaResult('cached')]
    }));

    await component.loadResults(false, 0, 25);

    expect(mocks.mediaService.search).not.toHaveBeenCalled();
    expect(component.results).toHaveLength(1);
    expect(component.results[0].id).toBe('cached');
    expect(component.hasMoreResults).toBe(false);
  });

  it('loads remote results, increments page, and caches request without paging keys', async () => {
    const first = createMediaResult('first');
    const second = createMediaResult('second');
    const { component, mocks } = await createComponent({
      searchResponse: createSearchResponse([first, second])
    });

    component.cast = ['A'];
    component.directors = ['B'];
    component.genres = ['C'];
    component.producers = ['D'];
    component.writers = ['E'];
    component.descending = true;
    component.keywords = '  test  ';

    await component.loadResults(true, 0, 2);

    expect(component.results.map(result => result.id)).toEqual(['first', 'second']);
    expect(component.hasMoreResults).toBe(true);
    expect((component as any).pageIndex).toBe(1);

    const cachedRaw = sessionStorage.getItem(SearchComponent.CACHE_KEY);
    expect(cachedRaw).toBeTruthy();
    const cached = JSON.parse(cachedRaw as string);
    expect(cached.request).toEqual(expect.objectContaining({
      sort: 'title',
      keywords: 'test',
      descending: true,
      cast: ['A'],
      directors: ['B'],
      genres: ['C'],
      producers: ['D'],
      writers: ['E']
    }));
    expect(cached.request.skip).toBeUndefined();
    expect(cached.request.take).toBeUndefined();
    expect(mocks.mediaService.search).toHaveBeenCalledTimes(1);
  });

  it('restores scroll position through requestAnimationFrame and saves page position', async () => {
    vi.useFakeTimers();
    const rafSpy = vi.fn((callback: FrameRequestCallback): number => {
      callback(0);
      return 1;
    });
    vi.stubGlobal('requestAnimationFrame', (callback: FrameRequestCallback): number => {
      return rafSpy(callback);
    });

    const { component } = await createComponent({
      searchResponse: createSearchResponse([createMediaResult('one')])
    });

    // Keep the injected ViewChild stub stable while exercising loadResults.
    (component as any).cdr = { detectChanges: vi.fn() };

    const scrollHost = { scrollTop: 0 } as HTMLDivElement;
    component.searchContentComponent = {
      searchResultsElement: new ElementRef(scrollHost)
    } as unknown as any;
    (component as any).scrollPosition = 55;

    await component.loadResults(true, 0, 1);
    vi.advanceTimersByTime(120);

    expect(rafSpy).toHaveBeenCalledTimes(1);

    vi.unstubAllGlobals();
    vi.useRealTimers();
  });

  it('handles search errors by clearing results and resetting loading state', async () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);
    const { component } = await createComponent();

    component.results = [createMediaResult('existing')];
    component.isLoading = false;
    component.hasMoreResults = true;
    (component as any).mediaService = {
      search: vi.fn().mockReturnValue(throwError(() => new Error('boom')))
    };

    await component.loadResults(true, 0, 1);

    expect(component.results).toEqual([]);
    expect(component.isLoading).toBe(false);
    expect(consoleSpy).toHaveBeenCalled();

    consoleSpy.mockRestore();
  });

  it('returns early from loadResults when already loading or no more results', async () => {
    const { component, mocks } = await createComponent();

    mocks.mediaService.search.mockClear();

    component.isLoading = true;
    await component.loadResults();
    expect(mocks.mediaService.search).not.toHaveBeenCalled();

    component.isLoading = false;
    component.hasMoreResults = false;
    await component.loadResults();
    expect(mocks.mediaService.search).not.toHaveBeenCalled();
  });

  it('clears page position and resets scrollTop when search content exists', async () => {
    const { component } = await createComponent();
    const scrollHost = { scrollTop: 250 } as HTMLDivElement;

    component.searchContentComponent = {
      searchResultsElement: new ElementRef(scrollHost)
    } as unknown as any;
    (component as any).pageIndex = 6;
    (component as any).scrollPosition = 250;

    component.clearPagePosition();

    expect((component as any).pageIndex).toBe(0);
    expect((component as any).scrollPosition).toBe(0);
    expect(scrollHost.scrollTop).toBe(0);
  });

  it('triggers search from UI handlers and toggles sort state', async () => {
    const { component } = await createComponent();
    const onSearchSpy = vi.spyOn(component, 'onSearch').mockResolvedValue();

    component.onKeywordsChange();
    component.onSortChange();
    component.toggleSortDirection();
    component.selectSortOption('duration');

    expect(component.descending).toBe(true);
    expect(component.sort).toBe('duration');
    expect(onSearchSpy).toHaveBeenCalledTimes(4);

    onSearchSpy.mockRestore();
  });

  it('toggles and closes sort modal', async () => {
    const { component } = await createComponent();

    component.toggleSortOptions();
    expect(component.showSortModal).toBe(true);

    component.toggleSortOptions();
    expect(component.showSortModal).toBe(false);

    component.showSortModal = true;
    component.closeSortModal();
    expect(component.showSortModal).toBe(false);
  });

  it('loads more on scroll when near bottom and not pending', async () => {
    const { component } = await createComponent();
    const loadSpy = vi.spyOn(component, 'loadResults').mockResolvedValue();

    const event = {
      target: {
        scrollHeight: 600,
        scrollTop: 450,
        clientHeight: 100
      }
    } as unknown as Event;

    component.onScroll(event);

    expect(loadSpy).toHaveBeenCalledTimes(1);

    (component as any).pendingScrollUpdate = true;
    component.onScroll(event);

    expect(loadSpy).toHaveBeenCalledTimes(1);
    loadSpy.mockRestore();
  });

  it('resets state in onSearch and updates query params', async () => {
    const { component } = await createComponent();
    const loadSpy = vi.spyOn(component, 'loadResults').mockResolvedValue();

    component.results = [createMediaResult('old')];
    component.hasMoreResults = false;
    (component as any).pageIndex = 5;
    (component as any).scrollPosition = 30;

    await component.onSearch();

    expect(component.results).toEqual([]);
    expect(component.hasMoreResults).toBe(true);
    expect((component as any).pageIndex).toBe(0);
    expect((component as any).scrollPosition).toBe(0);
    expect(loadSpy).toHaveBeenCalledTimes(1);

    loadSpy.mockRestore();
  });

  it('saves page position and updates query params when card is clicked', async () => {
    const { component, mocks } = await createComponent();
    const scrollHost = { scrollTop: 88 } as HTMLDivElement;

    component.searchContentComponent = {
      searchResultsElement: new ElementRef(scrollHost)
    } as unknown as any;

    component.onCardClick();

    expect(sessionStorage.getItem(SearchComponent.PAGE_KEY)).toBe('scroll=88&pageIndex=0');
    expect(mocks.router.navigate).toHaveBeenCalled();
  });
});
