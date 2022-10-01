using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Importer.Input;

public class Apartment
{
    public int Id { get; set; }
    public char Letter { get; set; }
    public int Story { get; set; }
    public ApartmentSide Side { get; set; }
}