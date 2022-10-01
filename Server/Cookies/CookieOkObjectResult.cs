using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

public class CookieOkObjectResult : ObjectResult
{
    public CookieOkObjectResult(object? value, IEnumerable<Cookie> cookies) : base(value)
    {
        Cookies = cookies;
        StatusCode = StatusCodes.Status200OK;
    }

    public IEnumerable<Cookie> Cookies { get; }

    public override Task ExecuteResultAsync(ActionContext context)
    {
        foreach (var cookie in Cookies)
        {
            var options = new Microsoft.AspNetCore.Http.CookieOptions
            {
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                MaxAge = cookie.MaxAge?.ToTimeSpan(),
                Path = "/"
            };
            context.HttpContext.Response.Cookies.Append(cookie.Name, cookie.Value, options);
        }

        return base.ExecuteResultAsync(context);
    }
}
