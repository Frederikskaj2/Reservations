using System;
using static System.Linq.Expressions.Expression;

namespace Frederikskaj2.Reservations.Shared.Core;

static class EnumConverter<T> where T : Enum
{
    public static readonly Func<long, T> ToEnum = GenerateToEnum();
    public static readonly Func<T, long> ToInt64 = GenerateToInt64();

    static Func<long, T> GenerateToEnum()
    {
        var parameter = Parameter(typeof(long));
        var lambda = Lambda<Func<long, T>>(ConvertChecked(parameter, typeof(T)), parameter);
        return lambda.Compile();
    }

    static Func<T, long> GenerateToInt64()
    {
        var parameter = Parameter(typeof(T));
        var lambda = Lambda<Func<T, long>>(ConvertChecked(parameter, typeof(long)), parameter);
        return lambda.Compile();
    }
}
