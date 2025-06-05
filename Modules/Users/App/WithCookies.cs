using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Users;

static class WithCookies
{
    public static WithCookies<T> Create<T>(T value, IEnumerable<Cookie> cookies) where T : class => new(value, cookies);
}

record WithCookies<T>(T Value, IEnumerable<Cookie> Cookies) where T : class;
