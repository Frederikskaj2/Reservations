using System;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class KeyCodesModel : EmailWithNameModel
    {
        public KeyCodesModel(
            string from, Uri fromUrl, string name, IReadOnlyDictionary<int, Resource> resources,
            IEnumerable<WeeklyKeyCodes> weeklyKeyCodes)
            : base(from, fromUrl, name)
        {
            Resources = resources ?? throw new ArgumentNullException(nameof(resources));
            WeeklyKeyCodes = weeklyKeyCodes ?? throw new ArgumentNullException(nameof(weeklyKeyCodes));
        }

        public IReadOnlyDictionary<int, Resource> Resources { get; }
        public IEnumerable<WeeklyKeyCodes> WeeklyKeyCodes { get; }
    }
}