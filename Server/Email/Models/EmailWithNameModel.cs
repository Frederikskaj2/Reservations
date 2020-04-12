using System;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class EmailWithNameModel : EmailModel
    {
        public EmailWithNameModel(string from, Uri fromUrl, string name) : base(from, fromUrl)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));

            Name = name;
        }

        public string Name { get; }
    }
}