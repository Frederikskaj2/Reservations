using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class HistoryOrder
    {
        public HistoryOrder(int id, Instant createdTimestamp, Instant completedTimestamp, List<Resource>? resources, decimal price)
        {
            Id = id;
            CreatedTimestamp = createdTimestamp;
            CompletedTimestamp = completedTimestamp;
            Resources = resources;
            Price = price;
        }

        public int Id { get; }
        public Instant CreatedTimestamp { get; }
        public Instant CompletedTimestamp { get; }
        public List<Resource>? Resources { get; }
        public decimal Price { get; }
    }
}