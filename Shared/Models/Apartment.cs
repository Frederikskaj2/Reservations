namespace Frederikskaj2.Reservations.Shared
{
    public class Apartment
    {
        public Apartment(int id, char letter, int story, ApartmentSide side)
        {
            Id = id;
            Letter = letter;
            Story = story;
            Side = side;
        }

        public int Id { get; }
        public char Letter { get; }
        public int Story { get; }
        public ApartmentSide Side { get; }

        public override string ToString() => $"2{Letter} {GetStory(Story)}. {GetSide(Side)}.";

        private static string GetStory(int story) => story != 0 ? story.ToString() : "st";

        private static string GetSide(ApartmentSide side) => side == ApartmentSide.Left ? "tv" : "th";
    }
}