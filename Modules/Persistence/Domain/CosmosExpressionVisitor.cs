using Frederikskaj2.Reservations.Core;
using LanguageExt;
using NodaTime;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Frederikskaj2.Reservations.Persistence;

class CosmosExpressionVisitor<T>(string rootPropertyPath) : ExpressionVisitor
{
    readonly StringBuilder stringBuilder = new();

    public override string ToString() => stringBuilder.ToString();

    protected override Expression VisitBinary(BinaryExpression node)
    {
        stringBuilder.Append('(');
        Visit(node.Left);
        stringBuilder.Append(CultureInfo.InvariantCulture, $" {GetBinaryOperator(node.NodeType)} ");
        Visit(node.Right);
        stringBuilder.Append(')');
        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        stringBuilder.Append(FormatValue(node.Value));
        return base.VisitConstant(node);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node is { Expression: ConstantExpression constant, Member: FieldInfo instanceField })
        {
            var value = instanceField.GetValue(constant.Value);
            stringBuilder.Append(FormatValue(value));
        }
        else if (node.Expression is null && node.Member is FieldInfo staticField)
        {
            var value = staticField.GetValue(null);
            stringBuilder.Append(value);
        }
        else if (OneOfFunctions.IsOneOfType(node.Expression?.Type) && OneOfFunctions.IsIsTNumberMember(node.Member.Name, out var oneOfIndex))
        {
            stringBuilder.Append("is_defined(");
            base.VisitMember(node);
            if (node.Expression?.Type == typeof(T))
                stringBuilder.Append(rootPropertyPath);
            var propertyName = OneOfFunctions.GetOneOfSerializationPropertyName(node.Expression!.Type, oneOfIndex);
            var propertyPath = $".{propertyName.ToCamelCase()}";
            stringBuilder.Append(propertyPath);
            stringBuilder.Append(')');
        }
        else
        {
            base.VisitMember(node);
            if (node.Expression?.Type == typeof(T))
                stringBuilder.Append(rootPropertyPath);
            var propertyPath = $".{node.Member.Name.ToCamelCase()}";
            stringBuilder.Append(propertyPath);
        }

        return node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node is { Method: { IsSpecialName: true, Name: "get_Item" }, Arguments: [ConstantExpression constant] })
        {
            Visit(node.Object);
            var propertyPath = $".{constant.Value?.ToString().ToCamelCase()}";
            stringBuilder.Append(propertyPath);
            return node;
        }

        if ((node.Method.DeclaringType == typeof(Enumerable) || node.Method.DeclaringType == typeof(SeqExtensions)) &&
            node.Method is { IsGenericMethod: true, Name: nameof(Enumerable.Contains) })
        {
            Visit(node.Arguments[1]);
            stringBuilder.Append(" in (");
            Visit(node.Arguments[0]);
            stringBuilder.Append(')');
            return node;
        }

        if ((node.Method.DeclaringType?.IsGenericType ?? false) && node.Method.DeclaringType.GetGenericTypeDefinition() == typeof(HashSet<>) &&
            node.Method is { IsGenericMethod: false, Name: nameof(HashSet<object>.Contains) })
        {
            Visit(node.Arguments[0]);
            stringBuilder.Append(" in (");
            Visit(node.Object);
            stringBuilder.Append(')');
            return node;
        }

        if (node.Method == typeof(Enum).GetMethod("HasFlag"))
        {
            stringBuilder.Append("(is_defined(");
            Visit(node.Object);
            stringBuilder.Append(") and ");
            Visit(node.Object);
            var value = GetEnumAsNumber(node.Arguments[0]);
            stringBuilder.Append(CultureInfo.InvariantCulture, $" & {value} = {value})");
            return node;
        }

        throw new ArgumentException($"Method {node.Method} is not supported.", nameof(node));
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        stringBuilder.Append(GetUnaryOperator(node.NodeType));
        stringBuilder.Append('(');
        base.VisitUnary(node);
        stringBuilder.Append(')');
        return node;
    }

    static int GetEnumAsNumber(Expression node)
    {
        if (node is UnaryExpression { NodeType: ExpressionType.Convert } unary && unary.Type == typeof(Enum))
            return unary.Operand switch
            {
                ConstantExpression constant => (int) constant.Value!,
                MemberExpression { Expression: ConstantExpression constant, Member: FieldInfo field } => (int) field.GetValue(constant.Value)!,
                _ => throw new ArgumentException("Expression is not an enum value.", nameof(node)),
            };
        throw new ArgumentException("Expression is not an enum value.", nameof(node));
    }

    static string GetBinaryOperator(ExpressionType type) =>
        type switch
        {
            ExpressionType.Add => "+",
            ExpressionType.And => "&",
            ExpressionType.AndAlso => "and",
            ExpressionType.Divide => "/",
            ExpressionType.Equal => "=",
            ExpressionType.ExclusiveOr => "^",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LeftShift => "<<",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.Multiply => "*",
            ExpressionType.NotEqual => "!=",
            ExpressionType.Or => "|",
            ExpressionType.OrElse => "or",
            ExpressionType.RightShift => ">>",
            ExpressionType.Subtract => "-",
            _ => throw new ArgumentException($"{type} is not a supported binary operator.", nameof(type)),
        };

    static string GetUnaryOperator(ExpressionType type) =>
        type switch
        {
            ExpressionType.Negate => "-",
            ExpressionType.Not => "not ",
            ExpressionType.OnesComplement => "~",
            ExpressionType.UnaryPlus => "+",
            ExpressionType.Convert => "",
            _ => throw new ArgumentException($"{type} is not a supported unary operator.", nameof(type)),
        };

    // These formats should match the Cosmos JSON serialization used.
    static string FormatValue(object? value) =>
        value switch
        {
            string @string => $"""
                               "{@string.Replace(@"""", @"\""", StringComparison.Ordinal)}"
                               """,
            IEnumerable enumerable => string.Join(", ", enumerable.Cast<object?>().Select(FormatValue)),
            Enum @enum => Enum.Format(@enum.GetType(), @enum, "D"),
            DateTime dateTime => $"""
                                  "{dateTime:O}"
                                  """,
            DateTimeOffset dateTimeOffset => $"""
                                              "{dateTimeOffset:O}"
                                              """,
            Instant instant => $"""
                                "{instant.ToDateTimeUtc():O}"
                                """,
            LocalDate localDate => $"""
                                    "{localDate:yyyy-MM-dd}"
                                    """,
            bool @bool => @bool ? "true" : "false",
            IFormattable formattable => formattable.ToString(format: null, CultureInfo.InvariantCulture),
            null => "null",
            _ => throw new ArgumentException($"Cannot format type {value.GetType().FullName}.", nameof(value)),
        };
}
