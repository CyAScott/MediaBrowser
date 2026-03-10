import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { ActivatedRoute, Navigation, Router } from '@angular/router';
import { Location } from '@angular/common';
import { MediaReadModel, MediaService } from '../services';
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';

@Component({
  selector: 'app-player',
  imports: [CommonModule],
  templateUrl: './player.html',
  styleUrls: ['./player.css']
})
export class PlayerComponent implements OnInit, OnDestroy, AfterViewInit {
  private cdr = inject(ChangeDetectorRef);
  private location = inject(Location);
  private mediaService = inject(MediaService);
  private navigation: Navigation | null = null;
  private route = inject(ActivatedRoute);

  constructor(private router: Router) {
    this.navigation = this.router.currentNavigation();
  }
  
  @ViewChild('videoElement', { static: false }) videoElement!: ElementRef<HTMLVideoElement>;
  @ViewChild('audioElement', { static: false }) audioElement!: ElementRef<HTMLAudioElement>;
  
  mediaData: MediaReadModel | null = null;
  mediaId: string | null = null;
  headerVisible: boolean = true;
  hasHistory: boolean = false;

  private hideTimeout: number | null = null;
  private readonly VOLUME_STORAGE_KEY = 'mediaBrowser_volume';

  private clearHideTimer(): void {
    if (this.hideTimeout) {
      window.clearTimeout(this.hideTimeout);
      this.hideTimeout = null;
    }
  }

  private getSavedVolume(): number {
    const savedVolume = localStorage.getItem(this.VOLUME_STORAGE_KEY);
    return savedVolume ? parseFloat(savedVolume) : 1.0;
  }

  private saveVolume(volume: number): void {
    localStorage.setItem(this.VOLUME_STORAGE_KEY, volume.toString());
  }

  private startHideTimer(): void {
    this.clearHideTimer();
    this.hideTimeout = window.setTimeout(() => {
      this.headerVisible = false;
      this.cdr.detectChanges();
    }, 5000); // Hide after 5 seconds
  }

  applySavedVolume(): void {
    const savedVolume = this.getSavedVolume();
    
    // Apply to video element if it exists
    if (this.videoElement?.nativeElement) {
      this.videoElement.nativeElement.volume = savedVolume;
    }
    
    // Apply to audio element if it exists
    if (this.audioElement?.nativeElement) {
      this.audioElement.nativeElement.volume = savedVolume;
    }
  }

  async loadMediaById(id: string): Promise<void> {
    try { 
      console.log('Loading media by ID:', id);
      console.log('Navigation state:', this.navigation);
      const response = this.navigation?.extras.state?.['mediaData'] ?? await firstValueFrom(this.mediaService.get(id));
      this.mediaData = response;
      this.cdr.detectChanges();
    } catch (error) {
      console.error('Error loading media by ID:', error);
    }
  }

  goBack(): void {
    this.location.back();
  }

  editMedia(): void {
    if (this.mediaId) {
      this.router.navigate(['/edit', this.mediaId],
        { state: { mediaData: this.mediaData } }
      );
    }
  }

  async ngOnInit(): Promise<void> {
    const mediaId = this.route.snapshot.paramMap.get('id');
    if (mediaId) {
      this.mediaId = mediaId;
      this.loadMediaById(mediaId);
    }
    
    // Check if there's history available
    this.hasHistory = window.history.length > 1;
    
    this.startHideTimer();
  }

  ngAfterViewInit(): void {
    this.applySavedVolume();
  }

  ngOnDestroy(): void {
    this.clearHideTimer();
  }

  onMouseLeave(): void {
    this.startHideTimer();
  }

  onMouseMove(): void {
    if (!this.headerVisible) {
      this.headerVisible = true;
      this.cdr.detectChanges();
    }
    this.startHideTimer();
  }

  onVolumeChange(event: Event): void {
    const element = event.target as HTMLVideoElement | HTMLAudioElement;
    this.saveVolume(element.volume);
  }
}