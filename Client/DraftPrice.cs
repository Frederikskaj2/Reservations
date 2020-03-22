using Frederikskaj2.Reservations.Shared;

namespace Frederikskaj2.Reservations.Client
{
    public class DraftPrice
    {
        public decimal Rent { get; set; }
        public decimal CleaningFee { get; set; }
        public decimal Deposit { get; set; }
        public decimal CancellationFee { get; set; }

        public decimal Total => Rent + CleaningFee + Deposit + CancellationFee;

        public DraftPrice Accumulate(Price price)
        {
            Rent += price.Rent;
            CleaningFee += price.CleaningFee;
            Deposit += price.Deposit;
            CancellationFee += price.CancellationFee;
            return this;
        }

        public static DraftPrice FromPrice(Price price)
            => new DraftPrice
            {
                Rent = price.Rent,
                CleaningFee = price.Rent,
                Deposit = price.Deposit,
                CancellationFee = price.CancellationFee
            };
    }
}