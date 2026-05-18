using System.Security.Cryptography;

namespace Frederikskaj2.Reservations.Orders;

static class EntryCodeFunctions
{
    public static EntryCode CreateEntryCode()
    {
        while (true)
        {
            var entryCode = RandomNumberGenerator.GetString("123456789", 6);
            if (EntryCode.IsValid(entryCode))
                return EntryCode.FromString(entryCode);
        }
    }
}
