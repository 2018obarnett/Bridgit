using BridgeBidding;
using bridgit;
using System.Collections.Generic;

namespace BridgeBidderLibrary
{
    public class GameFunctions
    {

        // counts should be in order N, E, S, W for each of them the 5 values ace, king, queen, jack ,ten
        public string RunWithString(string deal, bool verbose=false, List<double> newBidValues = null )
        {
            var vulnerableOption = Vulnerable.None;

            Game game = Game.Parse(deal, vulnerableOption.ToString());

            if (newBidValues != null && newBidValues.Count == 20)
            {
                int index = 0;
                foreach (KeyValuePair<Direction, Hand> entry in game.Deal)
                {
                    entry.Value.AcePoints = newBidValues[index * 5];
                    entry.Value.KingPoints = newBidValues[index * 5 + 1];
                    entry.Value.QueenPoints = newBidValues[index * 5 + 2];
                    entry.Value.JackPoints = newBidValues[index * 5 + 3];
                    entry.Value.TenPoints = newBidValues[index * 5 + 4];
                    index++;
                }
            }

            string bids = InterractiveApp.BidDealReturnState(game, verbose);

            return (bids);
        }
    }
}
