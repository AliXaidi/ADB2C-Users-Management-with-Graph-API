using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Graph.Models.ExternalConnectors;
using static Microsoft.Graph.Constants;

namespace WebApplication1.Middlewares
{
    public class AfterAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AfterAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            if (context != null && context?.User?.Identity?.IsAuthenticated == true && context?.User?.Claims.FirstOrDefault(a => a.Type == "extension_Company") == null
                && !context.Request.Path.Value.Contains("SignOut"))
            {
                context.Response.Redirect("/Account/SignOut?error=401");
                return;
            }
            else
            {
                await _next(context);
            }
        }
    }
    public static class AfterAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAfterAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AfterAuthenticationMiddleware>();
        }
    }
}
