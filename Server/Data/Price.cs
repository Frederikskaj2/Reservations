namespace Frederikskaj2.Reservations.Server.Data
{
    public class Price
    {
        public int Rent { get; set; }
        public int CleaningFee { get; set; }
        public int Deposit { get; set; }

        public int GetTotal() => Rent + CleaningFee + Deposit;

        public Price Accumulate(Price price)
        {
            Rent += price.Rent;
            CleaningFee += price.CleaningFee;
            Deposit += price.Deposit;
            return this;
        }
    }
}