﻿using System;
using System.Diagnostics;
using System.Linq;

//using static BridgeBidding.BidRule;

namespace BridgeBidding
{



    public abstract class Bidder
	{
		public static BidRule PartnerBids(BidRulesFactory brf)
		{
			return PartnerBids(BidChoices.FromBidRulesFactory(brf));
		}

        public static BidRule PartnerBids(BidChoicesFactory bcf)
        {
            return _PartnerBids(null, bcf, new StaticConstraint[0]);
        }

	

		public static BidRule PartnerBids(Call call, BidRulesFactory brf, params StaticConstraint[] constraints)
		{
			Debug.Assert(call != null);
			return _PartnerBids(call, BidChoices.FromBidRulesFactory(brf), constraints);
		}
		public static BidRule PartnerBids(Call call, BidChoicesFactory choicesFactory)
		{
			Debug.Assert(call != null);
			return _PartnerBids(call, choicesFactory, new StaticConstraint[0]);
		}

		private static BidRule _PartnerBids(Call call,
											BidChoicesFactory choicesFactory, 
											params StaticConstraint[] constraints)
		{
			return new PartnerBidRule(call, choicesFactory, constraints);
		}

		public static BidRule Forcing(Call call, params Constraint[] constraints)
		{
			return Rule(call, BidForce.Forcing1Round, constraints);
		}

		public static BidRule Semiforcing(Call call, params Constraint[] constraints)
		{
			// TODO: What do do about semi-forcing?  
			return Rule(call, BidForce.Nonforcing, constraints);
		}

		public static BidRule ForcingToGame(Call call, params Constraint[] constraints)
		{
			return Rule(call, BidForce.ForcingToGame, constraints);
		}

		// TODO: Need a non-forcing BidMessage...


		public static BidRule Nonforcing(Call call, params Constraint[] constraints)
		{
			return Rule(call, BidForce.Nonforcing, constraints);
		}



		public static BidRule Invitational(Call call, params Constraint[] constraints)
		{
			return Rule(call, BidForce.Invitational, constraints);
		}
	

		public static BidRule Signoff(Call call, params Constraint[] constraints)
		{
			return Rule(call, BidForce.Signoff, constraints);
		}


		public static BidRule Rule(Call call, BidForce force, params Constraint[] constraints)
		{
			return new BidRule(call, force, constraints);
		}

		public static BidRule Alert(Call call, string text, params StaticConstraint[] constraints)
		{
			return new BidAnnotation(call, BidAnnotation.AnnotationType.Alert, text, constraints);
		}
		public static BidRule Announce(Call call, string text, params StaticConstraint[] constraints)
		{
			return new BidAnnotation(call, BidAnnotation.AnnotationType.Announce, text, constraints);
		}

		// ************************************************************ STATIC CONSTRAINTS ***

		public static StaticConstraint Seat(params int[] seats)
		{
			return new StaticConstraint((call, ps) => seats.Contains(ps.Seat));
		}
		public static StaticConstraint LastBid(Call call)
		{
			return new BidHistory(0, call, true);
		}
		public static StaticConstraint LastBid(int level, Suit suit, bool desired = true)
		{
			return new BidHistory(0, new Bid(level, suit), desired);
		}
		public static StaticConstraint LastBid(int level, Strain strain, bool desired = true)
		{
			return new BidHistory(0, new Bid(level, strain), desired);
		}
		public static StaticConstraint OpeningBid(Bid bid)
		{
			return new StaticConstraint((call, ps) => ps.BiddingState.OpeningBid == bid);
		}


		public static StaticConstraint Rebid(bool desired = true)
		{
			return new BidHistory(0, null, desired);
		}


		public static StaticConstraint Jump(params int[] jumpLevels)
		{
			return new JumpBid(jumpLevels);
		}

		public static StaticConstraint IsReverse(bool desiredValue = true)
		{
			return new StaticConstraint((call, ps) => ps.IsOpenerReverseBid(call));
		}

		public static StaticConstraint ForcedToBid()
		{
			return new StaticConstraint((call, ps) => ps.ForcedToBid);
		}

		public static StaticConstraint Not(StaticConstraint c)
		{
			return new StaticConstraint((call, ps) => !c.Conforms(call, ps));
		}

		public static StaticConstraint Partner(Constraint constraint)
		{
			return new PositionProxy(PositionProxy.RelativePosition.Partner, constraint);
		}

		public static StaticConstraint PassEndsAuction()
		{
			return new StaticConstraint((call, ps) => ps.BiddingState.Contract.PassEndsAuction);
		}

		public static StaticConstraint BidAvailable(int level, Suit suit)
		{ 
			return new StaticConstraint((call, ps) => ps.IsValidNextCall(new Bid(level, suit)));
	 	}

		public static StaticConstraint OppsContract()
		{ 
			return new StaticConstraint((call, ps) => ps.IsOpponentsContract); 
		}


