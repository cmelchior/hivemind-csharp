using System;

namespace HiveMind
{
	public interface BoardValueHeuristic
	{
		/// <summary>
		/// Calculate board value for the given state state.
		/// Value returned is between Integer.MIN_VALUE and Integer.MAX_VALUE.
		///
		/// Positive numbers indicate that white is winning, negative numbers that black is.
		/// </summary>
		int calculateBoardValue(Game.Game state);
	}
}

