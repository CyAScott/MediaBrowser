using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaBrowser;

public class IndexController(IWebHostEnvironment env) : Controller
{
    /// <summary>
    /// Catch-all route for Angular client-side routing.
    /// This will serve the index.html file for any route that doesn't match API endpoints.
    /// The Order = int.MaxValue ensures this route is evaluated last.
    /// </summary>
    /// <param name="catchAll">The route path that wasn't matched by other controllers</param>
    /// <returns>The index.html file content</returns>
    [HttpGet("/{**catchAll}", Order = int.MaxValue),
     AllowAnonymous]
    public IActionResult Index(string? catchAll = null)
    {
        // Don't handle API routes or swagger routes - these should be handled by other controllers
        if (Request.Path.StartsWithSegments("/api") || 
            Request.Path.StartsWithSegments("/swagger"))
        {
            return NotFound();
        }

        var indexPath = Path.Combine(env.WebRootPath, "index.html");

        return PhysicalFile(indexPath, "text/html");
    }
}