import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit, OnDestroy, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MediaService } from '../services';
import { SearchComponent } from '../search/search';
import { firstValueFrom } from 'rxjs';

interface CastMember {
  name: string;
  imageUrl: string;
  searchPath: string;
  searchParams: { [key: string]: string | string[] };
}

@Component({
  selector: 'app-cast',
  imports: [CommonModule, RouterModule],
  templateUrl: './cast.html',
  styleUrls: ['./cast.css']
})
export class CastComponent implements OnInit, AfterViewInit, OnDestroy {
  private cdr = inject(ChangeDetectorRef);
  private mediaService = inject(MediaService);

  @ViewChild('castGrid', { static: false }) castGrid!: ElementRef<HTMLDivElement>;

  castMembers: CastMember[] = [];
  isLoading: boolean = false;
  private scrollPosition: number = 0;
  private readonly SCROLL_KEY = 'cast-scroll-position';
  private scrollListener?: (event: Event) => void;

  constructor() { 
    // Load saved scroll position from session storage
    const savedScrollPosition = sessionStorage.getItem(this.SCROLL_KEY);
    if (savedScrollPosition) {
      this.scrollPosition = parseInt(savedScrollPosition, 10);
    }
  }

  async ngOnInit(): Promise<void> {
    await this.loadCastMembers();
  }

  ngAfterViewInit(): void {
    // Create bound scroll listener for proper cleanup
    this.scrollListener = this.onScroll.bind(this);
    
    // Add scroll event listener to save scroll position
    if (this.castGrid) {
      this.castGrid.nativeElement.addEventListener('scroll', this.scrollListener);
      
      // Restore scroll position if data is already loaded
      if (this.scrollPosition > 0 && this.castMembers.length > 0) {
        requestAnimationFrame(() => {
          this.castGrid.nativeElement.scrollTop = this.scrollPosition;
        });
      }
    }
  }

  ngOnDestroy(): void {
    // Remove scroll event listener
    if (this.castGrid && this.scrollListener) {
      this.castGrid.nativeElement.removeEventListener('scroll', this.scrollListener);
    }
  }

  private onScroll(event: Event): void {
    const element = event.target as HTMLDivElement;
    this.scrollPosition = element.scrollTop;
    // Save scroll position to sessionStorage
    sessionStorage.setItem(this.SCROLL_KEY, this.scrollPosition.toString());
  }

  clearScrollPosition(): void {
    this.scrollPosition = 0;
    sessionStorage.removeItem(this.SCROLL_KEY);
    if (this.castGrid) {
      this.castGrid.nativeElement.scrollTop = 0;
    }
  }

  async loadCastMembers(): Promise<void> {
    this.isLoading = true;
    
    try {
      this.castMembers = (await firstValueFrom(this.mediaService.getAllCast())).map(name => ({
        name,
        imageUrl: `/api/media/cast/${encodeURIComponent(name)}/thumbnail`,
        searchPath: '/search',
        searchParams: SearchComponent.createSearchQueryParams({ cast: [name] })
      }));
      
      // Restore scroll position after content is loaded and rendered
      if (this.castGrid && this.scrollPosition > 0) {
        // Use requestAnimationFrame to ensure DOM is fully updated
        requestAnimationFrame(() => {
          this.castGrid.nativeElement.scrollTop = this.scrollPosition;
        });
      }
    } catch (error) {
      console.error('Load error:', error);
    } finally {
      this.isLoading = false;
      this.cdr.detectChanges();
    }
  }
}