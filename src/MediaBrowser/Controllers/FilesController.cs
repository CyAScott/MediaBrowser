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
    /// Crud for media files.
    /// </summary>
    [ApiController]
    public class FilesController : Controller
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
            "audio/mp3",
            "audio/ogg",
            "audio/wav",
            "image/gif",
            "image/jpeg",
            "image/png",
            "video/mp4"
        };

        /// <inheritdoc/>
        public FilesController(IFiles files, IRoles roles, IUploadRequestProcessor uploadRequestProcessor)
        {
            Files = files;
            Roles = roles;
            UploadRequestProcessor = uploadRequestProcessor;
        }

        /// <summary>
        /// The collection of media files.
        /// </summary>
        public IFiles Files { get; }

        /// <summary>
        /// Processes media file uploads.
        /// </summary>
        public IUploadRequestProcessor UploadRequestProcessor { get; }

        /// <summary>
        /// The collection of user roles.
        /// </summary>
        public IRoles Roles { get; }

        /// <summary>
        /// Uploads a file.
        /// </summary>
        [HttpPost("api/files"), Authorize, ApiExplorerSettings(IgnoreApi = true), DisableRequestSizeLimit]
        public async Task<ActionResult<FileReadModel>> Upload()
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

            var request = JsonConvert.DeserializeObject<UploadFileRequest>(json.First());
            if (!await doRolesExists(request.ReadRoles, request.UpdateRoles))
            {
                return NotFound();
            }

            var files = new List<FormFile>();

            foreach (var uploadedFile in form.Files
                .Where(it => !string.IsNullOrEmpty(it.ContentType) &&
                             !string.IsNullOrEmpty(it.FileName) &&
                             !string.IsNullOrEmpty(it.Name) &&
                             acceptedMimeTypes.Contains(it.ContentType)))
            {
                if (uploadedFile.Name.StartsWith("thumbnail", StringComparison.OrdinalIgnoreCase))
                {
                    if (!uploadedFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    {
                        return BadRequest("Invalid thumbnail content type.");
                    }
                    files.Add(new FormFile(uploadedFile.CopyToAsync, true, uploadedFile.ContentType, uploadedFile.FileName, uploadedFile.Name, uploadedFile.Length));
                }
                else
                {
                    files.Add(new FormFile(uploadedFile.CopyToAsync, false, uploadedFile.ContentType, uploadedFile.FileName, uploadedFile.Name, uploadedFile.Length));
                }
            }

            try
            {
                var file = await UploadRequestProcessor.Process(jwt.Id, request, files);

                return new ActionResult<FileReadModel>(new FileReadModel(file));
            }
            catch (UploadException error)
            {
                return StatusCode((int)error.HttpStatusCode, error.Message);
            }
        }

        /// <summary>
        /// Read a file by id.
        /// </summary>
        [HttpGet("api/files/{fileId:guid}"), Authorize]
        public async Task<ActionResult<FileReadModel>> Get(Guid fileId)
        {
            var jwt = User.Identity as JwtPayload;
            var file = await Files.Get(fileId);

            if (jwt == null || file == null)
            {
                return NotFound();
            }

            if (!file.CanRead(jwt.Id, jwt.Roles))
            {
                return Unauthorized();
            }

            return new ActionResult<FileReadModel>(new FileReadModel(file));
        }

        /// <summary>
        /// Read a file's content.
        /// </summary>
        [HttpGet("api/files/{fileId:guid}/contents"), Authorize, ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> GetContents(Guid fileId)
        {
            var jwt = User.Identity as JwtPayload;
            var file = await Files.Get(fileId);

            if (jwt == null || file == null)
            {
                return NotFound();
            }

            if (!file.CanRead(jwt.Id, jwt.Roles))
            {
                return Unauthorized();
            }

            if (isCached(file.UploadedOn))
            {
                return StatusCode(304);
            }

            return File(FileSystem.OpenRead(file.Location), file.ContentType, !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Updates thumbnails.
        /// </summary>
        [HttpPut("api/files/{fileId:guid}/thumbnails"), Authorize, ApiExplorerSettings(IgnoreApi = true), DisableRequestSizeLimit]
        public async Task<ActionResult<FileReadModel>> Update(Guid fileId)
        {
            if (string.IsNullOrEmpty(Request.ContentType) || Request.ContentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) == -1)
            {
                return StatusCode(415);
            }

            var jwt = User.Identity as JwtPayload;
            var file = await Files.Get(fileId);

            if (jwt == null || file == null)
            {
                return NotFound();
            }

            if (!file.CanUpdate(jwt.Id, jwt.Roles))
            {
                return Unauthorized();
            }

            var form = Request.Form;

            if (!form.TryGetValue("json", out var json))
            {
                return BadRequest("Missing \"json\" part.");
            }

            var request = JsonConvert.DeserializeObject<UpdateFileRequest>(json.First());
            if (!await doRolesExists(request.ReadRoles, request.UpdateRoles))
            {
                return NotFound();
            }

            var files = new List<FormFile>();

            foreach (var uploadedFile in form.Files
                .Where(it => !string.IsNullOrEmpty(it.ContentType) &&
                             !string.IsNullOrEmpty(it.FileName) &&
                             !string.IsNullOrEmpty(it.Name) &&
                             acceptedMimeTypes.Contains(it.ContentType)))
            {
                if (!uploadedFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Invalid thumbnail content type.");
                }
                files.Add(new FormFile(uploadedFile.CopyToAsync, true, uploadedFile.ContentType, uploadedFile.FileName, uploadedFile.Name, uploadedFile.Length));
            }

            try
            {
                file = await UploadRequestProcessor.Process(jwt.Id, file, request, files);

                return new ActionResult<FileReadModel>(new FileReadModel(file));
            }
            catch (UploadException error)
            {
                return StatusCode((int)error.HttpStatusCode, error.Message);
            }
        }

        /// <summary>
        /// Read a file's thumbnail's contents.
        /// </summary>
        [HttpGet("api/files/{fileId:guid}/thumbnails/{md5:guid}/contents"), Authorize, ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> GetThumbnail(Guid fileId, Guid md5)
        {
            var jwt = User.Identity as JwtPayload;
            var file = await Files.Get(fileId);

            if (jwt == null || file == null)
            {
                return NotFound();
            }

            if (!file.CanRead(jwt.Id, jwt.Roles))
            {
                return Unauthorized();
            }

            var thumbnail = file.Thumbnails.FirstOrDefault(it => it.Md5 == md5);
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
        /// Search files.
        /// </summary>
        [HttpGet("api/files/search"), Authorize]
        public async Task<SearchFilesResponse<FileReadModel>> Search([FromQuery]SearchFilesRequest query)
        {
            var jwt = User.Identity as JwtPayload;
            if (jwt == null)
            {
                return null;
            }

            var response = await Files.Search(query, jwt.Id, jwt.Roles);

            return new SearchFilesResponse<FileReadModel>(query, response.Count, response.Results.Select(it => new FileReadModel(it)));
        }

        /// <summary>
        /// Updates a file by id.
        /// </summary>
        [HttpPut("api/files/{fileId:guid}"), Authorize]
        public async Task<ActionResult<FileReadModel>> Update(Guid fileId, [FromBody]UpdateFileRequest request)
        {
            var jwt = User.Identity as JwtPayload;
            var file = await Files.Get(fileId);

            if (jwt == null || file == null)
            {
                return NotFound();
            }

            if (!file.CanUpdate(jwt.Id, jwt.Roles))
            {
                return Unauthorized();
            }

            if (!await doRolesExists(request.ReadRoles, request.UpdateRoles))
            {
                return NotFound();
            }

            file = await Files.Update(fileId, request);

            return file == null ? NotFound() : new ActionResult<FileReadModel>(new FileReadModel(file));
        }
    }
}
