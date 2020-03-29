namespace Frederikskaj2.Reservations.Shared
{
    public enum TransactionType
    {
        None,
        Order,
        Deposit,
        OrderCancellation,
        DepositCancellation,
        CancellationFee,
        SettlementDeposit,
        SettlementDamages,
        PayIn,
        PayOut
    }
}
