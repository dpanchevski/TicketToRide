using System;
using System.Collections.Generic;
using System.Linq;
using TicketToRide.Classes;

namespace TicketToRide.Extensions
{
    public static class CardExtensions
    {
        // The following two methods can be implemented with Stack<T> or Queue<T>, but why not as a list :D (change if you have time)
        public static List<TrainCard> Pop(this List<TrainCard> cards, int count)
        {
            List<TrainCard> returnCards = new List<TrainCard>();
            for (int i = 0; i < count; i++)
            {
                var selectedCard = cards[0];
                cards.RemoveAt(0);
                returnCards.Add(selectedCard);
            }

            return returnCards;
        }

        public static List<DestinationCard> Pop(this List<DestinationCard> cards, int count)
        {
            List<DestinationCard> returnCards = new List<DestinationCard>();
            for (int i = 0; i < count; i++)
            {
                var selectedCard = cards[0];
                cards.RemoveAt(0);
                returnCards.Add(selectedCard);
                if(!cards.Any()) break;
            }

            return returnCards;
        }

        // Modern method of the Fisher-Yates algorithm - O(n)
        public static List<TrainCard> Shuffle(this List<TrainCard> cards)
        {
            Random r = new Random();

            // Step 1: For each unshuffled item in the collection
            for (int n = cards.Count; n > 0; --n)
            {
                // Step 2: Randomly pic an item which has not been shuffled
                int k = r.Next(n + 1);

                // Step 3: Swap the selected item with th last "unstruck" letter in the collection
                TrainCard temp = cards[n];
                cards[n] = cards[k];
                cards[k] = temp;
            }

            return cards;
        }

        public static List<DestinationCard> Shuffle(this List<DestinationCard> cards)
        {
            Random r = new Random();

            // Step 1: For each unshuffled item in the collection
            for (int n = cards.Count; n > 0; --n)
            {
                // Step 2: Randomly pic an item which has not been shuffled
                int k = r.Next(n + 1);

                // Step 3: Swap the selected item with th last "unstruck" letter in the collection
                DestinationCard temp = cards[n];
                cards[n] = cards[k];
                cards[k] = temp;
            }

            return cards;
        }

        // Original method of the Fisher-Yates algorithm - O(n^2)
        //public static List<DestinationCard> ShuffleOriginalFY(this List<DestinationCard> unshuffledDestinationCardsCards)
        //{
        //    Random r = new Random();
        //    List<DestinationCard> shuffledDestinationCards = new List<DestinationCard>();

        //    // Step 1: For each remaining unshuffled letter
        //    for (int n = unshuffledDestinationCardsCards.Count; n > 0; n--)
        //    {
        //        // Step 2: Randomly select one of the remaining unshuffled letters
        //        int k = r.Next(n);

        //        // Step 3: Place the selected letter in the shuffled collection
        //        DestinationCard temp = unshuffledDestinationCardsCards[k];
        //        shuffledDestinationCards.Add(temp);

        //        //Step 4: Remove the selected letter from the unshuffled collection
        //        unshuffledDestinationCardsCards.RemoveAt(k);
        //    }

        //    return shuffledDestinationCards;
        //}
    }
}
