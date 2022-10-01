using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

record Results<T>(HttpStatusCode Status, IEnumerable<T> Items) : IHttpResult
{
    public Results(IEnumerable<T> items) : this(HttpStatusCode.OK, items) { }
    public Results(HttpStatusCode status) : this(status, Enumerable.Empty<T>()) { }
}
