using MediaBrowser.Attributes;
using MediaBrowser.Filters;
using MediaBrowser.Models;
using MediaBrowser.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ViewsController : Controller
    {
        public ViewsController(Jwt jwt, IRoles roles, IUsers users)
        {
            Jwt = jwt;
            Roles = roles;
            Users = users;
        }

        public Jwt Jwt { get; }
        public IRoles Roles { get; }
        public IUsers Users { get; }

        [HttpGet("/"), AllowAnonymous]
        public IActionResult Index() => User.Identity is JwtPayload ? Redirect("/Media/Files") : Redirect("/Login");

        [HttpGet("/Login"), AllowAnonymous]
        public IActionResult Login() => View("Login");

        [HttpPost("/Login"), AllowAnonymous]
        public async Task<IActionResult> Login([FromForm]LoginRequest request)
        {
            var user = await Users.GetByUserName(request.UserName ?? "");

            if (user == null || user.DeletedOn != null || !user.IsPasswordValid(request.Password ?? ""))
            {
                return View("Login");
            }

            var jwt = new JwtPayload
            {
                FirstName = user.FirstName,
                Id = user.Id,
                LastName = user.LastName,
                UserName = user.UserName
            };

            jwt.Roles.UnionWith(user.Roles ?? new string[0]);

            Jwt.SetJwtCookie(HttpContext, jwt);

            return Redirect("/Media/Files");
        }

        [HttpPost("/Logout")]
        public IActionResult Logout()
        {
            Jwt.UnsetJwtCookie(HttpContext);

            return Redirect("/Login");
        }

        [HttpGet("/Media/{**path}")]
        public async Task<IActionResult> Media()
        {
            var jwt = User.Identity as JwtPayload;

            HashSet<string> allRoles = null;

            if (jwt.Roles.Contains(RequiresAdminRoleAttribute.AdminRole) &&
                await Roles.Count() < 1000)
            {
                allRoles = new HashSet<string>((await Roles.All()).Select(it => it.Name));
            }

            return View("Media", new MediaViewModel
            {
                AllRoles = allRoles,
                FirstName = jwt?.FirstName ?? "Anonymous",
                Id = jwt?.Id.ToString() ?? Guid.Empty.ToString(),
                LastName = jwt?.LastName ?? "User",
                Roles = jwt?.Roles ?? new HashSet<string>(),
                UserName = jwt?.UserName ?? "Unknown"
            });
        }
    }
}
