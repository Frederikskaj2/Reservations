namespace Frederikskaj2.Reservations.Users;

public record Apartment(ApartmentId ApartmentId, char Letter, int? Story, ApartmentSide Side)
{
    public OccupancyType OccupancyType =>
        Letter switch
        {
            'A' or 'B' or 'C' or 'F' or 'G' or 'M' or 'N' => OccupancyType.Tenant,
            'D' or 'E' or 'H' or 'K' or 'L' or 'P' or 'R' => OccupancyType.Owner,
            'V' or 'W' => OccupancyType.Houseboat,
            _ => OccupancyType.Unknown,
        };

    public override string ToString() => this != Deleted ? $"2{Letter}{GetStory(Story)}{GetSide(Side)}" : "2";

    public static readonly Apartment Deleted = new(ApartmentId.FromInt32(0), Letter: '\0', Story: null, ApartmentSide.None);

    static string GetStory(int? story) => story switch
    {
        > 0 => $", {story}.",
        0 => ", st.",
        _ => "",
    };

    static string GetSide(ApartmentSide side) => side switch
    {
        ApartmentSide.Left => " tv.",
        ApartmentSide.Right => " th.",
        _ => "",
    };
}
