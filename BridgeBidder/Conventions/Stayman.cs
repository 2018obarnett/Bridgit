﻿using System.Collections.Generic;

namespace BridgeBidding
{


    public class StaymanBidder : OneNoTrumpBidder
	{
 
        public StaymanBidder(NoTrumpDescription ntd) : base(ntd) { }

        public static CallFeaturesFactory InitiateConvention(NoTrumpDescription ntd)
        {
            return new StaymanBidder(ntd).Initiate;
        }

		private IEnumerable<CallFeature> Initiate(PositionState ps)
		{
            // TODO: REALLY THINK ABOUT WHO IS RESPONSIBLE FOR SYSTEMS "ON" OVER INTERFERRENCE!!!
            // If there is a bid then it can only be 2C..
            Call call = Bid.TwoClubs;
            if (ps.RHO.Bid is Bid rhoBid)
            {
                if (call.Equals(rhoBid))
                {
                    call = Call.Double; // Stolen bid
                }
                else 
                {
                    // TODO: Make sure calling code never calls this when it cant generate rules 
                    throw new System.Exception("INVALID STATE HERE...");
                }
            }
            return new CallFeature[] {
                Convention(UserText.Stayman),
                PartnerBids(call, Answer),

                Forcing(call, NTD.RR.InviteOrBetter, Shape(Suit.Hearts, 4), Shape(Suit.Spades, 0, 4), Flat(false), ShowsSuit(Suit.Hearts)),
                Forcing(call, NTD.RR.InviteOrBetter, Shape(Suit.Spades, 4), Shape(Suit.Hearts, 0, 4), Flat(false), ShowsSuit(Suit.Spades)),
                Forcing(call, NTD.RR.InviteOrBetter, Shape(Suit.Hearts, 4), Shape(Suit.Spades, 5), ShowsSuits(Suit.Hearts, Suit.Spades)),
                Forcing(call, NTD.RR.InviteOrBetter, Shape(Suit.Hearts, 5), Shape(Suit.Spades, 4), ShowsSuits(Suit.Hearts, Suit.Spades)),
                
                Forcing(call, NTD.RR.LessThanInvite, Shape(Suit.Diamonds, 4, 5), Shape(Suit.Hearts, 4), Shape(Suit.Spades, 4)),
            };
            // TODO: Need to add rules for garbage stayman if that is on, and for 4-way transfers if that is on...
		}

       
        public IEnumerable<CallFeature> Answer(PositionState ps)
		{
            return new CallFeature[] {
                // TODO: Should we tag this with convention too???
                PartnerBids(Bid.TwoDiamonds, RespondTo2D),
				PartnerBids(Bid.TwoHearts,   p => RespondTo2M(p, Suit.Hearts)),
				PartnerBids(Bid.TwoSpades,   p => RespondTo2M(p, Suit.Spades)),

				// TODO: Deal with interferenceDefaultPartnerBids(goodThrough: Bid.Double, Explain),

				// TODO: Are these bids truly forcing?  Not if garbage stayman...
				Forcing(Bid.TwoDiamonds, Shape(Suit.Hearts, 0, 3), Shape(Suit.Spades, 0, 3), ShowsNoSuit()),
				Forcing(Bid.TwoHearts, Shape(4, 5), LongerOrEqualTo(Suit.Spades)),
                Forcing(Bid.TwoSpades, Shape(4, 5), LongerThan(Suit.Hearts))
            };
        }


        public IEnumerable<CallFeature> RespondTo2D(PositionState ps)
        {
            var bids = new List<CallFeature>
            {
                // TODO: Points 0-7 defined as garbage range...
                Signoff(Call.Pass, NTD.RR.LessThanInvite),

                PartnerBids(Bid.ThreeHearts, p => GameNewMajor(p, Suit.Hearts)),
                PartnerBids(Bid.ThreeSpades, p => GameNewMajor(p, Suit.Spades)),
                // If we have a 5 card suit and want to go to game then show that suit.
                Forcing(Bid.ThreeSpades, NTD.RR.GameOrBetter, Shape(5)),
				Forcing(Bid.ThreeHearts, NTD.RR.GameOrBetter, Shape(5)),

                // These show invitational 5/4
                PartnerBids(Bid.TwoHearts,  p => PlaceConractNewMajor(p, Suit.Hearts)),
				PartnerBids(Bid.TwoSpades,  p => PlaceConractNewMajor(p, Suit.Spades)),
				Invitational(Bid.TwoHearts, NTD.RR.InviteGame, Shape(5)),
				Invitational(Bid.TwoSpades, NTD.RR.InviteGame, Shape(5)),

                PartnerBids(Bid.TwoNoTrump, PlaceContract2NTInvite),
				Invitational(Bid.TwoNoTrump, ShowsTrump(), NTD.RR.InviteGame),

                Signoff(Bid.ThreeNoTrump, ShowsTrump(), NTD.RR.Game),

                // TODO: Point ranges - Need to figure out where these...
                Invitational(Bid.FourNoTrump, ShowsTrump(), PairPoints((30, 31)))
			};
            bids.AddRange(Gerber.InitiateConvention(ps));
            return bids;
        }

