using System;
using HiveMind.Model;
using System.Collections.Generic;

namespace HiveMind
{
	public class Beetle : Bug
	{
		#region implemented abstract members of Bug

		public override bool IsFreeToMove (HiveMind.Model.Token token, HiveMind.Model.Board board)
		{
			// A bettle can always move. Even though some positions can be walled in, the bettle can always just go
			// on top of the wall instead.
			return true;
		}

		public override List<Hex> getTargetHexes (Token token, Board board)
		{
			List<Hex> neighbors = board.GetNeighborHexes(token.Hex);
			List<Hex> result = new List<Hex>();
		
			Hex startingHex = token.Hex;
			foreach (Hex targetHex in neighbors) 
			{
				bool canSlide = Rules.Rules.GetInstance().CanSlideTo(startingHex, targetHex, board);
				bool canCrawl = Rules.Rules.GetInstance().CanCrawlUp(startingHex, targetHex, board);
				bool stillOneHive = Rules.Rules.GetInstance().IsOneHiveIntact(startingHex, targetHex, board);
				if ((canSlide || canCrawl) && stillOneHive) 
				{
					result.Add(targetHex);
				}
			}
		
			return result;
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
	}
}
