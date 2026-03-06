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
     AllowAnonymous,
     ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Index(string? catchAll = null) =>
        PhysicalFile(Path.Combine(env.WebRootPath, "index.html"), "text/html");
}