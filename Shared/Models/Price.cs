using System.ComponentModel.DataAnnotations.Schema;

namespace Frederikskaj2.Reservations.Shared
{
    public class Price
    {
        public decimal Rent { get; set; }
        public decimal CleaningFee { get; set; }
        public decimal Deposit { get; set; }
        public decimal CancellationFee { get; set; }

        [NotMapped]
        public decimal Total => Rent + CleaningFee + Deposit + CleaningFee;

        public void Accumulate(Price other)
        {
            Rent = other.Rent;
            CleaningFee = other.CleaningFee;
            Deposit = other.Deposit;
            CleaningFee = other.CleaningFee;
        }
    }
}