using MediaBrowser.Extensions;
using MediaBrowser.Models;
using MediaBrowser.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileSystem = System.IO.File;
using FormFile = MediaBrowser.Services.FormFile;

namespace MediaBrowser.Controllers
{
    /// <summary>
    /// Crud for playlists.
    /// </summary>
    [ApiController]
    public class PlaylistsController : Controller
    {
        private bool isCached(DateTime lastModified)
        {
            var requestHeaders = Request.GetTypedHeaders();

            if (requestHeaders.IfModifiedSince.HasValue &&
                requestHeaders.IfModifiedSince.Value.AddMinutes(1) >= lastModified)
            {
                return true;
            }

            var responseHeaders = Response.GetTypedHeaders();

            responseHeaders.CacheControl = new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.FromDays(14)
            };
            responseHeaders.LastModified = lastModified;

            return false;
        }
        private async Task<bool> doRolesExists(HashSet<string> readRoles, HashSet<string> updateRoles)
        {
            var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (readRoles != null)
            {
                roles.UnionWith(readRoles);
            }

            if (updateRoles != null)
            {
                roles.UnionWith(updateRoles);
            }

            if (roles.Count == 0)
            {
                return true;
            }

            var matchedRoles = (await Task.WhenAll(roles.Select(Roles.GetByName))).Select(it => it.Name).ToArray();

            return roles.Count == matchedRoles.Length;
        }
        private static readonly HashSet<string> acceptedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/gif",
            "image/jpeg",
            "image/png"
        };

        /// <inheritdoc/>
        public PlaylistsController(IFiles files, IPlaylists playlists, IRoles roles, IUploadRequestProcessor uploadRequestProcessor)
        {
            Files = files;
            Playlists = playlists;
            Roles = roles;
            UploadRequestProcessor = uploadRequestProcessor;
        }

        /// <summary>
        /// The collection of media files.
        /// </summary>
        public IFiles Files { get; }

        /// <summary>
        /// The collection of playlists.
        /// </summary>
        public IPlaylists Playlists { get; }

        /// <summary>
        /// Processes playlist creation.
        /// </summary>
        public IUploadRequestProcessor UploadRequestProcessor { get; }

        /// <summary>
        /// The collection of user roles.
        /// </summary>
        public IRoles Roles { get; }

        /// <summary>
        /// Creates a playlist.
        /// </summary>
        [HttpPost("api/playlists"), Authorize, Consumes("application/json"), DisableRequestSizeLimit]
        public async Task<ActionResult<PlaylistReadModel>> Create([FromBody]CreatePlaylistRequest request)
        {
            var jwt = User.Identity as JwtPayload;
            if (jwt == null)
            {
                return Unauthorized();
            }

            if (!await doRolesExists(request.ReadRoles, request.UpdateRoles))
            {
                return NotFound();
            }

            try
            {
                var playlist = await UploadRequestProcessor.Process(jwt.Id, request);

                return new ActionResult<PlaylistReadModel>(new PlaylistReadModel(playlist));
            }
            catch (UploadException error)
            {
                return StatusCode((int)error.HttpStatusCode, error.Message);
            }
        }

        /// <summary>
        /// Creates a playlist.
        /// </summary>
        [HttpPost("api/playlists"), Authorize, ApiExplorerSettings(IgnoreApi = true), Consumes("multipart/form-data"), DisableRequestSizeLimit]
        public async Task<ActionResult<PlaylistReadModel>> Create()
        {
            var jwt = User.Identity as JwtPayload;
            if (jwt == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(Request.ContentType) || Request.ContentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) == -1)
            {
                return StatusCode(415);
            }

            var form = Request.Form;

            if (!form.TryGetValue("json", out var json))
            {
                return BadRequest("Missing \"json\" part.");
            }

            var request = JsonConvert.DeserializeObject<CreatePlaylistRequest>(json.First());
            if (!await doRolesExists(request.ReadRoles, request.UpdateRoles))
            {
                return NotFound();
            }

            var thumbnails = new List<FormFile>();

            foreach (var uploadedThumbnail in form.Files
                .Where(it => !string.IsNullOrEmpty(it.ContentType) &&
                             !string.IsNullOrEmpty(it.FileName) &&
                             !string.IsNullOrEmpty(it.Name) &&
                             acceptedMimeTypes.Contains(it.ContentType)))
            {
                thumbnails.Add(new FormFile(uploadedThumbnail.CopyToAsync, true, uploadedThumbnail.ContentType, uploadedThumbnail.FileName, uploadedThumbnail.Name, uploadedThumbnail.Length));
            }

            try
            {
                var playlist = await UploadRequestProcessor.Process(jwt.Id, request, thumbnails);

                return new ActionResult<PlaylistReadModel>(new PlaylistReadModel(playlist));
            }
            catch (UploadException error)
            {
                return StatusCode((int)error.HttpStatusCode, error.Message);
            }
        }

        /// <summary>
        /// Read a playlist by id.
        /// </summary>
        [HttpGet("api/playlists/{playlistId:guid}"), Authorize]
        public async Task<ActionResult<PlaylistReadModel>> Get(Guid playlistId)
        {
            var jwt = User.Identity as JwtPayload;
            var playlist = await Playlists.Get(playlistId);

            if (jwt == null || playlist == null)
            {
                return NotFound();
            }

            if (!playlist.CanRead(jwt.Id, jwt.Roles))
            {
                return Unauthorized();
            }

            return new ActionResult<PlaylistReadModel>(new PlaylistReadModel(playlist));
        }

        /// <summary>
        /// Removes a file from a playlist.
        /// </summary>
        [HttpDelete("api/playlists/{playlistId:guid}/files/{fileId:guid}"), Authorize]
        public Task<ActionResult<FileReadModel>> DeletePlaylistReference(Guid playlistId, Guid fileId) =>
            SetPlaylistReference(playlistId, fileId, null);

        /// <summary>
        /// Adds a file to the end of a playlist.
        /// </summary>
        [HttpPatch("api/playlists/{playlistId:guid}/files/{fileId:guid}"), Authorize]
        public Task<ActionResult<FileReadModel>> SetPlaylistReference(Guid playlistId, Guid fileId) =>
            SetPlaylistReference(playlistId, fileId, -1);

        /// <summary>
        /// Adds a file to a playlist.
        /// </summary>
        [HttpPatch("api/playlists/{playlistId:guid}/files/{fileId:guid}/{index:min(0)}"), Authorize]
        public async Task<ActionResult<FileReadModel>> SetPlaylistReference(Guid playlistId, Guid fileId, int? index)
        {
            var jwt = User.Identity as JwtPayload;
            var playlist = await Playlists.Get(playlistId);

            if (jwt == null || playlist == null)
            {
                return NotFound();
            }

            if (!playlist.CanUpdate(jwt.Id, jwt.Roles))
            {
                return Unauthorized();
            }

            var file = await Files.Get(fileId);

            if (file == null)
            {
                return NotFound();
            }

            if (!file.CanRead(jwt.Id, jwt.Roles))
            {
                return Unauthorized();
            }

            file = await Files.SetPlaylistReference(fileId, playlist, index);

            return new ActionResult<FileReadModel>(new FileReadModel(file));
        }

        /// <summary>
        /// Search files.
        /// </summary>
        [HttpGet("api/playlists/{playlistId:guid}/files/search"), Authorize]
        public async Task<SearchFilesResponse<FileReadModel>> Search(Guid playlistId, [FromQuery]SearchFilesRequest query)
        {
            var jwt = User.Identity as JwtPayload;
            if (jwt == null)
            {
                return null;
            }

            var response = await Files.Search(query, jwt.Id, jwt.Roles, playlistId);

            return new SearchFilesResponse<FileReadModel>(query, response.Count, response.Results.Select(it => new FileReadModel(it)));
        }

        /// <summary>
        /// Updates playlist.
        /// </summary>
        [HttpPut("api/playlists/{playlistId:guid}"), Authorize, ApiExplorerSettings(IgnoreApi = true), Consumes("multipart/form-data"), DisableRequestSizeLimit]
        public async Task<ActionResult<PlaylistReadModel>> Update(Guid playlistId)
        {
            if (string.IsNullOrEmpty(Request.ContentType) || Request.ContentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) == -1)
            {
                return StatusCode(415);
            }

            var jwt = User.Identity as JwtPayload;
            var playlist = await Playlists.Get(playlistId);

            if (jwt == null || playlist == null)
            {
                return NotFound();
            }

            if (!playlist.CanUpdate(jwt.Id, jwt.Roles))
            {
                return Unauthorized();
            }

            var form = Request.Form;

            if (!form.TryGetValue("json", out var json))
            {
                return BadRequest("Missing \"json\" part.");
            }

            var request = JsonConvert.DeserializeObject<UpdatePlaylistRequest>(json.First());
            if (!await doRolesExists(request.ReadRoles, request.UpdateRoles))
            {
                return NotFound();
            }

            var thumbnails = new List<FormFile>();

            foreach (var uploadedThumbnail in form.Files
                .Where(it => !string.IsNullOrEmpty(it.ContentType) &&
                             !string.IsNullOrEmpty(it.FileName) &&
                             !string.IsNullOrEmpty(it.Name) &&
                             acceptedMimeTypes.Contains(it.ContentType)))
            {
                thumbnails.Add(new FormFile(uploadedThumbnail.CopyToAsync, true, uploadedThumbnail.ContentType, uploadedThumbnail.FileName, uploadedThumbnail.Name, uploadedThumbnail.Length));
            }

            try
            {
                playlist = await UploadRequestProcessor.Process(jwt.Id, playlist, request, thumbnails);

                return new ActionResult<PlaylistReadModel>(new PlaylistReadModel(playlist));
            }
            catch (UploadException error)
            {
                return StatusCode((int)error.HttpStatusCode, error.Message);
            }
        }

        /// <summary>
        /// Read a playlist's thumbnail's contents.
        /// </summary>
        [HttpGet("api/playlists/{playlistId:guid}/thumbnails/{md5:guid}/contents"), Authorize, ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> GetThumbnail(Guid playlistId, Guid md5)
        {
            var jwt = User.Identity as JwtPayload;
            var playlist = await Playlists.Get(playlistId);

            if (jwt == null || playlist == null)
            {
                return NotFound();
            }

            if (!playlist.CanRead(jwt.Id, jwt.Roles))
            {
                return Unauthorized();
            }

            var thumbnail = playlist.Thumbnails.FirstOrDefault(it => it.Md5 == md5);
            if (thumbnail == null)
            {
                return NotFound();
            }

            if (isCached(thumbnail.CreatedOn))
            {
                return StatusCode(304);
            }

            return File(FileSystem.OpenRead(thumbnail.Location), thumbnail.ContentType, false);
        }

        /// <summary>
        /// Search playlists.
        /// </summary>
        [HttpGet("api/playlists/search"), Authorize]
        public async Task<SearchPlaylistsResponse<PlaylistReadModel>> Search([FromQuery]SearchPlaylistsRequest query)
        {
            var jwt = User.Identity as JwtPayload;
            if (jwt == null)
            {
                return null;
            }

            var response = await Playlists.Search(query, jwt.Id, jwt.Roles);

            return new SearchPlaylistsResponse<PlaylistReadModel>(query, response.Count, response.Results.Select(it => new PlaylistReadModel(it)));
        }

        /// <summary>
        /// Updates a playlist by id.
        /// </summary>
        [HttpPut("api/playlists/{playlistId:guid}"), Authorize, Consumes("application/json")]
        public async Task<ActionResult<PlaylistReadModel>> Update(Guid playlistId, [FromBody]UpdatePlaylistRequest request)
        {
            var jwt = User.Identity as JwtPayload;
            var playlist = await Playlists.Get(playlistId);

            if (jwt == null || playlist == null)
            {
                return NotFound();
            }

            if (!playlist.CanUpdate(jwt.Id, jwt.Roles))
            {
                return Unauthorized();
            }

            if (!await doRolesExists(request.ReadRoles, request.UpdateRoles))
            {
                return NotFound();
            }

            playlist = await Playlists.Update(playlistId, request);

            return playlist == null ? NotFound() : new ActionResult<PlaylistReadModel>(new PlaylistReadModel(playlist));
        }
    }
}
