import { CommonModule } from '@angular/common';
import { CastMember } from '../types/CastMember';
import { ChangeDetectorRef, Component, inject, OnInit, OnDestroy, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { MediaManager } from '../types/MediaManager';
import { Router } from '@angular/router';

declare global {
  interface Window {
    mediaManager: MediaManager;
  }
}

@Component({
  selector: 'app-cast',
  imports: [CommonModule],
  templateUrl: './cast.html',
  styleUrls: ['./cast.css']
})
export class CastComponent implements OnInit, AfterViewInit, OnDestroy {
  private cdr = inject(ChangeDetectorRef);
  private router = inject(Router);

  @ViewChild('castGrid', { static: false }) castGrid!: ElementRef<HTMLDivElement>;

  castMembers: CastMember[] = [];
  isLoading: boolean = false;
  private scrollPosition: number = 0;
  private readonly SCROLL_KEY = 'cast-scroll-position';

  constructor() { 
    // Load saved scroll position
    const savedScrollPosition = localStorage.getItem(this.SCROLL_KEY);
    if (savedScrollPosition) {
      this.scrollPosition = parseInt(savedScrollPosition, 10);
    }
  }

  async ngOnInit(): Promise<void> {
    await this.loadCastMembers();
  }

  ngAfterViewInit(): void {
    // Add scroll event listener to save scroll position
    if (this.castGrid) {
      this.castGrid.nativeElement.addEventListener('scroll', this.onScroll.bind(this));
    }
  }

  ngOnDestroy(): void {
    // Remove scroll event listener
    if (this.castGrid) {
      this.castGrid.nativeElement.removeEventListener('scroll', this.onScroll.bind(this));
    }
  }

  private onScroll(event: Event): void {
    const element = event.target as HTMLDivElement;
    this.scrollPosition = element.scrollTop;
    // Save scroll position to localStorage
    localStorage.setItem(this.SCROLL_KEY, this.scrollPosition.toString());
  }

  async loadCastMembers(): Promise<void> {
    this.isLoading = true;
    
    try {
      this.castMembers = await window.mediaManager.getCastMembers();
      if (this.castGrid && this.scrollPosition > 0) {
        setTimeout(() => {
          this.castGrid.nativeElement.scrollTop = this.scrollPosition;
        }, 0);
      }
    } catch (error) {
      console.error('Load error:', error);
    } finally {
      this.isLoading = false;
      this.cdr.detectChanges();
    }
  }

  onCastMemberClick(castMember: CastMember): void {
    const state = { cast: castMember.name };

    this.router.navigate(['/search'], { state });
  }
}