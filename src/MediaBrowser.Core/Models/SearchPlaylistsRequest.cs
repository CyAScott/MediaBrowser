using System.ComponentModel.DataAnnotations;

namespace MediaBrowser.Models
{
    /// <summary>
    /// The search parameters for finding playlists.
    /// </summary>
    public class SearchPlaylistsRequest
    {
        /// <summary>
        /// Optional filter option.
        /// </summary>
        public PlaylistFilterOptions Filter { get; set; } = PlaylistFilterOptions.NoFilter;

        /// <summary>
        /// The field to sort by.
        /// </summary>
        public PlaylistSortOptions Sort { get; set; } = PlaylistSortOptions.Name;

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
        /// The number of playlists to read.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Take { get; set; } = 1;

        /// <summary>
        /// Optional. Keywords to search with.
        /// </summary>
        public string Keywords { get; set; }
    }

    /// <summary>
    /// The list of things to filter playlists by.
    /// </summary>
    public enum PlaylistFilterOptions
    {
        /// <summary>
        /// No filter will be applied.
        /// </summary>
        NoFilter
    }

    /// <summary>
    /// The list of things to sort playlists by.
    /// </summary>
    public enum PlaylistSortOptions
    {
        /// <summary>
        /// When the playlist was created.
        /// </summary>
        CreatedOn,

        /// <summary>
        /// The name of the playlist.
        /// </summary>
        Name
    }
}
