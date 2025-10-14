import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MediaManager } from '../types/MediaManager';
import { MediaInfo } from '../types/SearchMediaRequest';

declare global {
  interface Window {
    mediaManager: MediaManager;
  }
}

interface FileInfo {
  filename: string;
  path: string;
}

@Component({
  selector: 'app-import',
  imports: [CommonModule],
  templateUrl: './import.html',
  styleUrls: ['./import.css']
})
export class ImportComponent implements OnInit {
  private cdr = inject(ChangeDetectorRef);
  private router = inject(Router);
  private readonly DIRECTORY_STORAGE_KEY = 'import-selected-directory';

  directoryPath: string | null = null;
  files: FileInfo[] = [];

  ngOnInit(): void {
    this.loadPersistedDirectory();
  }

  private loadPersistedDirectory(): void {
    const persistedPath = localStorage.getItem(this.DIRECTORY_STORAGE_KEY);
    if (persistedPath) {
      this.directoryPath = persistedPath;
      this.scanDirectory(persistedPath);
    }
  }

  private persistDirectory(path: string | null): void {
    if (path) {
      localStorage.setItem(this.DIRECTORY_STORAGE_KEY, path);
    } else {
      localStorage.removeItem(this.DIRECTORY_STORAGE_KEY);
    }
  }

  private async scanDirectory(path: string): Promise<void> {
    try {
      const files = await window.mediaManager.scanDirectory(path);
      this.files = files.map(file => ({
        filename: file.split(/[/\\]/).pop() || file,
        path: file
      }));
      this.cdr.detectChanges();
    } catch (error) {
      console.error('Error scanning directory:', error);
      this.files = [];
      this.cdr.detectChanges();
    }
  }

  async selectDirectory(): Promise<void> {
    try {
      const selectedPath = await window.mediaManager.selectDirectory();
      this.directoryPath = selectedPath;
      this.persistDirectory(selectedPath);
      
      if (selectedPath) {
        await this.scanDirectory(selectedPath);
      } else {
        this.files = [];
        this.cdr.detectChanges();
      }
    } catch (error) {
      console.error('Error selecting directory:', error);
    }
  }

  async importFile(path: string, filename: string): Promise<void> {
    try {
      const ffprobeResult = await window.mediaManager.ffprobe(path);
      const stats = await window.mediaManager.readFileStats(path);
      
      const videoStreams = ffprobeResult.streams.filter(it => it.width && it.height);

      const mediaData: MediaInfo = {
        title: filename,
        originalTitle: filename,
        description: '',
        rating: 0,
        userStarRating: 0,
        published: stats.ctime.toISOString().split('T')[0],
        cast: [],
        producers: [],
        directors: [],
        writers: [],
        //non-standard data
        ctimeMs: Math.floor(stats.ctimeMs).toString(),
        duration: parseFloat(ffprobeResult.format.duration) || 0,
        ffprobe: ffprobeResult,
        height: videoStreams[0].height!,
        id: '',
        md5: stats.md5,
        mime: ffprobeResult.mime!,
        mtimeMs: Math.floor(stats.mtimeMs).toString(),
        path: filename,
        size: stats.size,
        width: videoStreams[0].width!,
        //urls
        fanartUrl: '',
        thumbnailUrl: '',
        url: stats.url
      };

      const state = { 
        mediaData: mediaData,
        path: path
      };

      this.router.navigate(['/edit'], { state });
    } catch (error) {
      console.error('Error importing file:', error);
    }
  }
}