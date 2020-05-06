using System;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class PayOutModel : EmailWithNameModel
    {
        public PayOutModel(string from, Uri fromUrl, string name, int amount)
            : base(from, fromUrl, name) => Amount = amount;

        public int Amount { get; }
    }
}