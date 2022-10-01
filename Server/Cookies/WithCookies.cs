using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Server;

record WithCookies<T>(T Value, IEnumerable<Cookie> Cookie) where T : class;
