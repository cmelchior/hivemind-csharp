using System;
using HiveMind.Model;
using System.Collections.Generic;

namespace HiveMind
{
	public class SoldierAnt : Bug
	{
		public SoldierAnt ()
		{
		}

		#region implemented abstract members of Bug

		public override bool IsFreeToMove (HiveMind.Model.Token token, HiveMind.Model.Board board)
		{
			return isRoomToSlideAwayOnGround(token, board);
		}

		public override List<Hex> getTargetHexes (Token token, Board board)
		{
			// Recursively find all squares you can slide to
			List<Hex> visitedHexes = new List<Hex>();
			visitedHexes.Add(token.Hex);
			return recursiveVisit(token, board, new List<Hex>(), visitedHexes);
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

		private List<Hex> recursiveVisit(Token from, Board board, List<Hex> targetHexes, List<Hex> visitedHexes) 
		{
			List<Hex> neighbors = board.GetNeighborHexes(from.Hex);
			foreach (Hex targetHex in neighbors) 
			{
				if (visitedHexes.Contains(targetHex)) continue;
				visitedHexes.Add(targetHex);
				if (Rules.Rules.GetInstance().CanSlideTo(from.Hex, targetHex, board)) 
				{
					targetHexes.Add(targetHex);
					Hex originalHex = from.Hex;
					board.MoveToken(from, targetHex.Q, targetHex.R);
					recursiveVisit(from, board, targetHexes, visitedHexes);
					board.MoveToken(from, originalHex.Q, originalHex.R);
				}
			}

			return targetHexes;
		}


	}
}