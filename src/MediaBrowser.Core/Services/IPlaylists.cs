using MediaBrowser.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaBrowser.Services
{
    /// <summary>
    /// An object for playlist CRUD.
    /// </summary>
    public interface IPlaylists
    {
        /// <summary>
        /// Gets a playlist by name.
        /// </summary>
        Task<IPlaylist> GetByName(string name);

        /// <summary>
        /// Create a playlist.
        /// </summary>
        Task<IPlaylist> Create(CreatePlaylistRequest request, Guid playlistId, Guid userId, IThumbnail[] thumbnails = null);

        /// <summary>
        /// Read a playlist by id.
        /// </summary>
        Task<IPlaylist> Get(Guid playlistId);

        /// <summary>
        /// Read a playlist by ids.
        /// </summary>
        Task<IPlaylist[]> Get(IEnumerable<Guid> playlistIds);

        /// <summary>
        /// Search playlists.
        /// </summary>
        Task<SearchPlaylistsResponse<IPlaylist>> Search(SearchPlaylistsRequest request, Guid userId, HashSet<string> userRoles);

        /// <summary>
        /// Updates a playlist by id.
        /// </summary>
        Task<IPlaylist> Update(Guid playlistId, UpdatePlaylistRequest request, IThumbnail[] thumbnails = null);
    }
}
