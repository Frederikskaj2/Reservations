using System;

namespace Frederikskaj2.Reservations.Server
{
    internal static class MaskStringExtensions
    {
        private const char MaskChar = '*';

        // xyzzy@foobar.com => x****@*****r.com
        public static string? MaskEmail(this string? email)
        {
            if (email == null)
                return null;

            var atIndex = email.IndexOf('@', StringComparison.Ordinal);
            if (atIndex <= 0)
                return email;
            var dotIndex = email.LastIndexOf('.');
            if (dotIndex == -1 || dotIndex <= atIndex + 1 || dotIndex == email.Length - 1)
                return email;

            return string.Create(email.Length, (email, atIndex, dotIndex), SpanAction);

            static void SpanAction(Span<char> span, (string Email, int AtIndex, int DotIndex) state)
            {
                if (state.AtIndex > 1)
                {
                    span[0] = state.Email[0];
                    span.Slice(1, state.AtIndex - 1).Fill(MaskChar);
                }
                else
                {
                    span[0] = MaskChar;
                }

                span[state.AtIndex] = '@';

                var hideCount = Math.Max(state.DotIndex - state.AtIndex - 2, 1);
                span.Slice(state.AtIndex + 1, hideCount).Fill(MaskChar);
                var visibleStart = state.AtIndex + 1 + hideCount;
                var visible = state.Email.AsSpan(visibleStart);
                visible.CopyTo(span.Slice(visibleStart));
            }
        }
    }
}