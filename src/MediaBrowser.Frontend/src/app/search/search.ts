import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit, ViewChild, ElementRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MediaReadModel, MediaService, SearchMediaRequest } from '../services/media.service';
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';
import { SearchContentComponent } from './search-content';

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
 * - pageIndex: current page index (0-based)
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
  
  // Search parameters that match SearchMediaRequest
  readonly pageSize: number = 25;
  cast: string[] = [];
  descending: boolean = false;
  genres: string[] = [];
  keywords: string = '';
  sort: 'title' | 'createdOn' | 'duration' | 'userStarRating' = 'title';
  
  readonly sortOptions = [
    { value: 'title', label: 'Title' },
    { value: 'createdOn', label: 'Created On' },
    { value: 'duration', label: 'Duration' },
    { value: 'userStarRating', label: 'Rating' }
  ] as const;
  
  // UI state
  results: MediaReadModel[] = [];
  hasMoreResults: boolean = true;
  isLoading: boolean = false;
  pageIndex: number = 0;
  showSortModal: boolean = false;

  // Scroll management
  private pendingScrollUpdate: boolean = false;
  private scrollPosition: number = 0;
  private readonly SCROLL_KEY = 'search-scroll-position';  

  /**
   * Helper method to create search query parameters for navigation
   * @param params Search parameters that match SearchMediaRequest interface
   * @returns Query parameters object for router navigation
   */
  static createSearchQueryParams(params: Partial<SearchMediaRequest>, pageIndex: number): { [key: string]: string | string[] } {
    const queryParams: { [key: string]: string | string[] } = {};
    
    if (params.keywords?.trim()) {
      queryParams['keywords'] = params.keywords.trim();
    }
    if (params.cast && params.cast.length > 0) {
      queryParams['cast'] = params.cast;
    }
    if (params.genres && params.genres.length > 0) {
      queryParams['genres'] = params.genres;
    }
    if (params.sort) {
      queryParams['sort'] = params.sort;
    }
    if (params.descending) {
      queryParams['descending'] = 'true';
    }
    queryParams['pageIndex'] = pageIndex.toString();
    
    return queryParams;
  }

  constructor(private router: Router, private mediaService: MediaService) {
    // Load saved scroll position from session storage
    const savedScrollPosition = sessionStorage.getItem(this.SCROLL_KEY);
    if (savedScrollPosition) {
      this.scrollPosition = parseInt(savedScrollPosition, 10);
    }
  }

  onCardClick(): void {
    this.saveScrollPosition();
    this.updateQueryParams();
  }

  clearScrollPosition(): void {
    this.scrollPosition = 0;
    sessionStorage.removeItem(this.SCROLL_KEY);
    if (this.searchContentComponent?.searchResultsElement) {
      this.searchContentComponent.searchResultsElement.nativeElement.scrollTop = 0;
    }
  }



  async ngOnInit(): Promise<void> {
    // Load initial state from query parameters
    this.loadFromQueryParams();
    if (this.pageIndex === 0) {
      this.clearScrollPosition();
    }

    // Always perform search based on current parameters
    await this.loadResults(
      this.pageIndex === 0,
      0,
      Math.max(this.pageIndex * this.pageSize, this.pageSize)
    );
  }

  async onSearch(): Promise<void> {
    // Reset pagination for new search
    this.clearScrollPosition();
    this.hasMoreResults = true;
    this.pageIndex = 0;
    this.results = [];
    
    // Update URL with current search parameters
    this.updateQueryParams();
    
    await this.loadResults();
  }

  toggleSortDirection(): void {
    this.descending = !this.descending;
    this.onSearch();
  }

  toggleSortOptions(): void {
    this.showSortModal = !this.showSortModal;
  }

  selectSortOption(sortValue: 'title' | 'createdOn' | 'duration' | 'userStarRating'): void {
    this.sort = sortValue;
    this.showSortModal = false;
    this.onSearch();
  }

  closeSortModal(): void {
    this.showSortModal = false;
  }

  onSortChange(): void {
    this.onSearch();
  }

  onKeywordsChange(): void {
    this.onSearch();
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
      if (this.genres.length > 0) {
        searchRequest.genres = [...this.genres];
      }
      if (this.descending) {
        searchRequest.descending = this.descending;
      }

      const response = await firstValueFrom(this.mediaService.search(searchRequest));

      this.results = [...this.results, ...response.results];

      // Check if we have more results
      this.hasMoreResults = response.results.length === take;
      
      // Increment for next load
      if (autoIncrementPage && response.results.length > 0) {
        this.pageIndex++;
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

  // Keep the old method for compatibility but make it call the new one
  onScroll(event: Event): void {
    const element = event.target as HTMLElement;
    const threshold = 100; // Load more when 100px from bottom

    if (!this.isLoading && !this.pendingScrollUpdate && element.scrollHeight - element.scrollTop - element.clientHeight < threshold) {
      this.saveScrollPosition();
      this.loadResults();
    }
  }

  private loadFromQueryParams(): void {
    const params = this.route.snapshot.queryParams;
    
    // Load search parameters from URL
    this.keywords = params['keywords'] || '';
    this.sort = params['sort'] || 'title';
    this.descending = params['descending'] === 'true';
    this.pageIndex = parseInt(params['pageIndex']) || 0;
    
    // Handle array parameters
    if (params['cast']) {
      this.cast = Array.isArray(params['cast']) ? params['cast'] : [params['cast']];
    } else {
      this.cast = [];
    }
    
    if (params['genres']) {
      this.genres = Array.isArray(params['genres']) ? params['genres'] : [params['genres']];
    } else {
      this.genres = [];
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
    if (this.genres.length > 0) {
      queryParams['genres'] = this.genres;
    }
    queryParams['sort'] = this.sort;
    if (this.descending) {
      queryParams['descending'] = 'true';
    }
    if (this.pageIndex) {
      queryParams['pageIndex'] = this.pageIndex.toString();
    }

    // Update URL without triggering navigation
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      queryParamsHandling: 'replace'
    });
  }

  private saveScrollPosition(): void {
    try {
      if (this.searchContentComponent?.searchResultsElement) {
        this.scrollPosition = this.searchContentComponent.searchResultsElement.nativeElement.scrollTop;
        sessionStorage.setItem(this.SCROLL_KEY, this.scrollPosition.toString());
      } else {
        this.clearScrollPosition();
      }
    } catch (error) {
      console.error('Error saving scroll position:', error);
    }
  }
}