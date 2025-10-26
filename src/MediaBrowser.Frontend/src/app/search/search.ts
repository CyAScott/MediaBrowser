import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit, ViewChild, ElementRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MediaReadModel, MediaService, SearchMediaRequest } from '../services/media.service';
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';
import { SearchContentComponent } from './search-content';

interface CacheData {
  hasMoreResults: boolean;
  request: SearchMediaRequest;
  results: MediaReadModel[];
}

/**
 * SearchComponent that uses URL query parameters for search state management.
 * This allows for bookmarkable search results and proper browser navigation.
 * 
 * Query parameters correspond directly to SearchMediaRequest interface:
 * - keywords: string search term
 * - cast: array of cast member names
 * - genres: array of genre names  
 * - sort: sort field ('title' | 'createdOn' | 'duration' | 'userStarRating')
 * - descending: boolean for sort direction
 * 
 * Example URLs:
 * /search?keywords=action&sort=createdOn&descending=true
 * /search?cast=John%20Doe&cast=Jane%20Smith&genres=Action
 */
@Component({
  selector: 'app-search',
  imports: [CommonModule, FormsModule, RouterModule, SearchContentComponent],
  templateUrl: './search.html',
  styleUrls: ['./search.css']
})
export class SearchComponent implements OnInit {
  private cdr = inject(ChangeDetectorRef);
  private route = inject(ActivatedRoute);

  @ViewChild('searchContent', { static: false }) searchContentComponent!: SearchContentComponent;
  
  readonly sortOptions = [
    { value: 'title', label: 'Title' },
    { value: 'createdOn', label: 'Created On' },
    { value: 'duration', label: 'Duration' },
    { value: 'userStarRating', label: 'Rating' }
  ] as const;
  static readonly DEFAULT_SORT = 'title';
  
  // Search parameters that match SearchMediaRequest
  readonly pageSize: number = 25;
  cast: string[] = [];
  descending: boolean = false;
  directors: string[] = [];
  genres: string[] = [];
  keywords: string = '';
  producers: string[] = [];
  sort: 'title' | 'createdOn' | 'duration' | 'userStarRating' = SearchComponent.DEFAULT_SORT;
  writers: string[] = [];
  
  // UI state
  results: MediaReadModel[] = [];
  hasMoreResults: boolean = true;
  isLoading: boolean = false;
  showSortModal: boolean = false;

  // Page management
  private pageIndex: number = 0;
  private pendingScrollUpdate: boolean = false;
  private scrollPosition: number = 0;
  static readonly CACHE_KEY = 'cached-results';
  static readonly PAGE_KEY = 'search-page-position';

  static clearCachedResults(): void {
    sessionStorage.removeItem(SearchComponent.CACHE_KEY);
  }

  static clearPagePositionState(): void {
    sessionStorage.removeItem(SearchComponent.PAGE_KEY);
  }

  constructor(private router: Router, private mediaService: MediaService) {
    // Load saved page position from session storage
    const savedPagePosition = new URLSearchParams(sessionStorage.getItem(SearchComponent.PAGE_KEY) || '');
    if (savedPagePosition) {
      this.scrollPosition = parseInt(savedPagePosition.get('scroll') || '0', 10);
      this.pageIndex = parseInt(savedPagePosition.get('pageIndex') || '0', 10);
    }
  }

  private loadFromQueryParams(): void {
    const params = this.route.snapshot.queryParams;
    
    // Load search parameters from URL
    this.keywords = params['keywords'] || '';
    this.sort = params['sort'] || SearchComponent.DEFAULT_SORT;
    this.descending = params['descending'] === 'true';
    
    // Handle array parameters
    if (params['cast']) {
      this.cast = Array.isArray(params['cast']) ? params['cast'] : [params['cast']];
    } else {
      this.cast = [];
    }
    
    if (params['directors']) {
      this.directors = Array.isArray(params['directors']) ? params['directors'] : [params['directors']];
    } else {
      this.directors = [];
    }
    
    if (params['genres']) {
      this.genres = Array.isArray(params['genres']) ? params['genres'] : [params['genres']];
    } else {
      this.genres = [];
    }

    if (params['producers']) {
      this.producers = Array.isArray(params['producers']) ? params['producers'] : [params['producers']];
    } else {
      this.producers = [];
    }

    if (params['writers']) {
      this.writers = Array.isArray(params['writers']) ? params['writers'] : [params['writers']];
    } else {
      this.writers = [];
    }
  }

  private savePagePosition(): void {
    try {
      if (this.searchContentComponent?.searchResultsElement) {
        this.scrollPosition = this.searchContentComponent.searchResultsElement.nativeElement.scrollTop;
        sessionStorage.setItem(SearchComponent.PAGE_KEY, `scroll=${this.scrollPosition}&pageIndex=${this.pageIndex}`);
      } else {
        this.clearPagePosition();
      }
    } catch (error) {
      console.error('Error saving scroll position:', error);
    }
  }

