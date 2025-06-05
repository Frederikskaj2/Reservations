using LanguageExt;

namespace Frederikskaj2.Reservations.Core;

public static class OptionExtensions
{
    public static T? ToNullable<T>(this Option<T> option) where T : struct =>
        option.Case switch
        {
            T value => value,
            _ => null,
        };

    public static T? ToNullableReference<T>(this Option<T> option) where T : class =>
        option.Case switch
        {
            T value => value,
            _ => null,
        };
}
