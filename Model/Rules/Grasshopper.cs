using System;
using System.Collections.Generic;

namespace HiveMind.Model.Rules
{
	public class Grasshopper : Bug
	{
		#region implemented abstract members of Bug

		public override bool IsFreeToMove (HiveMind.Model.Token token, HiveMind.Model.Board board)
		{
			// A grasshopper can always move, due to the One Hive rule which ensures that there always is a neighbor to jump over
			return true;
		}

		public override List<Hex> getTargetHexes (Token token, Board board)
		{
			// Find neighbor tokens and travel in their direction until we hit a empty space
			List<Hex> targets = new List<Hex>();
			List<Token> neighbors = board.GetNeighborTokens(token.Hex);
			foreach (Token direction in neighbors) {
				targets.Add(getFirstEmptyHexInDirection(token.Hex, direction.Hex, board));
			}
		
			return targets;
		}

		public override bool canMimic ()
		{
			return false;
		}

		public override bool canMoveOthers ()
		{
			return false;
		}

		#endregion

		private Hex getFirstEmptyHexInDirection(Hex from, Hex to, Board board)
		{
			int startQ = from.Q;
			int startR = from.R;
		
			int directionQ = to.Q - from.Q;
			int directionR = to.R - from.R;
		
			int distance = 2; // We know that Distance:1 contains a token, startGame looking from Distance:2.
			while (!board.GetHex(startQ + distance * directionQ, startR + distance * directionR).IsEmpty ()) {
				distance++;
			}

			return board.GetHex(startQ + distance*directionQ, startR + distance*directionR);
		}
	}
}