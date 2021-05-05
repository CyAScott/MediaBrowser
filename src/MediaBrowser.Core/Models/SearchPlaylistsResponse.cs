using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Models
{
    /// <summary>
    /// The search parameters for finding playlists.
    /// </summary>
    public class SearchPlaylistsResponse<TPlaylist> : SearchPlaylistsRequest
    {
        /// <inheritdoc/>
        public SearchPlaylistsResponse(SearchPlaylistsRequest request, int count, IEnumerable<TPlaylist> playlists)
        {
            Ascending = request.Ascending;
            Count = count;
            Filter = request.Filter;
            Keywords = request.Keywords;
            Results = playlists.ToArray();
            Skip = request.Skip;
            Sort = request.Sort;
            Take = request.Take;
        }

        /// <summary>
        /// The found playlists.
        /// </summary>
        public TPlaylist[] Results { get; }

        /// <summary>
        /// The number of playlists the query could return.
        /// </summary>
        public int Count { get; set; }
    }
}
