using Frederikskaj2.Reservations.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Frederikskaj2.Reservations.Persistence;

static class ProjectionParser
{
    public static string GetProjection(LambdaExpression expression, Dictionary<ParameterExpression, string> prefixes)
    {
        var body = expression.Body;
        return body switch
        {
            MemberInitExpression memberInit => ParseMemberInit(memberInit, prefixes),
            NewExpression @new => ParseNew(@new, prefixes),
            MemberExpression member => ParseMember(member, prefixes),
            ParameterExpression parameter => ParseParameter(parameter),
            _ => throw new ArgumentException("Expression is not a suitable projection.", nameof(expression)),
        };
    }

    static string ParseMemberInit(MemberInitExpression memberInit, Dictionary<ParameterExpression, string> prefixes)
    {
        return string.Join(", ", memberInit.Bindings.Select(Parse));

        string Parse(MemberBinding binding)
        {
            if (binding.BindingType is not MemberBindingType.Assignment)
                throw new ArgumentException("Binding is not assignment.", nameof(binding));
            if (binding is not MemberAssignment { Expression: MemberExpression } assignment)
                throw new ArgumentException("The member initialization is not supported.", nameof(binding));
            var visitor = new MemberVisitor();
            visitor.Visit(assignment.Expression);
            var prefix = prefixes.TryGetValue(visitor.Parameter, out var value)
                ? value
                : throw new ArgumentException("Binding does not have a prefix.", nameof(binding));
            return $"{prefix}{visitor.Result} as {assignment.Member.Name.ToCamelCase()}";
        }
    }

    static string ParseNew(NewExpression @new, Dictionary<ParameterExpression, string> prefixes)
    {
        var parameterNames = @new.Constructor!.GetParameters().Select(parameter => parameter.Name!).ToList();
        return string.Join(", ", @new.Arguments.Select((expression, index) => Parse(expression, parameterNames[index])));

        string Parse(Expression expression, string parameterName)
        {
            if (expression is UnaryExpression unary)
                expression = unary.Operand;
            if (expression is not MemberExpression)
                throw new ArgumentException("The parameter expression is not supported.", nameof(expression));
            var visitor = new MemberVisitor();
            visitor.Visit(expression);
            var prefix = prefixes.TryGetValue(visitor.Parameter, out var value)
                ? value
                : throw new ArgumentException("Expression does not have a prefix.", nameof(expression));
            return $"{prefix}{visitor.Result} as {parameterName.ToCamelCase()}";
        }
    }

    static string ParseMember(MemberExpression member, Dictionary<ParameterExpression, string> prefixes)
    {
        var visitor = new MemberVisitor();
        visitor.Visit(member);
        var prefix = prefixes.TryGetValue(visitor.Parameter, out var value)
            ? value
            : throw new ArgumentException("Member does not have a prefix.", nameof(member));
        return $"value {prefix}{visitor.Result}";
    }

    static string ParseParameter(ParameterExpression parameter) => parameter.Name!.ToCamelCase();

    class MemberVisitor : ExpressionVisitor
    {
        readonly StringBuilder stringBuilder = new();

        ParameterExpression? parameter;

        public ParameterExpression Parameter
        {
            get => parameter ?? throw new InvalidOperationException();
            private set
            {
                if (parameter is not null)
                    throw new InvalidOperationException();
                parameter = value;
            }
        }

        public string Result => stringBuilder.ToString();

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression is ParameterExpression expression)
                Parameter = expression;
            base.VisitMember(node);
            if (OneOfFunctions.IsOneOfType(node.Expression?.Type) && OneOfFunctions.IsAsTNumberMember(node.Member.Name, out var oneOfIndex))
            {
                var propertyName = OneOfFunctions.GetOneOfSerializationPropertyName(node.Expression!.Type, oneOfIndex);
                var propertyPath = $".{propertyName.ToCamelCase()}";
                stringBuilder.Append(propertyPath);
            }
            else
            {
                var propertyPath = $".{node.Member.Name.ToCamelCase()}";
                stringBuilder.Append(propertyPath);
            }
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            base.VisitMethodCall(node);
            if (node.Method.Name is "get_Item" && node.Arguments.Count is 1 && node.Arguments[0] is ConstantExpression constant)
                stringBuilder.Append(CultureInfo.InvariantCulture, $"[{constant.Value}]");
            return node;
        }
    }
}
