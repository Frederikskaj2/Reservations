using System;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class PostingsModel : EmailWithNameModel
    {
        public PostingsModel(string from, Uri fromUrl, string name, IEnumerable<Posting> postings)
            : base(from, fromUrl, name)
            => Postings = postings ?? throw new ArgumentNullException(nameof(postings));

        public IEnumerable<Posting> Postings { get; }
    }
}