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
        Task<SearchFilesResponse<IFile>> Search(SearchFilesRequest request, Guid userId, HashSet<string> userRoles);

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
