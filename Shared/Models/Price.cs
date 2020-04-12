using System;

namespace Frederikskaj2.Reservations.Shared
{
    public class Price
    {
        public int Rent { get; set; }
        public int CleaningFee { get; set; }
        public int Deposit { get; set; }

        public int GetTotal() => Rent + CleaningFee + Deposit;

        public Price Accumulate(Price price)
        {
            if (price is null)
                throw new ArgumentNullException(nameof(price));

            Rent += price.Rent;
            CleaningFee += price.CleaningFee;
            Deposit += price.Deposit;
            return this;
        }
    }
}