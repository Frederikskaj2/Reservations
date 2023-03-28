using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Frederikskaj2.Reservations.Application;

public interface IQuery<TDocument>
{
    string Sql { get; }
    IQuery<TDocument> Distinct();
    IQuery<TDocument> Top(int n);
    IQuery<TChild> Join<TChild>(Expression<Func<TDocument, IEnumerable<TChild>>> joinExpression, Expression<Func<TChild, bool>> whereExpression);
    IQuery<TDocument> Where(Expression<Func<TDocument, bool>> expression);
    IQuery<TDocument> OrderBy<TValue>(Expression<Func<TDocument, TValue>> expression);
    IQuery<TDocument> OrderByDescending<TValue>(Expression<Func<TDocument, TValue>> expression);
    IProjectedQuery<TDocument> Project();
    IProjectedQuery<TProjection> ProjectProperty<TProjection>(Expression<Func<TDocument, TProjection>> expression);
    IProjectedQuery<TProjection> ProjectTo<TProjection>(Expression<Func<TDocument, TProjection>> expression) where TProjection : class;
}
