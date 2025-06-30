using System;

namespace Frederikskaj2.Reservations.Users;

public static class EmailExtensions
{
    const char maskChar = '*';

    public static string Mask(this EmailAddress email) => email.ToString().MaskEmail()!;

    // xyzzy@foobar.com => x****@*****r.com
    public static string? MaskEmail(this string? email)
    {
        if (email is null)
            return null;

        var atIndex = email.IndexOf('@', StringComparison.Ordinal);
        if (atIndex <= 0)
            return email;
        var dotIndex = email.LastIndexOf('.');
        if (dotIndex is -1 || dotIndex <= atIndex + 1 || dotIndex == email.Length - 1)
            return email;

        return string.Create(email.Length, (email, atIndex, dotIndex), SpanAction);

        static void SpanAction(Span<char> span, (string Email, int AtIndex, int DotIndex) state)
        {
            if (state.AtIndex > 1)
            {
                span[0] = state.Email[0];
                span[1..state.AtIndex].Fill(maskChar);
            }
            else
                span[0] = maskChar;

            span[state.AtIndex] = '@';

            var hideCount = Math.Max(state.DotIndex - state.AtIndex - 2, 1);
            span.Slice(state.AtIndex + 1, hideCount).Fill(maskChar);
            var visibleStart = state.AtIndex + 1 + hideCount;
            var visible = state.Email.AsSpan(visibleStart);
            visible.CopyTo(span[visibleStart..]);
        }
    }
}
