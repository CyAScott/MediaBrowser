using MediaBrowser.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace MediaBrowser.Filters
{
    /// <summary>
    /// Filter actions by roles.
    /// </summary>
    public class RolesFilter : IActionFilter
    {
        void IActionFilter.OnActionExecuted(ActionExecutedContext context)
        {
        }

        /// <inheritdoc/>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            var attributes =
                (actionDescriptor?.MethodInfo?.GetCustomAttributes(true).OfType<BaseRoleRequirementAttribute>() ?? Enumerable.Empty<BaseRoleRequirementAttribute>()).Concat(
                actionDescriptor?.ControllerTypeInfo?.GetCustomAttributes(true).OfType<BaseRoleRequirementAttribute>() ?? Enumerable.Empty<BaseRoleRequirementAttribute>())
                .ToArray();
            var user = context.HttpContext.User.Identity as JwtPayload;

            if (user != null && attributes.Length > 0 && !attributes.Any(it => it.MeetsRequirements(user)))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