        public IEnumerable<CallFeature> RespondTo2M(PositionState _, Suit major)
        {
            return new CallFeature[]
            {

                Signoff(Call.Pass, NTD.RR.LessThanInvite),

                Signoff(new Bid(4, major), Shape(4, 5), NTD.RR.GameAsDummy, ShowsTrump()),
                PartnerBids(new Bid(3, major), p => PlaceContractMajorInvite(p, major)),
                Invitational(new Bid(3, major), Shape(4, 5), NTD.RR.InviteAsDummy, ShowsTrump()),

                PartnerBids(Bid.ThreeNoTrump, CheckOpenerSpadeGame),
                Signoff(Bid.ThreeNoTrump, NTD.RR.Game, Shape(major, 0, 3)),

				PartnerBids(Bid.TwoNoTrump, PlaceContract2NTInvite),
				Invitational(Bid.TwoNoTrump, NTD.RR.InviteGame, Shape(major, 0, 3))
			};
		}
        /*
        public IEnumerable<CallFeature> Explain(PositionState _)
        {
            return new CallFeature[] {
                DefaultPartnerBids(Bid.Double, PlaceContract), 

                // TODO: Points 0-7 defined as garbage range...
                Signoff(Call.Pass, NTD.RR.LessThanInvite),   // Garbage stayman always passes...

                // If we have a 5 card suit and want to go to game then show that suit.
                Forcing(Bid.ThreeSpades, NTD.RR.GameOrBetter, Shape(5), Partner(LastBid(Bid.TwoDiamonds))),
                Forcing(Bid.ThreeHearts, NTD.RR.GameOrBetter, Shape(5), Partner(LastBid(Bid.TwoDiamonds))),


				// These show invitational 5/4
                Invitational(Bid.TwoHearts, NTD.RR.InviteGame, Shape(5), Partner(LastBid(Bid.TwoDiamonds))),
                Invitational(Bid.TwoSpades, NTD.RR.InviteGame, Shape(5), Partner(LastBid(Bid.TwoDiamonds))),

                Invitational(Bid.TwoUnknown, ShowsTrump(), NTD.RR.InviteGame, Partner(LastBid(Bid.TwoDiamonds))),
                Invitational(Bid.TwoUnknown, ShowsTrump(), NTD.RR.InviteGame, Partner(LastBid(Bid.TwoHearts)), Shape(Suit.Hearts, 0, 3)),
                Invitational(Bid.TwoUnknown, ShowsTrump(), NTD.RR.InviteGame, Partner(LastBid(Bid.TwoSpades)), Shape(Suit.Spades, 0, 3)),


                Invitational(Bid.ThreeHearts, ShowsTrump(), NTD.RR.InviteAsDummy, Partner(LastBid(Bid.TwoHearts)), Shape(4, 5)),
                Invitational(Bid.ThreeSpades, ShowsTrump(), NTD.RR.InviteAsDummy, Partner(LastBid(Bid.TwoSpades)), Shape(4, 5)),


                // Prioritize suited contracts over 3NT bid by placing these rules first...
                Signoff(4, Suit.Hearts, ShowsTrump(), NTD.RR.GameAsDummy, Partner(LastBid(Bid.TwoHearts)), Shape(4, 5)),
                Signoff(Bid.FourSpades, ShowsTrump(), NTD.RR.GameAsDummy, Partner(LastBid(Bid.TwoSpades)), Shape(4, 5)), 

                // TODO: After changeover is done an tests are working again, change all of these rules to simply
                // Signoff(Bid.ThreeUnknown, ShowsTrump(), Points(ResponderRange.Game), Fit(Suit.Hearts, false), Fit(Suit.Spades, false)),
                Signoff(Bid.ThreeUnknown, ShowsTrump(), NTD.RR.Game, Partner(LastBid(Bid.TwoDiamonds))),
                Signoff(Bid.ThreeUnknown, ShowsTrump(), NTD.RR.Game, Partner(LastBid(Bid.TwoHearts)), Shape(Suit.Hearts, 0, 3)),
                Signoff(Bid.ThreeUnknown, ShowsTrump(), NTD.RR.Game, Partner(LastBid(Bid.TwoSpades)), Shape(Suit.Spades, 0, 3)),

            };
        }
        */


