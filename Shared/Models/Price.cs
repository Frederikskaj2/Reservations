namespace Frederikskaj2.Reservations.Shared
{
    public class Price
    {
        public Price(decimal rent, decimal cleaningFee, decimal deposit, decimal cancellationFee)
        {
            Rent = rent;
            CleaningFee = cleaningFee;
            Deposit = deposit;
            CancellationFee = cancellationFee;
        }

        public decimal Rent { get; }
        public decimal CleaningFee { get; }
        public decimal Deposit { get; }
        public decimal CancellationFee { get; }
        public decimal Total => Rent + CleaningFee + Deposit + CleaningFee;
    }
}