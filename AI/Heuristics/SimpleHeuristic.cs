using System;
using HiveMind.Model;
using System.Collections.Generic;

namespace HiveMind
{
	public class SimpleHeuristic : BoardValueHeuristic
	{
		public SimpleHeuristic ()
		{
		}

		#region BoardValueHeuristic implementation

		public int calculateBoardValue (Game.Game state)
		{
			/// Heuristics
			/// - More pieces is better
			/// - More free pieces is better
			/// - Number of free hexes around the queen both offensive and defensive
			/// - Different bugs have different values
			Player whitePlayer = state.WhitePlayer;
			Player blackPlayer = state.BlackPlayer;

			bool blackWon = Rules.Rules.GetInstance ().isQueenSurrounded (whitePlayer, state.Board);
			bool whiteWon = Rules.Rules.GetInstance ().isQueenSurrounded (blackPlayer, state.Board);

			if (blackWon && whiteWon) {
				// A draw is considered almost as bad as a LOSS (no one wants an AI that tries to DRAW)
				if (state.ActivePlayer.IsWhitePlayer ()) {
					return int.MinValue + 1;
				} else {
					return int.MaxValue - 1;
				}
			} else if (blackWon) {
				return int.MinValue;
			} else if (whiteWon) {
				return int.MaxValue;
			}

			int whiteAntsInPlay = 0;
			int blackAntsInPlay = 0;

			List<Hex> hexes = state.Board.GetFilledHexes ();
			foreach (Hex hex in hexes) {
				Token t = hex.SeeTopToken ();
				if (t.OriginalType == BugType.SOLDIER_ANT) {
					if (t.Player.IsBlackPlayer ()) {
						blackAntsInPlay++;
					} else {
						whiteAntsInPlay++;
					}
				}
			}

			int whiteFreeTokens = Rules.Rules.GetInstance ().getFreeTokens (whitePlayer, state.Board).Count;
			int whiteTokensOnBoard = whitePlayer.GetNoStartingBugs () - whitePlayer.Supply.Count;
			int whiteHexesFilledAroundOpposingQueen = state.Board.GetNeighborTokens (blackPlayer.Queen.Hex).Count;

			int blackFreeTokens = Rules.Rules.GetInstance ().getFreeTokens (blackPlayer, state.Board).Count;
			int blackTokensOnBoard = blackPlayer.GetNoStartingBugs () - blackPlayer.Supply.Count;
			int blackHexesFilledAroundOpposingQueen = state.Board.GetNeighborTokens (whitePlayer.Queen.Hex).Count;

			return 100 * (whiteHexesFilledAroundOpposingQueen - blackHexesFilledAroundOpposingQueen)
			+ 20 * (whiteAntsInPlay - blackAntsInPlay)
			+ 10 * (whiteFreeTokens - blackFreeTokens);
		}
		#endregion
	}
}

