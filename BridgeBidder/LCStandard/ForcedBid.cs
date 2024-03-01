﻿using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace BridgeBidding
{
    public class ForcedBid: Bidder
	{
		// TODO: Where should we check position state to see if the forced bids are necessary?  Here?  
		public static IEnumerable<CallFeature> Bids(PositionState ps)
		{
			var bids = new List<CallFeature>();
			if (ps.ForcedToBid)
			{
				bids.AddRange(new CallFeature[] {
					Nonforcing(Bid._2C, Fit()),
					Nonforcing(Bid._2D, Fit()),
					Nonforcing(Bid._2H, Fit()),
					Nonforcing(Bid._2S, Fit()),

					Nonforcing(Bid._3C, NonJump, Fit()),
					Nonforcing(Bid._3D, NonJump, Fit()),
					Nonforcing(Bid._3H, NonJump, Fit()),
					Nonforcing(Bid._3S, NonJump, Fit()),

					Nonforcing(Bid._4C, NonJump, Fit()),
					Nonforcing(Bid._4D, NonJump, Fit()),
					Nonforcing(Bid._4H, NonJump, Fit()),
					Nonforcing(Bid._4S, NonJump, Fit()), 

					// Now the worst possible cases.  NT if no 7-card fit
					Nonforcing(Bid._1NT),
					Nonforcing(Bid._2NT, NonJump),
					Nonforcing(Bid._3NT, NonJump)
				});
			};
			return bids;
		}
	}
}
