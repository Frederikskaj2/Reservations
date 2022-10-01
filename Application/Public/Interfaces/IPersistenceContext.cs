using LanguageExt;
using System;
using System.Collections.Generic;
using System.Net;

namespace Frederikskaj2.Reservations.Application;

public interface IPersistenceContext
{
    IPersistenceContextFactory Factory { get; }
    IUntracked Untracked { get; }
    T Item<T>();
    T Item<T>(string id);
    Option<T> ItemOption<T>();
    Option<T> ItemOption<T>(string id);
    IEnumerable<T> Items<T>();
    IPersistenceContext CreateItem<T>(string id, T item) where T : class;
    IPersistenceContext CreateItem<T>(T item, Func<T, string> idFunc) where T : class;
    IPersistenceContext CreateItems<T>(IEnumerable<T> items, Func<T, string> idFunc) where T : class;
    IPersistenceContext UpdateItem<T>(Func<T, T> transformer) where T : class;
    IPersistenceContext UpdateItem<T>(string id, Func<T, T> transformer) where T : class;
    IPersistenceContext UpdateItem<T>(string id, T item) where T : class;
    IPersistenceContext UpdateItems<T>(Func<T, T> transformer) where T : class;
    IPersistenceContext UpdateItems<T>(IEnumerable<string> ids, Func<T, T> transformer) where T : class;
    IPersistenceContext DeleteItem<T>() where T : class;
    IPersistenceContext DeleteItem<T>(string id) where T : class;
    EitherAsync<HttpStatusCode, IPersistenceContext> ReadItem<T>(string id) where T : class;
    EitherAsync<HttpStatusCode, IPersistenceContext> ReadItem<T>(string id, Func<T> notFoundFactory) where T : class;
    EitherAsync<HttpStatusCode, IPersistenceContext> ReadItems<T>(IQuery<T> query) where T : class;
    EitherAsync<HttpStatusCode, IPersistenceContext> Write();
    IQuery<TDocument> Query<TDocument>() where TDocument : class;
}
