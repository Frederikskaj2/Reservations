using OneOf;

namespace Frederikskaj2.Reservations.Orders;

[GenerateOneOf]
public partial class OrderSpecifics : OneOfBase<Resident, Owner>
{
    public Resident Resident => AsT0;
    public Owner Owner => AsT1;
}
