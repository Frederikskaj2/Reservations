using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Frederikskaj2.Reservations.Persistence;

public interface IQuery<TDocument>
{
    string Sql { get; }

    IQuery<TDocument> Distinct();

    IQuery<TDocument> Top(int n);

    IQuery<TChild> Join<TChild>(Expression<Func<TDocument, IEnumerable<TChild>>> joinExpression, Expression<Func<TChild, bool>> whereExpression);

    IProjectedQuery<TProjection> Join<TChild, TProjection>(
        Expression<Func<TDocument, IEnumerable<TChild>>> joinExpression,
        Expression<Func<TChild, bool>> whereExpression,
        Expression<Func<TDocument, TChild, TProjection>> projectionExpression);

    IQuery<TDocument> Where(Expression<Func<TDocument, bool>> expression);

    IQuery<TDocument> OrderBy<TValue>(Expression<Func<TDocument, TValue>> expression);

    IQuery<TDocument> OrderByDescending<TValue>(Expression<Func<TDocument, TValue>> expression);

    IProjectedQuery<TDocument> Project();

    IProjectedQuery<TProjection> Project<TProjection>(Expression<Func<TDocument, TProjection>> expression);
}
