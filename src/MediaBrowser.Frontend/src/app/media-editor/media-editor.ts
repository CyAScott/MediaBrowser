import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Navigation, Router } from '@angular/router';
import { MediaManager } from '../types/MediaManager';
import { Thumbnail } from '../types/MoveToMediaDirRequest';
import { MediaInfo } from '../types/SearchMediaRequest';

declare global {
  interface Window {
    mediaManager: MediaManager;
  }
}

@Component({
  selector: 'app-media-editor',
  imports: [CommonModule, FormsModule],
  templateUrl: './media-editor.html',
  styleUrls: ['./media-editor.css']
})
export class MediaEditorComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private cdr = inject(ChangeDetectorRef);
  private mediaManager: MediaManager = window.mediaManager;
  private navigation: Navigation | null = null;

  constructor(private router: Router) {
    this.navigation = this.router.currentNavigation();
  }
  
  mediaData: MediaInfo | null = null;
  mediaId: string | null = null;
  path: string | null = null;
  thumbnail: Thumbnail | null = null;
  isLoading: boolean = false;
  isSaving: boolean = false;
  isCreatingThumbnail: boolean = false;
  
  // Form fields for editable properties
  editableData: {
    cast: string[];
    description: string;
    directors: string[];
    originalTitle: string;
    path: string;
    producers: string[];
    title: string;
    userStarRating: number;
    writers: string[];
  } = {
    cast: [],
    description: '',
    directors: [],
    originalTitle: '',
    path: '',
    producers: [],
    title: '',
    userStarRating: 0,
    writers: []
  };

  async ngOnInit(): Promise<void> {
    try {
      this.mediaId = this.route.snapshot.paramMap.get('id');
      if (this.mediaId) {
        await this.loadMediaById(this.mediaId);
      } else {
        await this.loadMediaByState();
      }
    } catch (error) {
      console.error('Error during initialization:', error);
    }
  }

  async loadMediaById(id: string): Promise<void> {
    console.log('Loading media by ID:', id);

    this.isLoading = true;

    this.mediaData = await this.mediaManager.getMediaById(id);
    if (!this.mediaData) {
      throw new Error('Media not found');
    }
      
    this.setEditableData(this.mediaData);

    this.isLoading = false;

    this.cdr.detectChanges();
  }

  async loadMediaByState(): Promise<void> {
    console.log('Loading media by state');

    const navigationState = this.navigation?.extras.state;

    if (!navigationState) {
      throw new Error('No navigation state found');
    }

    this.mediaData = navigationState?.['mediaData'];
    this.path = navigationState['path'];

    if (!this.mediaData || !this.path) {
      throw new Error('No media data or path found');
    }

    this.thumbnail = {
      timestamp: Math.min(60, this.mediaData.duration),
      x: 0,
      y: 0,
      width: this.mediaData.width || null,
      height: this.mediaData.height || null
    }
    this.setEditableData(this.mediaData);
  }

  setEditableData(mediaData: MediaInfo): void {
    this.editableData = {
      cast: [...mediaData.cast],
      description: mediaData.description,
      directors: [...mediaData.directors],
      originalTitle: mediaData.originalTitle,
      path: mediaData.path,
      producers: [...mediaData.producers],
      title: mediaData.title,
      userStarRating: mediaData.userStarRating,
      writers: [...mediaData.writers]
    };
  }

  async saveChanges(): Promise<void> {
    if (!this.mediaData) return;
    
    this.isSaving = true;
    try {
      // Create updated media object
      const updatedMedia: MediaInfo = {
        ...this.mediaData,
        ...this.editableData
      };

      if (this.mediaId) {
        await this.mediaManager.update(updatedMedia);
        // Navigate back to player
        this.router.navigate(['/player', this.mediaId]);
      } else if (this.path) {
        await await this.mediaManager.moveToMediaDir({
          filePath: this.path,
          nfo: updatedMedia,
          thumbnail: this.thumbnail!
        });
        // Navigate back to import
        this.router.navigate(['/import']);
      }

    } catch (error) {
      console.error('Error saving media changes:', error);
    } finally {
      this.isSaving = false;
    }
  }

  cancel(): void {
    if (this.mediaId) {
      this.router.navigate(['/player', this.mediaId]);
    } else {
      this.router.navigate(['/import']);
    }
  }

  // Helper methods for formatted display
  formatDuration(seconds: number): string {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const remainingSeconds = (seconds % 60).toFixed(3);
    
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${remainingSeconds.toString().padStart(2, '0')}`;
  }

  formatDateTime(epochMs: string): string {
    const date = new Date(parseInt(epochMs));
    return date.toLocaleString();
  }

  formatFileSize(bytes: number): string {
    const mb = bytes / (1024 * 1024);
    return `${mb.toFixed(2)} MB`;
  }

  // Array manipulation methods
  addArrayItem(arrayName: keyof Pick<typeof this.editableData, 'cast' | 'directors' | 'producers' | 'writers'>): void {
    this.editableData[arrayName].push('');
  }

  removeArrayItem(arrayName: keyof Pick<typeof this.editableData, 'cast' | 'directors' | 'producers' | 'writers'>, index: number): void {
    this.editableData[arrayName].splice(index, 1);
  }

  trackByIndex(index: number): number {
    return index;
  }

  // Star rating methods
  setRating(rating: number): void {
    this.editableData.userStarRating = rating;
  }

  getStarClass(starNumber: number): string {
    return starNumber <= this.editableData.userStarRating ? 'fa-solid fa-star' : 'fa-regular fa-star';
  }

  // Video player and thumbnail methods
  async createThumbnailFromVideo(): Promise<void> {
    const videoElement = document.querySelector('#video-player') as HTMLVideoElement;
    if (!videoElement) {
      console.error('Video player not found');
      return;
    }

    this.isCreatingThumbnail = true;
    try {
      this.thumbnail = {
        timestamp: videoElement.currentTime,
        x: 0,
        y: 0,
        width: this.mediaData?.width || null,
        height: this.mediaData?.height || null
      };

      if (this.mediaId) {
        await window.mediaManager.updateThumbnails(this.mediaId, this.thumbnail, null);
      }
    } catch (error) {
      console.error('Error creating thumbnail:', error);
    } finally {
      this.isCreatingThumbnail = false;
    }
    this.cdr.detectChanges();
  }

  getVideoUrl(): string {
    return this.mediaData?.url || '';
  }
}