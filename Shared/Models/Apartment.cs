namespace Frederikskaj2.Reservations.Shared
{
    public class Apartment
    {
        public int Id { get; set; }
        public char Letter { get; set; }
        public int Story { get; set; }
        public ApartmentSide Side { get; set; }

        public override string ToString() => $"2{Letter} {GetStory(Story)}. {GetSide(Side)}.";

        private static string GetStory(int story) => story != 0 ? story.ToString() : "st";

        private static string GetSide(ApartmentSide side) => side == ApartmentSide.Left ? "tv" : "th";
    }
}