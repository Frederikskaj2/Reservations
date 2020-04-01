namespace Frederikskaj2.Reservations.Shared
{
    public class OrderTotals
    {
        public Price? Price { get; set; }
        public int PayIn { get; set; }
        public int CancellationFee { get; set; }
        public int Damages { get; set; }
        public int PayOut { get; set; }
        public int Balance { get; set; }
    }
}