using System;
using System.Collections.Generic;
using HiveMind.Model;

namespace HiveMind
{
	public class Mosquito : Bug
	{
		#region implemented abstract members of Bug

		public override bool IsFreeToMove (HiveMind.Model.Token token, HiveMind.Model.Board board)
		{
			return false;  // A mosquito never moves except by mimicing other bugs.
		}

		public override List<Hex> getTargetHexes(Token token, Board board)
		{
			return new List<Hex>();
		}

		public override bool canMimic ()
		{
			return true;
		}

		public override bool canMoveOthers ()
		{
			return false;
		}

		#endregion
	}
}

