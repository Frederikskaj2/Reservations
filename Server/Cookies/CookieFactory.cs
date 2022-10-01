using Microsoft.Net.Http.Headers;
using NodaTime;
using System.Linq;
using System.Net;

namespace Frederikskaj2.Reservations.Server;

public static class CookieFactory
{
    // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Set-Cookie#cookie_prefixes
    const string hostPrefix = "__Host-";

    public static Cookie CreateCookie(string cookieName, string cookieValue, Duration? duration) => new(GetFullCookieName(cookieName), cookieValue, duration);

    public static string? ParseCookie(string cookieName, string? cookie)
    {
        if (cookie is null)
            return null;
        if (!CookieHeaderValue.TryParseList(cookie.Split(';'), out var cookieHeaderValues))
            return null;
        var fullCookieName = GetFullCookieName(cookieName);
        var cookieHeaderValue = cookieHeaderValues.FirstOrDefault(headerValue => headerValue.Name == fullCookieName);
        return cookieHeaderValue is not null ? WebUtility.UrlDecode(cookieHeaderValue.Value.Value) : null;
    }

    static string GetFullCookieName(string cookieName) => $"{hostPrefix}{cookieName}";
}
