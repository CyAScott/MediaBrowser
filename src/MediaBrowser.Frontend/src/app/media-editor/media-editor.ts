import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Navigation, Router } from '@angular/router';
import { MediaReadModel, MediaService } from '../services';
import { firstValueFrom } from 'rxjs';
import { ImportService } from '../services/import.service';

@Component({
  selector: 'app-media-editor',
  imports: [CommonModule, FormsModule],
  templateUrl: './media-editor.html',
  styleUrls: ['./media-editor.css']
})
export class MediaEditorComponent implements OnInit {
  private cdr = inject(ChangeDetectorRef);
  private importService = inject(ImportService);
  private mediaService = inject(MediaService);
  private navigation: Navigation | null = null;
  private route = inject(ActivatedRoute);

  constructor(private router: Router) {
    this.navigation = this.router.currentNavigation();
  }
  
  filename: string | null = null;
  isCreatingThumbnail: boolean = false;
  isLoading: boolean = false;
  isSaving: boolean = false;
  mediaData: MediaReadModel | null = null;
  mediaId: string | null = null;
  thumbnail: number | null = null;

  // Form fields for editable properties
  editableData: {
    cast: string[];
    description: string;
    directors: string[];
    originalTitle: string;
    path: string;
    producers: string[];
    title: string;
    userStarRating?: number;
    writers: string[];
  } = {
    cast: [],
    description: '',
    directors: [],
    originalTitle: '',
    path: '',
    producers: [],
    title: '',
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

    this.filename = null;
    this.mediaData = await firstValueFrom(this.mediaService.get(id));
    this.setEditableData(this.mediaData);
    this.thumbnail = 0;

    this.isLoading = false;

    this.cdr.detectChanges();
  }

  async loadMediaByState(): Promise<void> {
    console.log('Loading media by state');

    const navigationState = this.navigation?.extras.state;

    if (!navigationState) {
      throw new Error('No navigation state found');
    }

    this.filename = navigationState['filename'];
    this.mediaData = navigationState?.['mediaData'];
    this.thumbnail = 0;

    if (!this.mediaData || !this.filename) {
      throw new Error('No media data or filename found');
    }

    this.setEditableData(this.mediaData);
  }

  setEditableData(mediaData: MediaReadModel): void {
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
    if (!this.mediaData) {
      return;
    }

    this.isSaving = true;
    try {
      // Create updated media object
      const updatedMedia: MediaReadModel = {
        ...this.mediaData,
        ...this.editableData
      };

      if (this.mediaId) {
        await this.mediaManager.update(updatedMedia);
        // Navigate back to player
        this.router.navigate(['/player', this.mediaId]);
      } else if (this.filename) {
        await firstValueFrom(this.importService.import(this.filename, updatedMedia));
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
    return starNumber <= (this.editableData.userStarRating ?? 0) ? 'fa-solid fa-star' : 'fa-regular fa-star';
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
      this.thumbnail = videoElement.currentTime;

      if (this.mediaId) {
        await firstValueFrom(this.mediaService.updateThumbnail(this.mediaId, { at: this.thumbnail }));
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