        //******************** 2nd bid of opener.

        // Bid sequence was 1NT/2C/2X/
        public IEnumerable<CallFeature> CheckOpenerSpadeGame(PositionState ps)
        {
            return new CallFeature[]
            {
                Signoff(Bid.FourSpades, Fit(), ShowsTrump()),
                Signoff(Call.Pass)
            };
        }

        public IEnumerable<CallFeature> GameNewMajor(PositionState ps, Suit major)
        {
            return new CallFeature[]
            {
                Signoff(new Bid(4, major), Fit(), ShowsTrump()),
                Signoff(Bid.ThreeNoTrump)
            };
        }

        public IEnumerable<CallFeature> PlaceConractNewMajor(PositionState ps, Suit major)
        {
            return new CallFeature[]
            {
                Signoff(Call.Pass, NTD.OR.DontAcceptInvite, Fit(major)),    // TODO: Need to use dummy points here...
                Signoff(Bid.TwoNoTrump, NTD.OR.DontAcceptInvite),
                Signoff(new Bid(4, major), Fit(), ShowsTrump(), NTD.OR.AcceptInvite),
                Signoff(Bid.ThreeNoTrump, ShowsTrump(), NTD.OR.AcceptInvite)
            };
        }

        public IEnumerable<CallFeature> PlaceContract2NTInvite(PositionState ps)
        {
            return new CallFeature[]
            {
				PartnerBids(Bid.ThreeSpades, CheckSpadeGame),
                // This is possible to know we have a fit if partner bid stayman, we respond hearts,
                Nonforcing(Bid.ThreeSpades, NTD.OR.DontAcceptInvite, Fit(), ShowsTrump()),


                Signoff(Bid.FourSpades, NTD.OR.AcceptInvite, Fit(), ShowsTrump()),

                Signoff(Bid.ThreeNoTrump, NTD.OR.AcceptInvite),

                Signoff(Call.Pass, NTD.OR.DontAcceptInvite)
			};

        }

        public IEnumerable<CallFeature> PlaceContractMajorInvite(PositionState ps, Suit major)
        {
			return new CallFeature[]
            {
				Signoff(new Bid(4, major), NTD.OR.AcceptInvite, Fit(), ShowsTrump()),
                Signoff(Call.Pass, NTD.OR.DontAcceptInvite)
            };

		}
		/*
        public IEnumerable<CallFeature> PlaceContract(PositionState _)
        {
            return new CallFeature[] {
				// These rules deal with a 5/4 invitational that we want to reject.  Leave contract in bid suit
				// if we have 3.  Otherwise put in NT
				Signoff(Bid.Pass, NTD.OR.DontAcceptInvite,  // TODO: Should check for dummy points...
                                    Fit(Suit.Hearts), Partner(LastBid(Bid.TwoHearts))),
                Signoff(Bid.Pass, NTD.OR.DontAcceptInvite,
                                    Fit(Suit.Spades), Partner(LastBid(Bid.TwoSpades))),

                Signoff(Bid.TwoUnknown, NTD.OR.DontAcceptInvite),



                Signoff(Bid.ThreeUnknown, NTD.OR.AcceptInvite, Partner(LastBid(Bid.TwoUnknown))),
                Signoff(Bid.ThreeUnknown, LastBid(Bid.TwoDiamonds), Partner(LastBid(Bid.ThreeHearts)),
                            Shape(Suit.Hearts, 2)),
                Signoff(Bid.ThreeUnknown, NTD.OR.AcceptInvite, LastBid(Bid.TwoDiamonds),
                                    Partner(LastBid(Bid.TwoHearts)), Shape(Suit.Hearts, 2)),
                Signoff(Bid.ThreeUnknown,  LastBid(Bid.TwoDiamonds), Partner(LastBid(Bid.ThreeSpades)),
                            Shape(Suit.Spades, 2)),
                Signoff(Bid.ThreeUnknown, NTD.OR.AcceptInvite, LastBid(Bid.TwoDiamonds),
                        Partner(LastBid(Bid.TwoSpades)), Shape(Suit.Spades, 2)),



                Signoff(4, Suit.Hearts, NTD.OR.AcceptInvite, Fit()),
               //TODO: I think above rule ocvers itl.. Signoff(4, Suit.Hearts, LastBid(Bid.TwoDiamonds), Partner(LastBid(Bid.ThreeHearts)), Shape(3)),


                Signoff(Bid.FourSpades, NTD.OR.AcceptInvite, Partner(LastBid(Bid.ThreeSpades)), Fit()),
                Signoff(Bid.FourSpades, NTD.OR.AcceptInvite, Fit()),
                Signoff(Bid.FourSpades, Partner(LastBid(Bid.ThreeUnknown)), Fit()),
                Signoff(Bid.FourSpades, LastBid(Bid.TwoDiamonds), Partner(LastBid(Bid.ThreeSpades)), Shape(3))
            };
        }
        */
		public IEnumerable<CallFeature> CheckSpadeGame(PositionState _)
        {
            return new CallFeature[] {
                Signoff(Bid.FourSpades, ShowsTrump(), NTD.RR.GameAsDummy, Shape(4, 5)),
                Signoff(Call.Pass)
            };
		}
	}


