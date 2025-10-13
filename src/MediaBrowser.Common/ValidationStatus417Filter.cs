using Microsoft.AspNetCore.Mvc.Filters;

namespace MediaBrowser;

public class ValidationStatus417Filter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new ObjectResult(context.ModelState)
            {
                StatusCode = 417
            };
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
