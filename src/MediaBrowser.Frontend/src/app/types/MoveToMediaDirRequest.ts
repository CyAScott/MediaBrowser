import type { MediaInfo } from './SearchMediaRequest.js';

interface MoveToMediaDirRequest {
  filePath: string;
  nfo: MediaInfo;
  thumbnail: Thumbnail;
  fanartThumbnail?: Thumbnail;
}

interface Thumbnail {
  // Thumbnail timestamp in seconds
  timestamp: number;

  x: number | null;
  y: number | null;
  width: number | null;
  height: number | null;
}

export type { MoveToMediaDirRequest, Thumbnail };