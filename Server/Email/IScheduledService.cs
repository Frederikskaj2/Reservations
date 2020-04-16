using System;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.Email
{
    internal interface IScheduledService
    {
        TimeSpan Interval { get; }
        Task DoWork();
    }
}