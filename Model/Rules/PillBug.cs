using System;

namespace HiveMind
{
	public class PillBug : Bug
	{
		public PillBug ()
		{
		}

		#region implemented abstract members of Bug

		public override bool IsFreeToMove (HiveMind.Model.Token token, HiveMind.Model.Board board)
		{
			throw new NotImplementedException ();
		}

		public override System.Collections.Generic.List<HiveMind.Model.Hex> getTargetHexes (HiveMind.Model.Token token, HiveMind.Model.Board board)
		{
			throw new NotImplementedException ();
		}

		public override bool canMimic ()
		{
			throw new NotImplementedException ();
		}

		public override bool canMoveOthers ()
		{
			throw new NotImplementedException ();
		}

		#endregion

	}
}

