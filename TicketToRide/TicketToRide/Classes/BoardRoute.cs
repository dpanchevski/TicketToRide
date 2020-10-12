using TicketToRide.Enums;

namespace TicketToRide.Classes
{
    public class BoardRoute
    {
        public City Origin { get; set; }
        public City Destination { get; set; }
        public TrainColor Color { get; set; }
        public int Length { get; set; }
        public bool IsOccupied { get; set; }
        public int PointValue
        {
            get
            {
                return Length switch
                {
                    1 => 1,
                    2 => 2,
                    3 => 4,
                    4 => 7,
                    5 => 10,
                    6 => 15,
                    _ => 1 // Don't expect this to ever be used
                };
            }
        }
        public PlayerColor? OccupingPlayerColor { get; set; } // If not null, is the color of the player who has claimed this route

        public BoardRoute(City origin, City destination, TrainColor color, int length)
        {
            Origin = origin;
            Destination = destination;
            Color = color;
            Length = length;
        }
    }
}
