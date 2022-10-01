using System;
using static System.Linq.Expressions.Expression;

namespace Frederikskaj2.Reservations.Client;

static class EnumConverter<T> where T : Enum
{
    public static readonly Func<int, T> Convert = GenerateConverter();

    static Func<int, T> GenerateConverter()
    {
        var parameter = Parameter(typeof(int));
        var lambda = Lambda<Func<int, T>>(ConvertChecked(parameter, typeof(T)), parameter);
        return lambda.Compile();
    }
}