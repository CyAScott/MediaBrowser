import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

export interface MediaReadOnlyData {
  ctimeMs: string;
  duration?: number;
  height?: number;
  id: string;
  md5: string;
  mime: string;
  mtimeMs: string;
  parentId?: string;
  published?: string;
  size?: number;
  start?: number;
  width?: number;
}

@Component({
  selector: 'app-readonly-info-section',
  imports: [CommonModule],
  templateUrl: './readonly-info-section.html',
  styleUrls: ['../media-editor.css', './readonly-info-section.css']
})
export class ReadonlyInfoSectionComponent {
  @Input() mediaData!: MediaReadOnlyData;
  @Input() showSection: boolean = true;

  static formatDateTime(epochMs: string): string {
    const date = new Date(parseInt(epochMs));
    return date.toLocaleString();
  }

  static formatDuration(seconds?: number): string {
    seconds ??= 0;

    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const remainingSeconds = Math.floor(seconds % 60);
    
    return `${hours ? `${hours}:` : ''}${minutes.toString().padStart(hours ? 2 : 1, '0')}:${remainingSeconds.toString().padStart(2, '0')}`;
  }

  static formatFileSize(bytes?: number): string {
    bytes ??= 0;

    const mb = bytes / (1024 * 1024);
    return `${mb.toFixed(2)} MB`;
  }

  formatDateTime(epochMs: string): string {
    return ReadonlyInfoSectionComponent.formatDateTime(epochMs);
  }

  formatDuration(seconds?: number): string {
    return ReadonlyInfoSectionComponent.formatDuration(seconds);
  }

  formatFileSize(bytes?: number): string {
    return ReadonlyInfoSectionComponent.formatFileSize(bytes);
  }
}