  private updateQueryParams(): void {
    const queryParams: any = {};
    
    // Only add parameters that have values to keep URL clean
    if (this.keywords?.trim()) {
      queryParams['keywords'] = this.keywords.trim();
    }
    if (this.cast.length > 0) {
      queryParams['cast'] = this.cast;
    }
    if (this.directors.length > 0) {
      queryParams['directors'] = this.directors;
    }
    if (this.genres.length > 0) {
      queryParams['genres'] = this.genres;
    }
    if (this.producers.length > 0) {
      queryParams['producers'] = this.producers;
    }
    if (this.writers.length > 0) {
      queryParams['writers'] = this.writers;
    }
    queryParams['sort'] = this.sort;
    if (this.descending) {
      queryParams['descending'] = 'true';
    }

    // Update URL without triggering navigation
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      queryParamsHandling: 'replace'
    });
  }

  clearPagePosition(): void {
    this.pageIndex = 0;
    this.scrollPosition = 0;
    SearchComponent.clearPagePositionState();
    if (this.searchContentComponent?.searchResultsElement) {
      this.searchContentComponent.searchResultsElement.nativeElement.scrollTop = 0;
    }
  }

  closeSortModal(): void {
    this.showSortModal = false;
  }

  async loadResults(
    autoIncrementPage: boolean = true,
    skip: number = this.pageIndex * this.pageSize,
    take: number = this.pageSize): Promise<void> {
    if (this.isLoading || !this.hasMoreResults) {
      return;
    }

    try {
      this.isLoading = true;
      
      // Build search request matching SearchMediaRequest interface
      const searchRequest: SearchMediaRequest = {
        skip: skip,
        sort: this.sort,
        take: take,
      };

      // Add optional parameters only if they have values
      if (this.keywords?.trim()) {
        searchRequest.keywords = this.keywords.trim();
      }
      if (this.cast.length > 0) {
        searchRequest.cast = [...this.cast];
      }
      if (this.directors.length > 0) {
        searchRequest.directors = [...this.directors];
      }
      if (this.genres.length > 0) {
        searchRequest.genres = [...this.genres];
      }
      if (this.descending) {
        searchRequest.descending = this.descending;
      }
      if (this.producers.length > 0) {
        searchRequest.producers = [...this.producers];
      }
      if (this.writers.length > 0) {
        searchRequest.writers = [...this.writers];
      }

      const cachedDataValue = sessionStorage.getItem(SearchComponent.CACHE_KEY);
      console.log('Cached data value:', cachedDataValue);
      const cachedData: CacheData | null = cachedDataValue && !autoIncrementPage ? JSON.parse(cachedDataValue) : null;

      const searchRequestWithoutPageSettings = { ...searchRequest };
      delete searchRequestWithoutPageSettings.skip;
      delete searchRequestWithoutPageSettings.take;

      if (cachedData && JSON.stringify(cachedData.request) === JSON.stringify(searchRequestWithoutPageSettings)) {
        this.hasMoreResults = cachedData.hasMoreResults;
        this.results = cachedData.results;
      } else {
        const response = await firstValueFrom(this.mediaService.search(searchRequest));

        this.results = [...this.results, ...response.results];

        // Check if we have more results
        this.hasMoreResults = response.results.length === take;
        
        // Increment for next load
        if (autoIncrementPage && response.results.length > 0) {
          this.pageIndex++;
        }

        delete searchRequest.skip;
        delete searchRequest.take;

        sessionStorage.setItem(SearchComponent.CACHE_KEY, JSON.stringify({
          hasMoreResults: this.hasMoreResults,
          request: searchRequest,
          results: this.results,
        }));
      }

      this.updateQueryParams();

      this.cdr.detectChanges();
      
      // Restore scroll position after content is loaded and rendered (only once per component lifecycle)
      if (this.searchContentComponent?.searchResultsElement && this.scrollPosition > 0) {
        this.pendingScrollUpdate = true;
        // Use requestAnimationFrame to ensure DOM is fully updated
        requestAnimationFrame(() => {
          this.searchContentComponent.searchResultsElement.nativeElement.scrollTop = this.scrollPosition;
          setTimeout(() => {
            this.pendingScrollUpdate = false;
            this.savePagePosition();
          }, 100);
        });
      }
    } catch (error) {
      console.error('Search error:', error);
      this.results = [];
    } finally {
      this.isLoading = false;
      this.cdr.detectChanges();
    }
  }

  async ngOnInit(): Promise<void> {
    // Load initial state from query parameters
    this.loadFromQueryParams();
    if (this.pageIndex === 0) {
      this.clearPagePosition();
    }

    // Always perform search based on current parameters
    await this.loadResults(
      this.pageIndex === 0,
      0,
      Math.max(this.pageIndex * this.pageSize, this.pageSize)
    );
  }

  onCardClick(): void {
    this.savePagePosition();
    this.updateQueryParams();
  }

  onKeywordsChange(): void {
    this.onSearch();
  }

  onScroll(event: Event): void {
    const element = event.target as HTMLElement;
    const threshold = 100; // Load more when 100px from bottom

    if (!this.isLoading && !this.pendingScrollUpdate && element.scrollHeight - element.scrollTop - element.clientHeight < threshold) {
      this.savePagePosition();
      this.loadResults();
    }
  }

  async onSearch(): Promise<void> {
    // Reset pagination for new search
    this.clearPagePosition();
    this.hasMoreResults = true;
    this.results = [];
    
    // Update URL with current search parameters
    this.updateQueryParams();
    
    await this.loadResults();
  }

  onSortChange(): void {
    this.onSearch();
  }

  selectSortOption(sortValue: 'title' | 'createdOn' | 'duration' | 'userStarRating'): void {
    this.sort = sortValue;
    this.showSortModal = false;
    this.onSearch();
  }

  toggleSortDirection(): void {
    this.descending = !this.descending;
    this.onSearch();
  }

  toggleSortOptions(): void {
    this.showSortModal = !this.showSortModal;
  }
}