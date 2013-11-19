using System;
using HiveMind.Model;
using System.Collections.Generic;

namespace HiveMind
{
	public abstract class Bug
	{

		/// <summary>
		/// Returns true if the token can move.
		/// INVARIANT: It is already assumed that moving the token will full the "One Hive" rule.
		/// </summary>
		/// <returns>True if the token can move</returns>
		/// <param name="token">Token to move.</param>
		/// <param name="board">Board.</param>
		public abstract bool IsFreeToMove(Token token, Board board);

		/// <summary>
		/// Returns true if the token can move.
		/// INVARIANT: It is already assumed that moving the token will full the "One Hive" rule.
		/// </summary>
		/// <returns>Legal hexes the token can move to.</returns>
		/// <param name="token">Token.</param>
		/// <param name="board">Board.</param>
		public abstract List<Hex> getTargetHexes(Token token, Board board);

		/// <summary>
		/// Returns true if the given bug type has the "Mimic movement" ability, eg. the Mosquito.
		/// </summary>
		/// <returns>The mimic.</returns>
		public abstract bool canMimic();

		/// <summary>
		/// Returns true if the given bug type has the ability to move other bugs around, eg. the PillBug.
		/// </summary>
		public abstract bool canMoveOthers();

		/// <summary>
		/// Returns true if it is possible to slide away from the given position.
		/// This requires two empty spaces next to each other.
		///
		/// INVARIANT: Only works at the ground level.
		/// </summary>
		/// <returns>The room to slide away on ground.</returns>
		/// <param name="token">Token.</param>
		/// <param name="board">Board.</param>
		protected bool isRoomToSlideAwayOnGround(Token token, Board board) {
			List<Hex> hexes = board.GetEmptyNeighborHexes(token);

			// Fullfill the "Freedom to move" rule if two hexes next to each other is empty
			int free = 0;
			foreach (Hex hex in hexes) {
				if (hex.IsEmpty()) {
					free++;
					if (free == 2) {
						return true;
					}
				} else {
					free = Math.Max(0, free - 1);
				}
			}

			return false;
		}
	}
}

