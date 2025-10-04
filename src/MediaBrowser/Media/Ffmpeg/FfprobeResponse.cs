namespace MediaBrowser.Media.Ffmpeg;

public class FfprobeResponse
{
    [JsonPropertyName("streams")]
    public required IReadOnlyList<Streams> Streams { get; init; }
    [JsonPropertyName("format")]
    public required Format Format { get; init; }
}

public class Streams
{
    [JsonPropertyName("index")]
    public required int Index { get; init; }
    [JsonPropertyName("codec_name")]
    public required string CodecName { get; init; }
    [JsonPropertyName("codec_long_name")]
    public required string CodecLongName { get; init; }
    [JsonPropertyName("profile")]
    public required string Profile { get; init; }
    [JsonPropertyName("codec_type")]
    public required string CodecType { get; init; }
    [JsonPropertyName("codec_tag_string")]
    public required string CodecTagString { get; init; }
    [JsonPropertyName("codec_tag")]
    public required string CodecTag { get; init; }
    [JsonPropertyName("sample_fmt")]
    public required string SampleFmt { get; init; }
    [JsonPropertyName("sample_rate")]
    public required string SampleRate { get; init; }
    [JsonPropertyName("channels")]
    public required int Channels { get; init; }
    [JsonPropertyName("channel_layout")]
    public required string ChannelLayout { get; init; }
    [JsonPropertyName("bits_per_sample")]
    public required int BitsPerSample { get; init; }
    [JsonPropertyName("initial_padding")]
    public required int InitialPadding { get; init; }
    [JsonPropertyName("id")]
    public required string Id { get; init; }
    [JsonPropertyName("r_frame_rate")]
    public required string RFrameRate { get; init; }
    [JsonPropertyName("avg_frame_rate")]
    public required string AvgFrameRate { get; init; }
    [JsonPropertyName("time_base")]
    public required string TimeBase { get; init; }
    [JsonPropertyName("start_pts")]
    public required int StartPts { get; init; }
    [JsonPropertyName("start_time")]
    public required string StartTime { get; init; }
    [JsonPropertyName("duration_ts")]
    public required int DurationTs { get; init; }
    [JsonPropertyName("duration")]
    public required string Duration { get; init; }
    [JsonPropertyName("bit_rate")]
    public required string BitRate { get; init; }
    [JsonPropertyName("nb_frames")]
    public required string NbFrames { get; init; }
    [JsonPropertyName("extradata_size")]
    public required int ExtradataSize { get; init; }
    [JsonPropertyName("disposition")]
    public required Disposition Disposition { get; init; }
    [JsonPropertyName("tags")]
    public required DispositionTags Tags { get; init; }
    [JsonPropertyName("width")]
    public required int Width { get; init; }
    [JsonPropertyName("height")]
    public required int Height { get; init; }
    [JsonPropertyName("coded_width")]
    public required int CodedWidth { get; init; }
    [JsonPropertyName("coded_height")]
    public required int CodedHeight { get; init; }
    [JsonPropertyName("has_b_frames")]
    public required int HasBFrames { get; init; }
    [JsonPropertyName("sample_aspect_ratio")]
    public required string SampleAspectRatio { get; init; }
    [JsonPropertyName("display_aspect_ratio")]
    public required string DisplayAspectRatio { get; init; }
    [JsonPropertyName("pix_fmt")]
    public required string PixFmt { get; init; }
    [JsonPropertyName("level")]
    public required int Level { get; init; }
    [JsonPropertyName("color_range")]
    public required string ColorRange { get; init; }
    [JsonPropertyName("color_space")]
    public required string ColorSpace { get; init; }
    [JsonPropertyName("color_transfer")]
    public required string ColorTransfer { get; init; }
    [JsonPropertyName("color_primaries")]
    public required string ColorPrimaries { get; init; }
    [JsonPropertyName("chroma_location")]
    public required string ChromaLocation { get; init; }
    [JsonPropertyName("field_order")]
    public required string FieldOrder { get; init; }
    [JsonPropertyName("refs")]
    public required int Refs { get; init; }
    [JsonPropertyName("is_avc")]
    public required string IsAvc { get; init; }
    [JsonPropertyName("nal_length_size")]
    public required string NalLengthSize { get; init; }
    [JsonPropertyName("bits_per_raw_sample")]
    public required string BitsPerRawSample { get; init; }
}

public class Disposition
{
    [JsonPropertyName("default")]
    public required int Default { get; init; }
    [JsonPropertyName("dub")]
    public required int Dub { get; init; }
    [JsonPropertyName("original")]
    public required int Original { get; init; }
    [JsonPropertyName("comment")]
    public required int Comment { get; init; }
    [JsonPropertyName("lyrics")]
    public required int Lyrics { get; init; }
    [JsonPropertyName("karaoke")]
    public required int Karaoke { get; init; }
    [JsonPropertyName("forced")]
    public required int Forced { get; init; }
    [JsonPropertyName("hearing_impaired")]
    public required int HearingImpaired { get; init; }
    [JsonPropertyName("visual_impaired")]
    public required int VisualImpaired { get; init; }
    [JsonPropertyName("clean_effects")]
    public required int CleanEffects { get; init; }
    [JsonPropertyName("attached_pic")]
    public required int AttachedPic { get; init; }
    [JsonPropertyName("timed_thumbnails")]
    public required int TimedThumbnails { get; init; }
    [JsonPropertyName("non_diegetic")]
    public required int NonDiegetic { get; init; }
    [JsonPropertyName("captions")]
    public required int Captions { get; init; }
    [JsonPropertyName("descriptions")]
    public required int Descriptions { get; init; }
    [JsonPropertyName("metadata")]
    public required int Metadata { get; init; }
    [JsonPropertyName("dependent")]
    public required int Dependent { get; init; }
    [JsonPropertyName("still_image")]
    public required int StillImage { get; init; }
    [JsonPropertyName("multilayer")]
    public required int Multilayer { get; init; }
}

public class DispositionTags
{
    public string creation_time { get; init; }
    public string language { get; init; }
    public string handler_name { get; init; }
    public string vendor_id { get; init; }
}

public class Format
{
    [JsonPropertyName("filename")]
    public required string Filename { get; init; }
    [JsonPropertyName("nb_streams")]
    public required int NbStreams { get; init; }
    [JsonPropertyName("nb_programs")]
    public required int NbPrograms { get; init; }
    [JsonPropertyName("nb_stream_groups")]
    public required int NbStreamGroups { get; init; }
    [JsonPropertyName("format_name")]
    public required string FormatName { get; init; }
    [JsonPropertyName("format_long_name")]
    public required string FormatLongName { get; init; }
    [JsonPropertyName("start_time")]
    public required string StartTime { get; init; }
    [JsonPropertyName("duration")]
    public required string Duration { get; init; }
    [JsonPropertyName("size")]
    public required string Size { get; init; }
    [JsonPropertyName("bit_rate")]
    public required string BitRate { get; init; }
    [JsonPropertyName("probe_score")]
    public required int ProbeScore { get; init; }
    [JsonPropertyName("tags")]
    public required FormatTags Tags { get; init; }
}

public class FormatTags
{
    [JsonPropertyName("major_brand")]
    public required string MajorBrand { get; init; }
    [JsonPropertyName("minor_version")]
    public required string MinorVersion { get; init; }
    [JsonPropertyName("compatible_brands")]
    public required string CompatibleBrands { get; init; }
    [JsonPropertyName("creation_time")]
    public required string CreationTime { get; init; }
    [JsonPropertyName("encoder")]
    public required string Encoder { get; init; }
}