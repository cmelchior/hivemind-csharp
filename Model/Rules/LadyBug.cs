using System;
using HiveMind.Model;
using HiveMind.Model.Rules;

using System.Collections.Generic;

namespace HiveMind
{
	public class LadyBug : Bug
	{
		#region implemented abstract members of Bug

		public override bool IsFreeToMove (HiveMind.Model.Token token, HiveMind.Model.Board board)
		{
			// As a ladybug starts by crawling on top of the hive, she can always move.
			return true;
		}

		public override List<Hex> getTargetHexes (Token token, Board board)
		{
			// Recursively find all squares you can slide to. Only squares at distance 3 count
			List<Hex> visitedHexes = new List<Hex>();
			visitedHexes.Add(token.Hex);
			HashSet<Hex> result = recursiveVisit(token, 1, board, new HashSet<Hex>());
			result.Remove(token.Hex); // Remove starting hex
			return new List<Hex>(result);
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

		private HashSet<Hex> recursiveVisit(Token from, int distance, Board board, HashSet<Hex> targetHexes) 
		{

			// Crawl up on hive
			if (distance == 1) 
			{
				List<Token> neighbors = board.GetNeighborTokens(from.Hex);
				distance++;
				foreach (Token neighbor in neighbors) 
				{
					Hex originalHex = from.Hex;
					Hex toHex = neighbor.Hex;
					if (Rules.Rules.GetInstance().CanCrawlUp(from.Hex, toHex, board)) 
					{
						board.MoveToken(from, toHex.Q, toHex.R);
						recursiveVisit(from, distance, board, targetHexes);
						board.MoveToken(from, originalHex.Q, originalHex.R);
					}
				}

				// Move around hive
			} 
			else if (distance == 2) 
			{
				List<Token> neighbors = board.GetNeighborTokens(from.Hex);
				distance++;
				foreach (Token neighbor in neighbors) 
				{
					Hex originalHex = from.Hex;
					Hex toHex = neighbor.Hex;
					if (Rules.Rules.GetInstance().CanSlideTo(originalHex, toHex, board) || Rules.Rules.GetInstance().CanCrawlUp(originalHex, toHex, board)) 
					{
						board.MoveToken(from, toHex.Q, toHex.R);
						recursiveVisit(from, distance, board, targetHexes);
						board.MoveToken(from, originalHex.Q, originalHex.R);
					}
				}

				// Move down from hive
			} 
			else if (distance == 3) 
			{
				List<Hex> neighbors = board.GetNeighborHexes(from.Hex);
				foreach (Hex neighbor in neighbors) {
					if (neighbor.GetHeight() > 0) continue;
					if (Rules.Rules.GetInstance().CanCrawlDown(from.Hex, neighbor, board)) {
						targetHexes.Add(neighbor);
					}
				}

				return targetHexes;
			}

			return targetHexes;
		}
	}
}