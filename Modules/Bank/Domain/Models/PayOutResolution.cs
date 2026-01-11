using OneOf;

namespace Frederikskaj2.Reservations.Bank;

[GenerateOneOf]
public partial class PayOutResolution : OneOfBase<Reconciled, Cancelled>
{
    public Reconciled Reconciled => AsT0;
    public Cancelled Cancelled => AsT1;
}
