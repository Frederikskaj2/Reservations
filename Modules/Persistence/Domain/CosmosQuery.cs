using Frederikskaj2.Reservations.Core;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Frederikskaj2.Reservations.Persistence;

class CosmosQuery<TDocument>(CosmosQueryDefinition queryDefinition) : IProjectedQuery<TDocument>
{
    string? sql;

    public string Sql => sql ??= queryDefinition.ToSql();

    public IQuery<TDocument> Distinct() =>
        new CosmosQuery<TDocument>(queryDefinition with { Distinct = true });

    public IQuery<TDocument> Top(int n) =>
        new CosmosQuery<TDocument>(queryDefinition with { Top = $" top {n}" });

    public IQuery<TChild> Join<TChild>(
        Expression<Func<TDocument, IEnumerable<TChild>>> joinExpression, Expression<Func<TChild, bool>> whereExpression)
    {
        var visitor = new CosmosExpressionVisitor<TChild>("child");
        visitor.Visit(whereExpression);
        return new CosmosQuery<TChild>(
            queryDefinition with
            {
                Projection = "value child",
                Join = $"child in {GetPropertyPath(joinExpression.Body, queryDefinition.RootPrefix)}",
                Where = $"{queryDefinition.Where} and {visitor}",
                RootPrefix = "child",
            });
    }

    public IProjectedQuery<TProjection> Join<TChild, TProjection>(
        Expression<Func<TDocument, IEnumerable<TChild>>> joinExpression,
        Expression<Func<TChild, bool>> whereExpression,
        Expression<Func<TDocument, TChild, TProjection>> projectionExpression)
    {
        const string childPrefix = "child";
        var visitor = new CosmosExpressionVisitor<TChild>(childPrefix);
        visitor.Visit(whereExpression);
        var parentParameter = projectionExpression.Parameters[0];
        var childParameter = projectionExpression.Parameters[1];
        var projection = ProjectionParser.GetProjection(
            projectionExpression,
            new() { [parentParameter] = queryDefinition.RootPrefix, [childParameter] = childPrefix });
        return new CosmosQuery<TProjection>(
            queryDefinition with
            {
                Projection = projection,
                Join = $"child in {GetPropertyPath(joinExpression.Body, queryDefinition.RootPrefix)}",
                Where = $"{queryDefinition.Where} and {visitor}",
                RootPrefix = "",
            });
    }

    public IQuery<TDocument> Where(Expression<Func<TDocument, bool>> expression)
    {
        var visitor = new CosmosExpressionVisitor<TDocument>(queryDefinition.RootPrefix);
        visitor.Visit(expression);
        return new CosmosQuery<TDocument>(queryDefinition with { Where = $"{queryDefinition.Where} and {visitor}" });
    }

    // Notice multiple OrderBy require that a composite index is defined.
    public IQuery<TDocument> OrderBy<TValue>(Expression<Func<TDocument, TValue>> expression)
    {
        var visitor = new CosmosExpressionVisitor<TDocument>(queryDefinition.RootPrefix);
        visitor.Visit(expression);
        var prefix = queryDefinition.OrderBy is not null
            ? $"{queryDefinition.OrderBy}, "
            : "";
        return new CosmosQuery<TDocument>(queryDefinition with { OrderBy = $"{prefix}{visitor}" });
    }

    // Notice multiple OrderBy require that a composite index is defined.
    public IQuery<TDocument> OrderByDescending<TValue>(Expression<Func<TDocument, TValue>> expression)
    {
        var visitor = new CosmosExpressionVisitor<TDocument>(queryDefinition.RootPrefix);
        visitor.Visit(expression);
        var prefix = queryDefinition.OrderBy is not null
            ? $"{queryDefinition.OrderBy}, "
            : "";
        return new CosmosQuery<TDocument>(queryDefinition with { OrderBy = $"{prefix}{visitor} desc" });
    }

    public IProjectedQuery<TDocument> Project() =>
        new CosmosQuery<TDocument>(queryDefinition with { Projection = "value root.v" });

    public IProjectedQuery<TProjection> Project<TProjection>(Expression<Func<TDocument, TProjection>> expression)
    {
        var parameter = expression.Parameters[0];
        var projection = ProjectionParser.GetProjection(expression, new() { [parameter] = queryDefinition.RootPrefix });
        return new CosmosQuery<TProjection>(queryDefinition with { Projection = projection });
    }

    static string GetPropertyPath(Expression expression, string prefix)
    {
        while (true)
        {
            if (expression.NodeType is ExpressionType.Parameter) return prefix;

            if (expression is MemberExpression member)
            {
                var memberName = OneOfFunctions.IsAsTNumberMember(member.Member.Name, out var oneOfIndex)
                    ? OneOfFunctions.GetOneOfSerializationPropertyName(member.Type, oneOfIndex)
                    : member.Member.Name;
                var propertyPath = $".{memberName.ToCamelCase()}";
                return GetPropertyPath(member.Expression!, prefix) + propertyPath;
            }

            if (IsIndexer(expression, out var indexer))
            {
                if (indexer.Arguments[0] is not ConstantExpression constant) throw new ArgumentException("Index expression does not have constant parameter value.", nameof(expression));
                var propertyPath = $"[{constant.Value?.ToString().ToCamelCase()}]";
                return GetPropertyPath(indexer.Object!, prefix) + propertyPath;
            }

            if (IsMap(expression, out var select))
            {
                if (select.Object is not MemberExpression source) throw new ArgumentException("Projection source is not a member.", nameof(expression));
                if (select.Arguments is not [LambdaExpression lambda]) throw new ArgumentException("Projection is not supported.", nameof(expression));
                if (lambda is not { Parameters: [{ } parameter] }) throw new ArgumentException("Projection is not supported.", nameof(expression));
                var sourceName = source.Member.Name.ToCamelCase();
                var projection = ProjectionParser.GetProjection(lambda, new() { [parameter] = prefix });
                return $"array(select {projection} from {sourceName} in {GetPropertyPath(select.Object, prefix)})";
            }

            if (expression is UnaryExpression { NodeType: ExpressionType.Convert } convert)
            {
                expression = convert.Operand;
                continue;
            }

            throw new ArgumentException("Expression is not property access.", nameof(expression));
        }
    }

    static bool IsIndexer(Expression expression, [MaybeNullWhen(false)] out MethodCallExpression methodCall)
    {
        if (expression is MethodCallExpression { Method: { IsSpecialName: true, Name: "get_Item" }, Arguments.Count: 1 } call)
        {
            methodCall = call;
            return true;
        }

        methodCall = null;
        return false;
    }

    static bool IsMap(Expression expression, [MaybeNullWhen(false)] out MethodCallExpression methodCall)
    {
        if (expression is MethodCallExpression
            {
                Method:
                {
                    IsStatic: false,
                    Name: nameof(Seq<object>.Map) or nameof(Seq<object>.Select),
                },
                Arguments.Count: 1,
            } call)
        {
            methodCall = call;
            return true;
        }

        methodCall = null;
        return false;
    }
}
