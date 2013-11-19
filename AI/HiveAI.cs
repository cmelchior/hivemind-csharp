using System;
using HiveMind.AI.Statistics;
using HiveMind.Game;
using HiveMind.Model;

namespace HiveMind
{
	public interface HiveAI
	{

		/// <summary>
		/// Return the next game move which the AI considers the best depending on the state of the game.
		/// </summary>
		GameCommand NextMove(Game.Game state, Board board);

		/// <summary>
		/// Return metrics from this AI
		/// </summary>
		AIStatistics GetAiStats();

		/// <summary>
		/// Return the name of the AI
		/// </summary>
		String GetName();

		/// <summary>
		/// Copy the AI parameters to create a new version with the same name.
		/// </summary>
		HiveAI Copy();

		/// <summary>
		/// Returns true if the HiveAI uses zobrist keys and Standard Position. If not, no need to use computational power to maintain Standard
		/// Position if it is not used.
		/// </summary>
		bool MaintainsStandardPosition();
	}
}

