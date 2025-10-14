import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MediaManager } from '../types/MediaManager';
import { MediaInfo } from '../types/SearchMediaRequest';

declare global {
  interface Window {
    mediaManager: MediaManager;
  }
}

@Component({
  selector: 'app-player',
  imports: [CommonModule],
  templateUrl: './player.html',
  styleUrls: ['./player.css']
})
export class PlayerComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);
  private mediaManager: MediaManager = window.mediaManager;
  
  mediaData: MediaInfo | null = null;
  mediaId: string | null = null;
  headerVisible: boolean = true;
  private hideTimeout: number | null = null;

  async ngOnInit(): Promise<void> {
    const mediaId = this.route.snapshot.paramMap.get('id');
    if (mediaId) {
      this.mediaId = mediaId;
      this.loadMediaById(mediaId);
    }
    this.startHideTimer();
  }

  ngOnDestroy(): void {
    this.clearHideTimer();
  }

  async loadMediaById(id: string): Promise<void> {
    try { 
      const response = await this.mediaManager.getMediaById(id);
      this.mediaData = response;
      this.cdr.detectChanges();
    } catch (error) {
      console.error('Error loading media by ID:', error);
    }
  }

  goBack(): void {
    this.router.navigate(['/search']);
  }

  editMedia(): void {
    if (this.mediaId) {
      this.router.navigate(['/edit', this.mediaId]);
    }
  }

  private startHideTimer(): void {
    this.clearHideTimer();
    this.hideTimeout = window.setTimeout(() => {
      this.headerVisible = false;
      this.cdr.detectChanges();
    }, 5000); // Hide after 5 seconds
  }

  private clearHideTimer(): void {
    if (this.hideTimeout) {
      window.clearTimeout(this.hideTimeout);
      this.hideTimeout = null;
    }
  }

  onMouseMove(): void {
    if (!this.headerVisible) {
      this.headerVisible = true;
      this.cdr.detectChanges();
    }
    this.startHideTimer(); // Reset the timer
  }

  onMouseLeave(): void {
    this.startHideTimer(); // Start hiding when mouse leaves
  }
}