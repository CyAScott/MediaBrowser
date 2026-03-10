import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

export interface MediaReadOnlyData {
  id: string;
  duration?: number;
  size?: number;
  md5: string;
  ctimeMs: string;
  mtimeMs: string;
  width?: number;
  height?: number;
  mime: string;
  rating?: number;
  published?: string;
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

  formatDateTime(epochMs: string): string {
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

  formatDuration(seconds?: number): string {
    return ReadonlyInfoSectionComponent.formatDuration(seconds);
  }

  formatFileSize(bytes?: number): string {
    const mb = (bytes ?? 0) / (1024 * 1024);
    return `${mb.toFixed(2)} MB`;
  }
}