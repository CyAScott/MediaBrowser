import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MediaReadModel, MediaService, SearchMediaRequest } from '../services/media.service';
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';
import { SearchContentComponent } from './search-content';
import { SearchQueryParams } from './search-query-params';

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
 * - cast: array of cast member names
 * - descending: boolean for sort direction
 * - directors: array of director names
 * - genres: array of genre names
 * - keywords: string search term
 * - producers: array of producer names
 * - sort: sort field ('title' | 'createdOn' | 'duration' | 'userStarRating' | 'random')
 * - writers: array of writer names
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
    { value: 'userStarRating', label: 'Rating' },
    { value: 'random', label: 'Random' }
  ] as const;

  // Search parameters that match SearchMediaRequest
  readonly pageSize: number = 25;
  parameters: SearchQueryParams = new SearchQueryParams();
  
  // UI state
  results: MediaReadModel[] = [];
  hasMoreResults: boolean = true;
  isLoading: boolean = false;
  showSortModal: boolean = false;

  // Page management
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
      this.parameters.pageIndex = parseInt(savedPagePosition.get('pageIndex') || '0', 10);
    }
  }

  private savePagePosition(): void {
    try {
      if (this.searchContentComponent?.searchResultsElement) {
        this.scrollPosition = this.searchContentComponent.searchResultsElement.nativeElement.scrollTop;
        sessionStorage.setItem(SearchComponent.PAGE_KEY, `scroll=${this.scrollPosition}&pageIndex=${this.parameters.pageIndex}`);
      } else {
        this.clearPagePosition();
      }
    } catch (error) {
      console.error('Error saving scroll position:', error);
    }
  }

  private updateQueryParams(): void {
    const queryParams: any = SearchQueryParams.getQueryParams(this.parameters);

    // Update URL without triggering navigation
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      queryParamsHandling: 'replace'
    });
  }

  clearPagePosition(): void {
    this.parameters.pageIndex = 0;
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
    skip: number = this.parameters.pageIndex * this.pageSize,
    take: number = this.pageSize): Promise<void> {
    if (this.isLoading || !this.hasMoreResults) {
      return;
    }

    try {
      this.isLoading = true;
      
      // Build search request matching SearchMediaRequest interface
      const searchRequest: SearchMediaRequest = SearchQueryParams.getSearchMediaRequest(this.parameters, skip, take);

      const cachedDataValue = sessionStorage.getItem(SearchComponent.CACHE_KEY);
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
          this.parameters.pageIndex++;
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
    SearchQueryParams.loadFromQueryParams(this.parameters, this.route.snapshot.queryParams);
    if (this.parameters.pageIndex === 0) {
      this.clearPagePosition();
    }

    // Always perform search based on current parameters
    await this.loadResults(
      this.parameters.pageIndex === 0,
      0,
      Math.max(this.parameters.pageIndex * this.pageSize, this.pageSize)
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

  selectSortOption(sortValue: 'title' | 'createdOn' | 'duration' | 'userStarRating' | 'random'): void {
    this.parameters.sort = sortValue;
    this.showSortModal = false;
    this.onSearch();
  }

  toggleSortDirection(): void {
    this.parameters.descending = !this.parameters.descending;
    this.onSearch();
  }

  toggleSortOptions(): void {
    this.showSortModal = !this.showSortModal;
  }
}