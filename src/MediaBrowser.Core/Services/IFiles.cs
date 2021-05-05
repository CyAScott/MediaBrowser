using MediaBrowser.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaBrowser.Services
{
    /// <summary>
    /// An object for media file CRUD.
    /// </summary>
    public interface IFiles
    {
        /// <summary>
        /// Gets a file by name.
        /// </summary>
        Task<IFile> GetByName(string name);

        /// <summary>
        /// Read a file by id.
        /// </summary>
        Task<IFile> Get(Guid fileId);

        /// <summary>
        /// Search files.
        /// </summary>
        Task<SearchFilesResponse<IFile>> Search(SearchFilesRequest request, Guid userId, HashSet<string> userRoles, Guid? playlistId = null);

        /// <summary>
        /// Sets the playlist reference for a media file.
        /// </summary>
        /// <param name="fileId">The id for the media file.</param>
        /// <param name="playlist">The playlist to set.</param>
        /// <param name="index">If null then removes the reference else if -1 then sets to the last item in the list else sets the index.</param>
        Task<IFile> SetPlaylistReference(Guid fileId, IPlaylist playlist, int? index = null);

        /// <summary>
        /// Updates a file by id.
        /// </summary>
        Task<IFile> Update(Guid fileId, UpdateFileRequest request, IThumbnail[] thumbnails = null);

        /// <summary>
        /// Uploads a file.
        /// </summary>
        Task<IFile> Upload(UploadFileRequest request, UploadedFileInfo file);
    }
}
