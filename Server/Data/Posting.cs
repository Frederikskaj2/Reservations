using Frederikskaj2.Reservations.Shared;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class Posting
    {
        public int Id { get; set; }
        public LocalDate Date { get; set; }
        public PostingType Type {get; set; }
        public int? UserId { get; set; }
        public virtual User? User { get; set; }
        public int? OrderId { get; set; }
        public string? FullName { get; set; }
        public int Income { get; set;}
        public int Bank { get; set;}
        public int Deposits { get; set;}
        public int IncomeBalance { get; set;}
        public int BankBalance { get; set;}
        public int DepositsBalance { get; set;}
    }
}
