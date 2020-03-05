namespace Frederikskaj2.Reservations.Shared
{
    public class PriceOptions
    {
        public decimal LowRentPerDay { get; set; }
        public decimal HighRentPerDay { get; set; }
        public decimal CleaningFee { get; set; }
        public decimal LowDeposit { get; set; }
        public decimal HighDeposit { get; set; }
    }
}