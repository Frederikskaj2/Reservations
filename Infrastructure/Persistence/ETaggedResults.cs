using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

record ETaggedResults<T>(HttpStatusCode Status, IEnumerable<ETagged<T>> Items) : IHttpResult
{
    public ETaggedResults(IEnumerable<ETagged<T>> items) : this(HttpStatusCode.OK, items) { }
    public ETaggedResults(HttpStatusCode status) : this(status, Enumerable.Empty<ETagged<T>>()) { }
}
