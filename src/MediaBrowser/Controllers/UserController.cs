using MediaBrowser.Attributes;
using MediaBrowser.Models;
using MediaBrowser.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MediaBrowser.Controllers
{
    /// <summary>
    /// Crud for users.
    /// </summary>
    [ApiController]
    public class UserController : Controller
    {
        /// <inheritdoc/>
        public UserController(IRoles roles, IUsers users)
        {
            Roles = roles;
            Users = users;
        }

        /// <summary>
        /// The collection of roles.
        /// </summary>
        public IRoles Roles { get; }

        /// <summary>
        /// The collection of users.
        /// </summary>
        public IUsers Users { get; }

        /// <summary>
        /// Create a user.
        /// </summary>
        [HttpPost("api/users"), Authorize, RequiresAdminRole]
        public async Task<ActionResult<UserReadModel>> Create([FromBody]CreateUserRequest request)
        {
            var roles = request.Roles == null ? null : new RoleSet((await Task.WhenAll(request.Roles.Select(Roles.GetByName))).Select(it => it.Name));

            if (roles != null)
            {
                if (request.Roles.Except(roles, StringComparer.OrdinalIgnoreCase).Any())
                {
                    return StatusCode((int)HttpStatusCode.NotFound);
                }

                request.Roles = roles;
            }

            return new ActionResult<UserReadModel>(new UserReadModel(await Users.Create(request)));
        }

        /// <summary>
        /// Read a user by id.
        /// </summary>
        [HttpGet("api/users/{userId:guid}"), Authorize]
        public async Task<ActionResult<UserReadModel>> Get(Guid userId)
        {
            var jwt = User.Identity as JwtPayload;
            var user = await Users.Get(userId);

            if (jwt == null || user == null)
            {
                return NotFound();
            }

            if (jwt.Id != user.Id && !jwt.Roles.Contains(RequiresAdminRoleAttribute.AdminRole))
            {
                return Unauthorized();
            }

            return new ActionResult<UserReadModel>(new UserReadModel(user));
        }

        /// <summary>
        /// Search users.
        /// </summary>
        [HttpGet("api/users/search"), Authorize, RequiresAdminRole]
        public async Task<SearchUsersResponse<UserReadModel>> Search([FromQuery]SearchUsersRequest query)
        {
            var response = await Users.Search(query);
            return new SearchUsersResponse<UserReadModel>(query, response.Count, response.Results.Select(it => new UserReadModel(it)));
        }

        /// <summary>
        /// Updates a user by id.
        /// </summary>
        [HttpPut("api/users/{userId:guid}"), Authorize]
        public async Task<ActionResult<UserReadModel>> Update(Guid userId, [FromBody]UpdateUserRequest request)
        {
            var jwt = User.Identity as JwtPayload;
            var user = await Users.Get(userId);

            if (jwt == null || user == null)
            {
                return NotFound();
            }

            if (jwt.Id != user.Id && !jwt.Roles.Contains(RequiresAdminRoleAttribute.AdminRole))
            {
                return Unauthorized();
            }

            user = await Users.Update(userId, request);

            return user == null ? NotFound() : new ActionResult<UserReadModel>(new UserReadModel(user));
        }

        /// <summary>
        /// Soft deletes a user by id.
        /// </summary>
        [HttpDelete("api/users/{userId:guid}"), Authorize, RequiresAdminRole]
        public async Task<ActionResult<UserReadModel>> Delete(Guid userId)
        {
            var user = await Users.Delete(userId);
            return user == null ? NotFound() : new ActionResult<UserReadModel>(new UserReadModel(user));
        }
    }
}