    //*********************************************************************************************

    // TODO: Maybe move thse 2NT stayman...
    public class Stayman2NT: Bidder
    {
        private TwoNoTrump NTB;

        public Stayman2NT(TwoNoTrump ntb)
        {
            this.NTB = ntb;
        }

        public IEnumerable<CallFeature> InitiateConvention(PositionState ps) 
        {
            // If there is a bid then it can only be 3C..
            Bid bidStayman = Bid.ThreeClubs;

            // TODO: This is no longer possible unless convert this to PositionCalls...
            Call call = ps.RightHandOpponent.GetBidHistory(0).Equals(bidStayman) ? Bid.Double : bidStayman;
            return new CallFeature[] {
                PartnerBids(call, Answer),
                Forcing(call, NTB.RespondGame, Shape(Suit.Hearts, 4), Flat(false)),
                Forcing(call, NTB.RespondGame, Shape(Suit.Spades, 4), Flat(false)),
                Forcing(call, NTB.RespondGame, Shape(Suit.Hearts, 4), Shape(Suit.Spades, 5)),
                Forcing(call, NTB.RespondGame, Shape(Suit.Hearts, 5), Shape(Suit.Spades, 4))
                // TODO: The following rule is "Garbage Stayman"
                //Forcing(Bid.TwoClubs, Points(NTLessThanInvite), Shape(Suit.Diamonds, 4, 5), Shape(Suit.Hearts, 4), Shape(Suit.Spades, 4)),
            };
        }
        public IEnumerable<CallFeature> Answer(PositionState _)
        {
            return new CallFeature[] {
                PartnerBids(ResponderRebid),

                Forcing(Bid.ThreeDiamonds, Shape(Suit.Hearts, 0, 3), Shape(Suit.Spades, 0, 3)),

                // If we are 4-4 then hearts bid before spades.  Can't be 5-5 or wouldn't be balanced.
                Forcing(Bid.ThreeHearts, Shape(4, 5), LongerOrEqualTo(Suit.Spades)),
                Forcing(Bid.ThreeSpades, Shape(4, 5), LongerThan(Suit.Hearts))
            };
        }

        public static IEnumerable<CallFeature> ResponderRebid(PositionState _)
        {
            return new CallFeature[] {
                PartnerBids(Bid.ThreeHearts, OpenerRebid),
                PartnerBids(Bid.ThreeSpades, OpenerRebid),

                Forcing(Bid.ThreeHearts, Shape(5), Partner(LastBid(Bid.ThreeDiamonds))),
                Forcing(Bid.ThreeSpades, Shape(5), Partner(LastBid(Bid.ThreeDiamonds))),

                Signoff(Bid.FourHearts, Fit()),
                Signoff(Bid.FourSpades, Fit()),
                
                Signoff(Bid.ThreeNoTrump),
            };
        }
    
        public static IEnumerable<CallFeature> OpenerRebid(PositionState _)
        { 
            return new CallFeature[] {
                Signoff(Bid.ThreeNoTrump, Fit(Suit.Hearts, false), Fit(Suit.Spades, false)),
                Signoff(Bid.FourHearts, Fit()),
                Signoff(Bid.FourSpades, Fit()),
            };
        }
    }

}
