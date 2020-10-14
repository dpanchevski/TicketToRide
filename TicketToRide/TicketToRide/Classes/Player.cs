using System;
using System.Collections.Generic;
using System.Linq;
using TicketToRide.Enums;

namespace TicketToRide.Classes
{
    public class Player
    {
        // The player name
        public string Name { get; set; }
        // The player current collection of destination cards
        public List<DestinationCard> DestinationCards { get; set; } = new List<DestinationCard>();
        // The routes the player wants to claim at any given time
        public List<BoardRoute> TargetedRoutes { get; set; } = new List<BoardRoute>();
        // All the cities this player has already connected
        public List<City> ConnectedCities { get; set; } = new List<City>();
        // The player color (red, blue, green, black or yellow)
        public PlayerColor Color { get; set; }
        // The player current collection of train cards
        public List<TrainCard> Hand { get; set; } = new List<TrainCard>();
        // When one player has 2 or less trains cars remaining, the final turn begins
        public int RemainingTrainCars { get; set; } = 48;
        // The train card colors player wants to draw
        public List<TrainColor> DesiredColors { get; set; } = new List<TrainColor>();
        // A reference to the game board
        public Board Board { get; set; }

        public void TakeTurn()
        {
            CalculateTargetedRoutes();

            // If player can claim a route they desire, they will do so immediately
            var hasClaimed = TryClaimRoute();

            if (hasClaimed)
                return;


        }

        public List<BoardRoute> CalculateTargetedRoutes(DestinationCard card)
        {
            var allRoutes = new List<BoardRoute>();

            // Step 1: Are the origin and destination already connected?
            if (Board.Routes.IsAlreadyConnected(card.Origin, card.Destination, Color))
                return allRoutes;

            Board.Routes.AlreadyCheckedCities.Clear();

            // Step 2: if the cities aren't already connected, attempt to connect them from something we've already connected
            foreach (var city in ConnectedCities)
            {
                var foundDestinationRoutes = Board.Routes.FindIdealUnclaimedRoute(city, card.Destination);

                if (foundDestinationRoutes.Any())
                {
                    allRoutes.AddRange(foundDestinationRoutes);
                    break;
                }

                var foundOriginRoutes = Board.Routes.FindIdealUnclaimedRoute(card.Origin, city);

                if (foundOriginRoutes.Any())
                {
                    allRoutes.AddRange(foundOriginRoutes);
                    break;
                }
            }

            // Step 3: If we can't connect them from something we have already connected, can we make a brand new connection?
            allRoutes = Board.Routes.FindIdealUnclaimedRoute(card.Origin, card.Destination);

            // Step 4: If there is a duplicate route in our targeted routes, remove it
            var routesToRemove = new List<BoardRoute>();

            foreach (var route in allRoutes)
            {
                var matchingRoutes = Board.Routes.Routes
                    .Where(x => x.Length == route.Length && x.IsOccupied && x.OccupingPlayerColor == Color && 
                                ((x.Origin == route.Origin && x.Destination == route.Destination) || 
                                 (x.Origin == route.Destination && x.Destination == route.Origin)));

                if (matchingRoutes.Any())
                    routesToRemove.Add(route);
            }

            foreach (var route in routesToRemove)
            {
                allRoutes.Remove(route);
            }

            return allRoutes;
        }

        // Calculate the top five common routes, then have player target the colors necessary to claim these routes
        public void CalculateTargetedRoutes()
        {
            var allRoutes = new List<BoardRoute>();

            var highestCards = DestinationCards.OrderBy(x => x.PointValue).ToList();

            foreach (var destinationCard in highestCards)
            {
                var matchingRoutes = CalculateTargetedRoutes(destinationCard);

                if (matchingRoutes.Any())
                {
                    allRoutes.AddRange(matchingRoutes);
                    break;
                }
            }

            TargetedRoutes = allRoutes
                .GroupBy(x => new { x.Origin, x.Destination, x.Color, x.Length })
                .Select(group => new
                {
                    Metric = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(x => x.Count)
                .ThenByDescending(x => x.Metric.Length)
                .Take(5)
                .Select(x => new BoardRoute(x.Metric.Origin, x.Metric.Destination, x.Metric.Color, x.Metric.Length))
                .ToList();

            DesiredColors = TargetedRoutes.Select(x => x.Color).Distinct().ToList();
        }

        public bool ClaimRoute(BoardRoute route, TrainColor color)
        {
            // If we don't have enough train cars remaining to claim this route, we cannot do so.
            if (route.Length > RemainingTrainCars)
                return false;

            // First see if we have enough cards in the hand to claim this route.
            var colorCards = Hand.Where(x => x.Color == color).ToList();

            // If we don't have enough color cards for this route...
            if (colorCards.Count < route.Length)
            {
                // ...see if we have enough Locomotive cards to fill the gap
                var gap = route.Length - colorCards.Count;
                var locomotiveCards = Hand.Where(x => x.Color == TrainColor.Locomotive).ToList();

                // Cannot claim this route
                if (locomotiveCards.Count < gap)
                    return false;

                var matchingWilds = Hand.GetMatching(TrainColor.Locomotive, gap);

                Board.DiscardPile.AddRange(matchingWilds);

                if (matchingWilds.Count != route.Length)
                {
                    var matchingColors = Hand.GetMatching(colorCards.First().Color, colorCards.Count);

                    Board.DiscardPile.AddRange(matchingColors);
                }

                // Method for placing the player colored train cars on the route
                Board.Routes.ClaimRoute(route, this.Color);

                // Add the cities to the list of connected cities
                ConnectedCities.Add(route.Origin);
                ConnectedCities.Add(route.Destination);

                ConnectedCities = ConnectedCities.Distinct().ToList();

                RemainingTrainCars -= route.Length;

                Console.WriteLine(Name + " claims the route " + route.Origin + " to " + route.Destination + "!");

                return true;
            }

            // If we only need color cards to claim this route, discard the appropriate number of them
            var neededColorCards = Hand.Where(x => x.Color == color).Take(route.Length).ToList();

            foreach (var colorCard in neededColorCards)
            {
                Hand.Remove(colorCard);
                Board.DiscardPile.Add(colorCard);
            }

            // Mark the route as claimed on the board
            Board.Routes.ClaimRoute(route, this.Color);

            RemainingTrainCars -= route.Length;

            Console.WriteLine(Name + " claims the route " + route.Origin + " to " + route.Destination + "!");

            return true;
        }

        public bool TryClaimRoute()
        {
            // How do we know if the player can claim a route they desire?
            // For each of the desired routes, loop through and see if the player has sufficient cards to claim route
            foreach (var route in TargetedRoutes)
            {
                // How many cards do we need for this rote?
                var cardCount = route.Length;

                // If the route has a color, we need to use that color to claim it
                var selectedColor = route.Color;

                // If the player is targeting a Gray route, they can use any color as long as it's not their currently desired color
                if (route.Color == TrainColor.Grey)
                {
                    // Select all cards in hand that are not in our desired color and are not locomotives
                    var matchingCard = Hand
                        .Where(x => x.Color != TrainColor.Locomotive && !DesiredColors.Contains(x.Color))
                        .GroupBy(x => x)
                        .Select(group => new
                        {
                            Matric = group,
                            Count = group.Count()
                        })
                        .OrderByDescending(x => x.Count)
                        .Select(x => x.Matric.Key)
                        .FirstOrDefault();

                    if (matchingCard == null)
                        continue;

                    selectedColor = matchingCard.Color;
                }

                // Now attempt to claim te specified route with the selected color
                return ClaimRoute(route, selectedColor);
            }

            return false;
        }
    }
}
