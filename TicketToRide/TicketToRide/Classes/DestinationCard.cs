using TicketToRide.Enums;

namespace TicketToRide.Classes
{
    public class DestinationCard
    {
        public City Origin { get; set; }
        public City Destination { get; set; }
        public int PointValue { get; set; }

        public DestinationCard(City origin, City destination, int points)
        {
            Origin = origin;
            Destination = destination;
            PointValue = points;
        }
    }
}
