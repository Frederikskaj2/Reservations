using LanguageExt;
using System;
using System.Collections.Generic;
using System.Net;

namespace Frederikskaj2.Reservations.Application;

public interface IUntracked
{
    EitherAsync<HttpStatusCode, T> ReadItem<T>(string id) where T : class;
    EitherAsync<HttpStatusCode, T> ReadItem<T>(string id, Func<T> notFoundFactory);
    EitherAsync<HttpStatusCode, IEnumerable<T>> ReadItems<T>(IQuery<T> query);
    EitherAsync<HttpStatusCode, IEnumerable<T>> ReadItems<T>(IProjectedQuery<T> query);
}
