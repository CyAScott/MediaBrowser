using System.ComponentModel.DataAnnotations;

namespace MediaBrowser.Models
{
    /// <summary>
    /// The search parameters for finding media files.
    /// </summary>
    public class SearchFilesRequest
    {
        /// <summary>
        /// Optional filter option.
        /// </summary>
        public FileFilterOptions Filter { get; set; } = FileFilterOptions.Html5Friendly;

        /// <summary>
        /// The field to sort by.
        /// </summary>
        public FileSortOptions Sort { get; set; } = FileSortOptions.Name;

        /// <summary>
        /// Sorts in ascending order when true.
        /// </summary>
        public bool Ascending { get; set; } = true;

        /// <summary>
        /// The number of media files to skip.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int Skip { get; set; }

        /// <summary>
        /// The number of media files to read.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Take { get; set; } = 1;

        /// <summary>
        /// Optional. Keywords to search with.
        /// </summary>
        public string Keywords { get; set; }
    }

    /// <summary>
    /// The list of things to filter media files by.
    /// </summary>
    public enum FileFilterOptions
    {
        /// <summary>
        /// Only returns audio files that are html5 friendly.
        /// </summary>
        AudioFiles,

        /// <summary>
        /// Only returns media files that are html5 friendly.
        /// </summary>
        Html5Friendly,

        /// <summary>
        /// No filter will be applied.
        /// </summary>
        NoFilter,

        /// <summary>
        /// Returns media files that are NOT html5 friendly.
        /// </summary>
        NonHtml5Friendly,

        /// <summary>
        /// Only returns photo files.
        /// </summary>
        Photos,

        /// <summary>
        /// Only returns video files that are html5 friendly.
        /// </summary>
        Videos
    }

    /// <summary>
    /// The list of things to sort media files by.
    /// </summary>
    public enum FileSortOptions
    {
        /// <summary>
        /// When the media file was created.
        /// </summary>
        CreatedOn,

        /// <summary>
        /// The time duration of the media file.
        /// </summary>
        Duration,

        /// <summary>
        /// The name of the media file.
        /// </summary>
        Name,

        /// <summary>
        /// The type of media file.
        /// </summary>
        Type
    }
}
