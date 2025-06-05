namespace Frederikskaj2.Reservations.Users;

public record Apartment(ApartmentId ApartmentId, char Letter, int? Story, ApartmentSide Side)
{
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
