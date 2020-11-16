using MediaBrowser.Filters;
using MediaBrowser.Models;
using MediaBrowser.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ViewsController : Controller
    {
        public ViewsController(Jwt jwt, IUsers users)
        {
            Jwt = jwt;
            Users = users;
        }

        public Jwt Jwt { get; }
        public IUsers Users { get; }

        [HttpGet("/"), AllowAnonymous]
        public IActionResult Index() => User.Identity is JwtPayload ? Redirect("/Media") : Redirect("/Login");

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

            return Redirect("/Media");
        }

        [HttpPost("/Logout")]
        public IActionResult Logout()
        {
            Jwt.UnsetJwtCookie(HttpContext);

            return Redirect("/Login");
        }

        [HttpGet("/Media/{**path}")]
        public IActionResult Media() => View("Media");
    }
}
