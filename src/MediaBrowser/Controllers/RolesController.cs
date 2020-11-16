using MediaBrowser.Attributes;
using MediaBrowser.Models;
using MediaBrowser.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaBrowser.Controllers
{
    /// <summary>
    /// Crud for user roles.
    /// </summary>
    [ApiController]
    public class RolesController : Controller
    {
        /// <inheritdoc/>
        public RolesController(IRoles roles) => Roles = roles;

        /// <summary>
        /// The collection of roles.
        /// </summary>
        public IRoles Roles { get; }

        /// <summary>
        /// Create a role.
        /// </summary>
        [HttpPost("api/roles"), Authorize, RequiresAdminRole]
        public async Task<ActionResult<RoleReadModel>> Create([FromBody]CreateRoleRequest request) =>
            new ActionResult<RoleReadModel>(new RoleReadModel(await Roles.Create(request)));

        /// <summary>
        /// Read a user role by id.
        /// </summary>
        [HttpGet("api/roles/{roleId:guid}"), Authorize, RequiresAdminRole]
        public async Task<ActionResult<RoleReadModel>> Get(Guid roleId)
        {
            var role = await Roles.Get(roleId);
            return role == null ? NotFound() : new ActionResult<RoleReadModel>(new RoleReadModel(role));
        }

        /// <summary>
        /// Search user roles.
        /// </summary>
        [HttpGet("api/roles/search"), Authorize, RequiresAdminRole]
        public async Task<SearchRolesResponse<RoleReadModel>> Search([FromQuery]SearchRolesRequest query)
        {
            var response = await Roles.Search(query);
            return new SearchRolesResponse<RoleReadModel>(query, response.Results.Select(it => new RoleReadModel(it)));
        }

        /// <summary>
        /// Updates a user role by id.
        /// </summary>
        [HttpPut("api/roles/{roleId:guid}"), Authorize, RequiresAdminRole]
        public async Task<ActionResult<RoleReadModel>> Update(Guid roleId, [FromBody]UpdateRoleRequest request)
        {
            var role = await Roles.Update(roleId, request);

            return role == null ? NotFound() : new ActionResult<RoleReadModel>(new RoleReadModel(role));
        }
    }
}
