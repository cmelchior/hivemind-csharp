using System;
using System.Collections.Generic;
using HiveMind.Model;

namespace HiveMind
{
	public class UnknownBug : Bug
	{
		public UnknownBug ()
		{
		}

		#region implemented abstract members of Bug
		public override bool IsFreeToMove (HiveMind.Model.Token token, HiveMind.Model.Board board)
		{
			return false;
		}

		public override List<HiveMind.Model.Hex> getTargetHexes (HiveMind.Model.Token token, HiveMind.Model.Board board)
		{
			return new List<Hex>();
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