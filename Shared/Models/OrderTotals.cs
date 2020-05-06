namespace Frederikskaj2.Reservations.Shared
{
    public class OrderTotals
    {
        public int Price { get; set; }
        public int FromOtherOrders { get; set; }
        public int PayIn { get; set; }
        public int SettledDeposits { get; set; }
        public int CancellationFee { get; set; }
        public int Damages { get; set; }
        public int ToOtherOrders { get; set; }
        public int PayOut { get; set; }

        public int GetBalance() => -Price + + FromOtherOrders + PayIn + SettledDeposits - CancellationFee - Damages - ToOtherOrders - PayOut;
        public bool IsPaid() => FromOtherOrders + PayIn > 0;
    }
}