using System;
using System.Collections.Generic;
using HiveMind.Model;

namespace HiveMind
{
	public class Spider : Bug
	{
		#region implemented abstract members of Bug

		public override bool IsFreeToMove (HiveMind.Model.Token token, HiveMind.Model.Board board)
		{
			return isRoomToSlideAwayOnGround(token, board);
		}

		public override List<Hex> getTargetHexes (Token token, Board board)
		{
			// Recursively find all squares you can slide to. Only squares at distance 3 count
			List<Hex> visitedHexes = new List<Hex>();
			visitedHexes.Add(token.Hex);
			return recursiveVisit(token, 0, board, new List<Hex>(), visitedHexes);
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

		private List<Hex> recursiveVisit(Token from, int distance, Board board, List<Hex> targetHexes, List<Hex> visitedHexes) 
		{
			List<Hex> neighbors = board.GetNeighborHexes(from.Hex);
			distance++;
			foreach (Hex targetHex in neighbors) 
			{
				if (visitedHexes.Contains(targetHex)) continue;
				visitedHexes.Add(targetHex);
				if (Rules.Rules.GetInstance().CanSlideTo(from.Hex, targetHex, board)) 
				{

					// Only hexes exactly 3 fullfill the spiders movement rules
					if (distance == 3) 
					{
						targetHexes.Add(targetHex);
					}

					// Only need to look for more targets if not at maximum range
					if (distance < 3) 
					{
						Hex originalHex = from.Hex;
						board.MoveToken(from, targetHex.Q, targetHex.R);
						recursiveVisit(from, distance, board, targetHexes, visitedHexes);
						board.MoveToken(from, originalHex.Q, originalHex.R);
					}
				}
			}

			return targetHexes;
		}


	}
}
