import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, ElementRef, EventEmitter, inject, Input, Output, ViewChild } from '@angular/core';

export interface ThumbnailData {
  thumbnail: number | null;
  thumbnailPreviewUrl: string;
  selectedImageFile: File | null;
}

export interface MediaThumbnailData {
  mime: string;
  url?: string;
}

@Component({
  selector: 'app-thumbnail-section',
  imports: [CommonModule],
  templateUrl: './thumbnail-section.html',
  styleUrls: ['../media-editor.css', './thumbnail-section.css']
})
export class ThumbnailSectionComponent {
  @Input() isCreatingThumbnail: boolean = false;
  @Input() mediaData!: MediaThumbnailData;
  @Input() showSaveThumbnail: boolean = false;
  @Input() showSetThumbnail: boolean = false;
  @Input() thumbnailData: ThumbnailData = {
    thumbnail: null,
    thumbnailPreviewUrl: '',
    selectedImageFile: null
  };

  @Output() saveThumbnailEvent = new EventEmitter<void>();
  @Output() setPreview = new EventEmitter<void>();
  @Output() thumbnailDataChange = new EventEmitter<ThumbnailData>();

  @ViewChild('fileInput') fileInput?: ElementRef<HTMLInputElement>;
  @ViewChild('videoPlayer') videoPlayer?: ElementRef<HTMLVideoElement>;

  private cdr = inject(ChangeDetectorRef);
  private generateThumbnailPreview(videoElement: HTMLVideoElement): string {
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
  private handleImageFile(file: File): void {
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
      this.thumbnailData.selectedImageFile = file;
      this.thumbnailData.thumbnail = null;
      this.thumbnailData.thumbnailPreviewUrl = e.target?.result as string;
      this.onThumbnailDataChange();
      this.cdr.detectChanges();
    };
    reader.readAsDataURL(file);
  }
  private onThumbnailDataChange(): void {
    this.thumbnailDataChange.emit(this.thumbnailData);
  }

  isDragOver: boolean = false;

  getVideoUrl(): string {
    return this.mediaData?.url || '';
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

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.handleImageFile(file);
    }
  }

  onVideoMetadataLoaded(): void {
    if (this.thumbnailData.thumbnail !== null) {
      this.videoPlayer!.nativeElement.currentTime = this.thumbnailData.thumbnail;
    }
  }

  openFileBrowser(): void {
    this.fileInput?.nativeElement.click();
  }

  saveThumbnail(): void {
    this.onThumbnailDataChange();
    this.saveThumbnailEvent.emit();
  }

  setThumbnailPreview(): void {
    if (this.videoPlayer) {
      this.thumbnailData.thumbnail = this.videoPlayer.nativeElement.currentTime;
      this.thumbnailData.thumbnailPreviewUrl = this.generateThumbnailPreview(this.videoPlayer.nativeElement);
      this.onThumbnailDataChange();
    } else if (this.fileInput) {
      this.fileInput.nativeElement.click();
    }
    this.setPreview.emit();
  }
}