		public static StaticConstraint AgreedStrain(params Strain[] strains)
		{
			return new AgreedStrain(strains);
		}

		public static StaticConstraint ContractIsAgreedStrain()
		{
			return new StaticConstraint((call, ps) =>  
					(ps.BiddingState.Contract.Bid is Bid contractBid &&
                    ps.BiddingState.Contract.IsOurs(ps) && 
                    contractBid.Strain == ps.PairState.Agreements.AgreedStrain));
		}


		// ************************************  DYNAMIC CONSTRAINTS ***
		public static DynamicConstraint PassIn4thSeat()
		{
			return new PassIn4thSeat();
		}
		public static DynamicConstraint HighCardPoints(int min, int max)
		{
			 return new ShowsPoints(null, min, max, HasPoints.PointType.HighCard); 
		
		}
		public static DynamicConstraint HighCardPoints((int min, int max) range)
		{
			return HighCardPoints(range.min, range.max);
		}

		public static DynamicConstraint Points(int min, int max)
		{
			return new ShowsPoints(null, min, max, HasPoints.PointType.Starting);
		}

		public static DynamicConstraint Points((int min, int max) range) {
			return Points(range.min, range.max); }

		public static DynamicConstraint DummyPoints(int min, int max)
		{
			// TODO: Rename this??? SuitPoints???  
			return new ShowsPoints(null, min, max, HasPoints.PointType.Dummy);
		}
		public static Constraint DummyPoints((int min, int max) range)
		{
			return DummyPoints(range.min, range.max); 
		}

		public static Constraint DummyPoints(Suit? trumpSuit, (int min, int max) range)
		{
			// TODO: Rename this too????  SuitPoints
			return new ShowsPoints(trumpSuit, range.min, range.max, HasPoints.PointType.Dummy);
		}

		public static Constraint Shape(int min) { return new ShowsShape(null, min, min); }
		public static Constraint Shape(Suit suit, int count) { return new ShowsShape(suit, count, count); }
		public static Constraint Shape(int min, int max) { return new ShowsShape(null, min, max); }
		public static Constraint Shape(Suit suit, int min, int max) { return new ShowsShape(suit, min, max); }
		public static Constraint Balanced(bool desired = true) { return new ShowsBalanced(desired); }
		public static Constraint Flat(bool desired = true) { return new ShowsFlat(desired); }



		public static StaticConstraint RHO(Constraint constraint)
		{
			return new PositionProxy(PositionProxy.RelativePosition.RightHandOpponent, constraint);
		}

		public static Constraint HasShape(int count)
		{
			return HasShape(count, count);
		}

		public static Constraint HasMinShape(int count)
		{
			return HasMinShape(null, count);
		}

		public static Constraint HasMinShape(Suit? suit, int count)
		{
			return new HasMinShape(suit, count);
		}


		public static Constraint HasShape(int min, int max)
		{
			return new HasShape(null, min, max);
		}

		public static Constraint Quality(SuitQuality min, SuitQuality max) {
			return new ShowsQuality(null, min, max);
		}
		public static Constraint Quality(Suit suit, SuitQuality min, SuitQuality max)
		{ return new ShowsQuality(suit, min, max); }

		public static Constraint And(params Constraint[] constraints)
		{
			return new ConstraintGroup(constraints);
		}

        public static Constraint ExcellentSuit(Suit? suit = null)
        { return new ShowsQuality(suit, SuitQuality.Excellent, SuitQuality.Solid); }


        public static Constraint GoodSuit(Suit? suit = null)
		{ return new ShowsQuality(suit, SuitQuality.Good, SuitQuality.Solid); }

		public static Constraint DecentSuit(Suit? suit = null)
		{ return new ShowsQuality(suit, SuitQuality.Decent, SuitQuality.Solid); }

		public static Constraint Better(Suit better, Suit worse) { return new ShowsBetterSuit(better, worse, worse, false); }

		public static Constraint BetterOrEqual(Suit better, Suit worse) { return new ShowsBetterSuit(better, worse, better, false); }

		public static Constraint BetterThan(Suit worse) { return new ShowsBetterSuit(null, worse, worse, false); }

		public static Constraint BetterOrEqualTo(Suit worse) { return new ShowsBetterSuit(null, worse, null, false); }


		public static Constraint LongerThan(Suit shorter) { return new ShowsBetterSuit(null, shorter, shorter, true); }

		public static Constraint LongerOrEqualTo(Suit shorter) { return new ShowsBetterSuit(null, shorter, null, true); }
		public static Constraint Longer(Suit longer, Suit shorter) { return new ShowsBetterSuit(longer, shorter, shorter, true); }

		public static Constraint LongerOrEqual(Suit longer, Suit shorter) { return new ShowsBetterSuit(longer, shorter, longer, true); }

