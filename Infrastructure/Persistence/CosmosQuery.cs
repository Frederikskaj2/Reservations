using Frederikskaj2.Reservations.Application;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

class CosmosQuery<TDocument> : IQuery<TDocument>, IProjectedQuery<TDocument>
{
    readonly CosmosQueryDefinition queryDefinition;
    string? sql;

    public CosmosQuery(CosmosQueryDefinition queryDefinition) => this.queryDefinition = queryDefinition;

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
        return new CosmosQuery<TChild>(queryDefinition with
        {
            Projection = "child",
            Join = $"child in {GetPropertyPath(joinExpression.Body, queryDefinition.RootPrefix)}",
            Where = $"{queryDefinition.Where} and {visitor}",
            RootPrefix = "child"
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
        new CosmosQuery<TDocument>(queryDefinition with { Projection = "value " + queryDefinition.RootPrefix });

    public IProjectedQuery<TProjection> ProjectProperty<TProjection>(Expression<Func<TDocument, TProjection>> expression) =>
        new CosmosQuery<TProjection>(queryDefinition with { Projection = $"value {GetPropertyPath(expression.Body, queryDefinition.RootPrefix)}" });

    public IProjectedQuery<TProjection> ProjectTo<TProjection>(Expression<Func<TDocument, TProjection>> expression) where TProjection : class =>
        new CosmosQuery<TProjection>(queryDefinition with { Projection = $"{GetProjection(expression.Body, queryDefinition.RootPrefix)}" });

    static string GetPropertyPath(Expression expression, string prefix)
    {
        if (expression.NodeType == ExpressionType.Parameter)
            return prefix;

        if (expression is MemberExpression member)
        {
            var propertyPath = $".{member.Member.Name.ToCamelCase()}";
            return GetPropertyPath(member.Expression!, prefix) + propertyPath;
        }

        if (IsIndexer(expression, out var indexer))
        {
            if (indexer.Arguments[0] is not ConstantExpression constant)
                throw new ArgumentException("Index expression does not have constant parameter value.", nameof(expression));
            var propertyPath = $"[{constant.Value?.ToString().ToCamelCase()}]";
            return GetPropertyPath(indexer.Object!, prefix) + propertyPath;
        }

        if (IsMap(expression, out var select))
        {
            if (select.Object is not MemberExpression source)
                throw new ArgumentException("Projection source is not a member.", nameof(expression));
            var sourceName = source.Member.Name.ToCamelCase();
            var projection = GetProjection(((LambdaExpression) select.Arguments[0]).Body, sourceName);
            return $"array(select {projection} from {sourceName} in {GetPropertyPath(select.Object, prefix)})";
        }

        if (expression is UnaryExpression { NodeType: ExpressionType.Convert } convert)
            return GetPropertyPath(convert.Operand, prefix);

        throw new ArgumentException("Expression is not property access.", nameof(expression));
    }

    static bool IsIndexer(Expression expression, [MaybeNullWhen(false)] out MethodCallExpression methodCall)
    {
        if (expression is MethodCallExpression call && call.Method.IsSpecialName && call.Method.Name == "get_Item" && call.Arguments.Count == 1)
        {
            methodCall = call;
            return true;
        }

        methodCall = default;
        return false;
    }

    static bool IsMap(Expression expression, [MaybeNullWhen(false)] out MethodCallExpression methodCall)
    {
        if (expression is MethodCallExpression call &&
            !call.Method.IsStatic &&
            call.Method.Name is nameof(Seq<object>.Map) or nameof(Seq<object>.Select) &&
            call.Arguments.Count == 1)
        {
            methodCall = call;
            return true;
        }

        methodCall = default;
        return false;
    }

    static string GetProjection(Expression expression, string prefix)
    {
        if (expression is not MemberInitExpression memberInit)
            throw new ArgumentException("Expression is not member initialization.", nameof(expression));
        if (memberInit.Bindings.Any(binding => binding.BindingType != MemberBindingType.Assignment))
            throw new ArgumentException("Only member assignment is supported.", nameof(expression));
        var assignments = memberInit.Bindings.Cast<MemberAssignment>()
            .Select(assignment => $"{GetPropertyPath(assignment.Expression, prefix)} as {assignment.Member.Name.ToCamelCase()}");
        return string.Join(", ", assignments);
    }
}
