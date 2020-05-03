namespace Frederikskaj2.Reservations.Shared
{
    public enum TransactionType
    {
        None,
        Order,
        Deposit,
        BalanceIn,
        PayIn,
        OrderCancellation,
        DepositCancellation,
        CancellationFee,
        SettlementDamages,
        SettlementDeposit,
        BalanceOut,
        PayOut,
    }
}
