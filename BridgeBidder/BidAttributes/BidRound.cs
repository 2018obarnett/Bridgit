﻿using System.Diagnostics;

namespace BridgeBidding
{
    public class BidRound : StaticConstraint
    {
        private int _bidRound;
        public BidRound(int round)
        {
            Debug.Assert(round > 0);
            this._bidRound = round;
        }

        public override bool Conforms(Call call, PositionState ps)
        {
            return ps.BidRound == _bidRound;
        }
    }
}
