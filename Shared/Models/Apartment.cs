namespace Frederikskaj2.Reservations.Shared
{
    public class Apartment
    {
        public int Id { get; set; }
        public char Letter { get; set; }
        public int Story { get; set; }
        public ApartmentSide Side { get; set; }

        public override string ToString() => $"2{Letter}{GetStory(Story)}{GetSide(Side)}";

        private static string GetStory(int story)
            => story switch
            {
                var above0 when above0 > 0 => $", {above0}.",
                0 => ", st.",
                _ => string.Empty
            };

        private static string GetSide(ApartmentSide side)
            => side switch
            {
                ApartmentSide.Left => " tv",
                ApartmentSide.Right => " th",
                _ => string.Empty
            };
    }
}