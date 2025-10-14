import { CastMember } from './CastMember.js';
import type { MoveToMediaDirRequest, Thumbnail } from './MoveToMediaDirRequest.js';
import type { FfprobeResult, FileStats, MediaInfo, SearchMediaRequest, SearchMediaResponse } from './SearchMediaRequest.js';
import type { Settings } from './Settings.js';

export interface MediaManager {
  ffprobe: (filePath: string) => Promise<FfprobeResult>;
  getCastMembers: () => Promise<CastMember[]>;
  getMediaById(id: string): Promise<MediaInfo | null>;
  getSettings: () => Promise<Settings>;
  moveToMediaDir: (request: MoveToMediaDirRequest) => Promise<void>;
  readFileStats: (filePath: string) => Promise<FileStats>;
  resyncMediaDb: () => Promise<void>;
  scanDirectory: (directoryPath: string) => Promise<string[]>;
  searchMedia: (request: SearchMediaRequest) => Promise<SearchMediaResponse>;
  selectDirectory: () => Promise<string | null>;
  setSettings: (settings: Settings) => Promise<void>;
  update: (media: MediaInfo) => Promise<void>;
  updateThumbnails: (id: string, thumbnail: Thumbnail, fanartThumbnail: Thumbnail | null) => Promise<void>;
}