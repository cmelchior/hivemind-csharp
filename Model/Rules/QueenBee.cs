using System;
using HiveMind.Model;
using System.Collections.Generic;

namespace HiveMind
{
	public class QueenBee : Bug
	{
		#region implemented abstract members of Bug

		public override bool IsFreeToMove(Token token, Board board)
		{
			return isRoomToSlideAwayOnGround(token, board);
		}

		public override List<Hex> getTargetHexes(Token token, Board board)
		{
			List<Hex> neighbors = board.GetNeighborHexes(token.Hex);
			List<Hex> result = new List<Hex>();

			Hex startingHex = token.Hex;
			foreach (Hex targetHex in neighbors) {
				bool canSlide = Rules.Rules.GetInstance().CanSlideTo(startingHex, targetHex, board);
				bool stillOneHive = Rules.Rules.GetInstance().IsOneHiveIntact(startingHex, targetHex, board);
				if (canSlide && stillOneHive) {
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