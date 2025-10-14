interface SearchMediaRequest {
  cast?: string[];
  descending?: boolean;
  genres?: string[];
  keywords?: string;
  sort: 'title' | 'createdOn' | 'duration' | 'userStarRating';
  skip?: number;
  take?: number;
}

interface MediaInfo {
  cast: string[];
  ctimeMs: string;
  description: string;
  directors: string[];
  duration: number;
  fanartUrl: string;
  ffprobe: FfprobeResult;
  height: number;
  id: string;
  md5: string;
  mime: string;
  mtimeMs: string;
  originalTitle: string;
  path: string;
  producers: string[];
  published: string;
  rating: number;
  size: number;
  thumbnailUrl: string;
  title: string;
  url: string;
  userStarRating: number;
  width: number;
  writers: string[];
}

interface SearchMediaResponse {
  results: MediaInfo[];
  count: number;
}

interface FileStats {
  ctime: Date;
  ctimeMs: number;
  md5: string;
  mtimeMs: number;
  size: number;
  url: string;
}

interface FfprobeResult {
  ext?: string;
  mime?: string;
  streams: Stream[];
  format: Format;
}

interface Stream {
  index: number;
  codec_name: string;
  codec_long_name: string;
  profile?: string;
  codec_type: string;
  codec_tag_string: string;
  codec_tag: string;
  width?: number;
  height?: number;
  coded_width?: number;
  coded_height?: number;
  has_b_frames?: number;
  sample_aspect_ratio?: string;
  display_aspect_ratio?: string;
  pix_fmt?: string;
  level?: number;
  color_range?: string;
  color_space?: string;
  color_transfer?: string;
  color_primaries?: string;
  chroma_location?: string;
  refs?: number;
  view_ids_available?: string;
  view_pos_available?: string;
  id: string;
  r_frame_rate: string;
  avg_frame_rate: string;
  time_base: string;
  start_pts: number;
  start_time: string;
  duration_ts: number;
  duration: string;
  bit_rate?: string;
  nb_frames: string;
  extradata_size: number;
  disposition: Disposition;
  tags: StreamTags;
  sample_fmt?: string;
  sample_rate?: string;
  channels?: number;
  channel_layout?: string;
  bits_per_sample?: number;
  initial_padding?: number;
}

interface Disposition {
  default: number;
  dub: number;
  original: number;
  comment: number;
  lyrics: number;
  karaoke: number;
  forced: number;
  hearing_impaired: number;
  visual_impaired: number;
  clean_effects: number;
  attached_pic: number;
  timed_thumbnails: number;
  non_diegetic: number;
  captions: number;
  descriptions: number;
  metadata: number;
  dependent: number;
  still_image: number;
  multilayer: number;
}

interface StreamTags {
  creation_time: string;
  language: string;
  handler_name: string;
  vendor_id?: string;
}

interface Format {
  filename: string;
  nb_streams: number;
  nb_programs: number;
  nb_stream_groups: number;
  format_name: string;
  format_long_name: string;
  start_time: string;
  duration: string;
  size: string;
  bit_rate: string;
  probe_score: number;
  tags: FormatTags;
}

interface FormatTags {
  major_brand: string;
  minor_version: string;
  compatible_brands: string;
  creation_time: string;
  encoder: string;
}

export type { 
  MediaInfo,
  SearchMediaRequest,
  SearchMediaResponse,
  FileStats,
  FfprobeResult,
  Stream,
  Format,
  Disposition,
  StreamTags,
  FormatTags
};