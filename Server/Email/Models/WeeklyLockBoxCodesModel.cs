using System;
using System.Collections.Generic;
using Frederikskaj2.Reservations.Shared;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class WeeklyLockBoxCodesModel : EmailWithNameModel
    {
        public WeeklyLockBoxCodesModel(
            string from, Uri fromUrl, string name, IReadOnlyDictionary<int, Resource> resources,
            IEnumerable<WeeklyLockBoxCodes> weeklyLockBoxCodes)
            : base(from, fromUrl, name)
        {
            Resources = resources ?? throw new ArgumentNullException(nameof(resources));
            WeeklyLockBoxCodes = weeklyLockBoxCodes ?? throw new ArgumentNullException(nameof(weeklyLockBoxCodes));
        }

        public IReadOnlyDictionary<int, Resource> Resources { get; }
        public IEnumerable<WeeklyLockBoxCodes> WeeklyLockBoxCodes { get; }
    }
}