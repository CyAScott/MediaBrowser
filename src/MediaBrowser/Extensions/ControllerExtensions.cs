using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.Extensions
{
    public static class ControllerExtensions
    {
        public static bool IsCached(this Controller controller, DateTime lastModified)
        {
            var requestHeaders = controller.Request.GetTypedHeaders();

            if (requestHeaders.IfModifiedSince.HasValue &&
                requestHeaders.IfModifiedSince.Value.AddMinutes(1) >= lastModified)
            {
                return true;
            }

            var responseHeaders = controller.Response.GetTypedHeaders();

            responseHeaders.CacheControl = new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.FromDays(14)
            };
            responseHeaders.LastModified = lastModified;

            return false;
        }
    }
}
