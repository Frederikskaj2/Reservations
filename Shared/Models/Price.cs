﻿namespace Frederikskaj2.Reservations.Shared
{
    public class Price
    {
        public decimal Rent { get; set; }
        public decimal CleaningFee { get; set; }
        public decimal Deposit { get; set; }
        public decimal CancellationFee { get; set; }

        public decimal GetTotal() => Rent + CleaningFee + Deposit + CancellationFee;

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