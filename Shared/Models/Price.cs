namespace Frederikskaj2.Reservations.Shared
{
    public class Price
    {
        public int Rent { get; set; }
        public int CleaningFee { get; set; }
        public int Deposit { get; set; }
        public int CancellationFee { get; set; }

        public int GetTotal() => Rent + CleaningFee + Deposit + CancellationFee;

        public Price Accumulate(Price price)
        {
            Rent += price.Rent;
            CleaningFee += price.CleaningFee;
            Deposit += price.Deposit;
            CancellationFee += price.CancellationFee;
            return this;
        }
    }
}