		public static DynamicConstraint LongestSuit(Suit? suit = null)
		{
			return new ShowsLongestSuit(suit);
		}

		public static Constraint DummyPoints(Suit trumpSuit, (int min, int max) range)
		{
			return new ShowsPoints(trumpSuit, range.min, range.max, HasPoints.PointType.Dummy);
		}

		public static Constraint LongestMajor(int max)
		{
			return And(Shape(Suit.Hearts, 0, max), Shape(Suit.Spades, 0, max));
		}




		

		public static Constraint ShowsTrump(Strain? trumpStrain = null)
		{
			return new ShowsTrump(trumpStrain);
		}

		public static Constraint ShowsTrump(Suit? trumpSuit)
		{
			if (trumpSuit == null) { return new ShowsTrump(null); }
			return new ShowsTrump(Call.SuitToStrain(trumpSuit));
		}


		public static Constraint AgreedAnySuit()
		{
			return AgreedStrain(Strain.Clubs, Strain.Diamonds, Strain.Hearts, Strain.Spades);
		}




		public static Constraint Aces(params int[] count)
		{
			return new KeyCards(null, null, count);
		}

		public static Constraint PairAces(params int[] count)
		{
			return new PairKeyCards(null, null, count);
		}

		public static Constraint Kings(params int[] count)
		{
			return new Kings(count);
		}

		public static Constraint PairKings(params int[] count)
		{
			return new PairKings(count);
		}

		public static Constraint CueBid(bool desiredValue = true)
		{
			return CueBid(null, desiredValue);
		}

		public static Constraint CueBid(Suit? suit, bool desiredValue = true)
		{
			return new CueBid(suit, desiredValue);
		}

		// Perhaps rename this.  Perhaps move this to takeout...
		public static Constraint TakeoutSuit(Suit? suit = null)
		{
			return And(new TakeoutSuit(suit), CueBid(false));
		}


		// TODO: Needs to be a version of this that does not check for explicitly
		// "shown" to make stayman work.

		public static Constraint Fit(int count = 8, Suit? suit = null, bool desiredValue = true)
		{
			return And(Partner(HasShownSuit(suit, eitherPartner: true)), new PairShowsMinShape(suit, count, desiredValue));
		}

		public static Constraint Fit(Suit suit, bool desiredValue = true)
		{
			return Fit(8, suit, desiredValue);
		}

		public static Constraint Fit(bool desiredValue)
		{
			return Fit(8, null, desiredValue);
		}

		public static Constraint Reverse(bool desiredValue = true)
		{
			return And(IsReverse(desiredValue), new ShowsReverseShape());
		}

		public static Constraint PairPoints((int Min, int Max) range)
		{
			return PairPoints(null, range);
		}

		public static Constraint PairPoints(Suit? suit, (int Min, int Max) range)
		{
			return new PairShowsPoints(suit, range.Min, range.Max);
		}

		public static Constraint AgreedStrainPoints((int Min, int Max) range)
		{
			return new PairShowsPoints(range.Min, range.Max);
		}


		public static Constraint OppsStopped(bool desired = true)
		{
			// TODO: THIS SHOULD REALLY SHOW OPPS STOPPED TOO......
			return new ShowsOppsStopped(desired);
		}




		public static Constraint RuleOf17(Suit? suit = null)
		{
			return new RuleOf17(suit);
		}

		public static Constraint Break(bool isStatic, string name)
		{	
			if (isStatic)
			{
				return new StaticBreak(name);
			}
			return new Break(name);
		}


		public static Constraint ShowsSuit(Suit suit)
		{
			return new ShowsSuit(true, suit);
		}
		public static Constraint ShowsSuits(params Suit[] suits)
		{
			return new ShowsSuit(false, suits);
		}



		public static Constraint HasShownSuit(Suit? suit = null, bool eitherPartner = false)
		{
			return new HasShownSuit(suit, eitherPartner);
		}

		public static Constraint ShowsSuit()
		{
			return new ShowsSuit(true, null);
		}
		public static Constraint ShowsNoSuit()
		{
			return new ShowsSuit(false, null);
		}

		public static DynamicConstraint BetterMinor(Suit? suit = null)
		{
			return new BetterMinor(suit);
		}

		public static DynamicConstraint RuleOf9()
		{
			return new RuleOf9();
		}




		// THE FOLLOWING CONSTRAINTS ARE GROUPS OF CONSTRAINTS
        public static Constraint RaisePartner(Suit? suit = null, int raise = 1, int fit = 8)
        {
            return And(Fit(fit, suit), Jump(raise - 1), ShowsTrump(suit));
        }
        public static Constraint RaisePartner(int level)
        {
            return RaisePartner(null, level);
        }
		public static Constraint RaisePartner(Suit suit)
		{
			return RaisePartner(suit, 1);
		}


    }
};

