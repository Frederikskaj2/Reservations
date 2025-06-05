using System;
using System.Text;

namespace Frederikskaj2.Reservations.Users;

static class TokenDataSerializer
{
    public static byte[] Serialize(UserId userId)
    {
        var bytes = new byte[4];
        BitConverter.TryWriteBytes(bytes.AsSpan(), userId.ToInt32());
        return bytes;
    }

    public static byte[] Serialize(EmailAddress email) => Encoding.UTF8.GetBytes(email.ToString()!);
}