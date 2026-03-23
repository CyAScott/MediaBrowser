import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, ElementRef, EventEmitter, inject, Input, OnInit, Output, ViewChild } from '@angular/core';

export interface ThumbnailData {
  thumbnail: number | null;
  thumbnailPreviewUrl: string;
  selectedImageFile: File | null;
}

export interface MediaThumbnailData {
  mime: string;
  start?: number;
  url?: string;
}

@Component({
  selector: 'app-thumbnail-section',
  imports: [CommonModule],
  templateUrl: './thumbnail-section.html',
  styleUrls: ['../media-editor.css', './thumbnail-section.css']
})
export class ThumbnailSectionComponent implements OnInit {
  @Input() initialThumbnail: ThumbnailData | undefined = undefined;
  @Input() isCreatingThumbnail: boolean = false;
  @Input() mediaData!: MediaThumbnailData;
  @Input() showSaveThumbnail: boolean = false;
  @Input() showSetThumbnail: boolean = false;

  @Output() saveThumbnailEvent = new EventEmitter<void>();
  @Output() setPreview = new EventEmitter<void>();
  @Output() thumbnailChange = new EventEmitter<ThumbnailData>();

  @ViewChild('fileInput') fileInput?: ElementRef<HTMLInputElement>;
  @ViewChild('videoPlayer') videoPlayer?: ElementRef<HTMLVideoElement>;

  private cdr = inject(ChangeDetectorRef);

  isDragOver: boolean = false;
  isLoading: boolean = true;
  selectedThumbnail: ThumbnailData | null = null;
  selectedThumbnailIndex: number = -1;
  thumbnails: ThumbnailData[] = [];

  generateThumbnailPreview(videoElement: HTMLVideoElement): string {
    try {
      // Create a canvas element to capture the video frame
      const canvas = document.createElement('canvas');
      const ctx = canvas.getContext('2d');
      
      if (!ctx) {
        console.error('Could not get canvas context');
        return '';
      }

      // Set canvas dimensions to match video
      canvas.width = videoElement.videoWidth;
      canvas.height = videoElement.videoHeight;

      // Draw the current video frame to the canvas
      ctx.drawImage(videoElement, 0, 0, canvas.width, canvas.height);

      // Convert canvas to base64 data URL
      return canvas.toDataURL('image/jpeg', 0.8);
    } catch (error) {
      console.error('Error generating thumbnail preview:', error);
      return '';
    }
  }

  getVideoUrl(): string {
    return this.mediaData?.url ? `${this.mediaData.url}#t=${this.mediaData.start ?? 0}` : '';
  }

  handleImageFile(file: File): void {
    if (!file.type.startsWith('image/')) {
      console.error('Please select an image file');
      return;
    }

    const maxSize = 10 * 1024 * 1024; // 10MB
    if (file.size > maxSize) {
      console.error('File size must be less than 10MB');
      return;
    }

    // Create preview URL
    const reader = new FileReader();
    reader.onload = (e) => {
      this.selectedThumbnailIndex = this.thumbnails.length;
      this.selectedThumbnail = {
        selectedImageFile: file,
        thumbnail: null,
        thumbnailPreviewUrl: e.target?.result as string,
      };
      this.thumbnails.push(this.selectedThumbnail);
      this.onThumbnailChange();
      this.cdr.detectChanges();
    };
    reader.readAsDataURL(file);
  }

  nextThumbnail(): void {
    this.selectedThumbnailIndex++;
    this.selectedThumbnail = this.thumbnails[this.selectedThumbnailIndex];
    this.onThumbnailChange();
    this.videoPlayer?.nativeElement.focus();
  }

  ngOnInit(): void {
    this.isLoading = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      const file = files[0];
      this.handleImageFile(file);
    }
  }

  onThumbnailChange(): void {
    if (this.selectedThumbnail) {
      this.thumbnailChange.emit(this.selectedThumbnail);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.handleImageFile(file);
    }
  }

  onVideoMetadataLoaded(): void {
    if (!this.isLoading) {
      return;
    }

    this.isLoading = false;

    let thumbnail = this.initialThumbnail;

    if (thumbnail) {
      this.thumbnails = [thumbnail];
      this.selectedThumbnail = thumbnail;
      this.selectedThumbnailIndex = 0;
      this.onThumbnailChange();
      this.cdr.detectChanges();
    }

    if (this.videoPlayer) {
      this.videoPlayer.nativeElement.focus();
      if (thumbnail?.thumbnailPreviewUrl === '' && typeof thumbnail?.thumbnail === 'number') {
          thumbnail.thumbnailPreviewUrl = this.generateThumbnailPreview(this.videoPlayer!.nativeElement);
          this.cdr.detectChanges();
      }
    }
  }

  openFileBrowser(): void {
    this.fileInput?.nativeElement.click();
  }

  previousThumbnail(): void {
    this.selectedThumbnailIndex--;
    this.selectedThumbnail = this.thumbnails[this.selectedThumbnailIndex];
    this.onThumbnailChange();
    this.videoPlayer?.nativeElement.focus();
  }

  saveThumbnail(): void {
    this.onThumbnailChange();
    this.saveThumbnailEvent.emit();
    this.videoPlayer?.nativeElement.focus();
  }

  setThumbnailPreview(): void {
    if (this.isCreatingThumbnail || !this.mediaData) {
      return;
    }

    if (this.videoPlayer) {
      const currentTimestamp = this.videoPlayer.nativeElement.currentTime; 
      const existingThumbnailIndex = this.thumbnails.findIndex((thumbnail) => thumbnail.thumbnail === currentTimestamp);

      if (existingThumbnailIndex !== -1) {
        this.selectedThumbnailIndex = existingThumbnailIndex;
        this.selectedThumbnail = this.thumbnails[existingThumbnailIndex];
        this.onThumbnailChange();
        this.videoPlayer.nativeElement.focus();
        this.setPreview.emit();
        return;
      }

      this.selectedThumbnailIndex = this.thumbnails.length;
      this.selectedThumbnail = {
        selectedImageFile: null,
        thumbnail: currentTimestamp,
        thumbnailPreviewUrl: this.generateThumbnailPreview(this.videoPlayer.nativeElement),
      };
      this.thumbnails.push(this.selectedThumbnail);
      this.onThumbnailChange();
      this.videoPlayer.nativeElement.focus();
    } else if (this.fileInput) {
      this.fileInput.nativeElement.click();
    }
    this.setPreview.emit();
  }
}