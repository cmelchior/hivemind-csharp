using System;

///
/// Utility class for some setups that are interesting to run heuristic checks on or use them as basis for AI checks.
///
using HiveMind.Game;
using HiveMind.Model;


namespace HiveMindTest
{
	public class TestSetups
	{

		/// Blacks have 2 pieces that can be moved to ensure the win.
		/// The returned state has black as the active player
		///
		/// | = = = = = = = = = = = = = = =  |
		/// |                  _ _           |
		/// |                /# # #\         |
		/// |               /# ANT #\        |
		/// |               \# -W- #/        |
		/// |                \#_#_#/         |
		/// |                /# # #\         |
		/// |           _ _ /# BTL #\ _ _    |
		/// |         /+ + +\# -W- #/     \  |
		/// |        /+ ANT +\#_#_#/  WIN  \ |
		/// |        \+ -B- +/# # #\  -B-  / |
		/// |         \+_+_+/# QBE #\ _ _ /  |
		/// |         /+ + +\# -W- #/# # #\  |
		/// |    _ _ /+ QBE +\#_#_#/# ANT #\ |
		/// |  /+ + +\+ -B- +/+ + +\# -W- #/ |
		/// | /+ HOP +\+_+_+/+ BTL +\#_#_#/  |
		/// | \+ -B- +/+ + +\+ -B- +/+ + +\  |
		/// |  \+_+_+/+ SPI +\+_+_+/+ SPI +\ |
		/// |        \+ -B- +/# # #\+ -B- +/ |
		/// |         \+_+_+/# HOP #\+_+_+/  |
		/// |               \# -W- #/        |
		/// |                \#_#_#/         |
		/// |                /# # #\         |
		/// |               /# HOP #\        |
		/// |               \# -W- #/        |
		/// |                \#_#_#/         |
		/// |                                |
		/// | = = = = = = = = = = = = = = =  |
		///
		/// @param state Game state with players ready.
		/// @return Game state forwarded to the given board setup. Move count might not be accurate.
		public static Game SureWinInOneTurn(Game state) {
			Board board = state.Board;
			Player white = state.WhitePlayer;
			Player black = state.BlackPlayer;

			// Black setup
			board.AddToken(white.GetFromSupply(BugType.SOLDIER_ANT), 0, 0);
			board.AddToken(white.GetFromSupply(BugType.BEETLE), 0, 1);
			board.AddToken(white.GetFromSupply(BugType.QUEEN_BEE), 0, 2);
			board.AddToken(white.GetFromSupply(BugType.GRASSHOPPER), 0, 4);
			board.AddToken(white.GetFromSupply(BugType.GRASSHOPPER), 0, 5);
			board.AddToken(white.GetFromSupply(BugType.SOLDIER_ANT), 1, 2);
			black.SetTokensMoved(6);

			// White setup
			board.AddToken(black.GetFromSupply(BugType.GRASSHOPPER), -2, 4);
			board.AddToken(black.GetFromSupply(BugType.SOLDIER_ANT), -1, 2);
			board.AddToken(black.GetFromSupply(BugType.QUEEN_BEE), -1, 3);
			board.AddToken(black.GetFromSupply(BugType.SPIDER), -1, 4);
			board.AddToken(black.GetFromSupply(BugType.BEETLE), 0, 3);
			board.AddToken(black.GetFromSupply(BugType.SPIDER), 1, 3);
			white.SetTokensMoved(6);

			state.ActivePlayer = black;
			return state;
		}

		/// Blacks have 4 pieces that can be moved to ensure the win in 2 turns.
		/// The returned state has black as the active player.
		///
		/// 1) Black: Move either of two ants to other queen
		/// 2) White: Block one piece
		/// 3) Black: Use either Beetle, Ant or Hopper to win
		///
		/// | = = = = = = = = = = = = = = = = = = = |
		/// |                         _ _           |
		/// |                       /# # #\         |
		/// |                  _ _ /# ANT #\        |
		/// |                /# # #\# -W- #/        |
		/// |               /# ANT #\#_#_#/         |
		/// |               \# -W- #/               |
		/// |                \#_#_#/                |
		/// |                /# # #\                |
		/// |           _ _ /# SPI #\ _ _           |
		/// |         /     \# -W- #/     \         |
		/// |    _ _ /  T2   \#_#_#/  T1   \ _ _    |
		/// |  /+ + +\       /# # #\       /# # #\  |
		/// | /+ BTL +\ _ _ /# QBE #\ _ _ /# ANT #\ |
		/// | \+ -B- +/+ + +\# -W- #/# # #\# -W- #/ |
		/// |  \+_+_+/+ QBE +\#_#_#/# SPI #\#_#_#/  |
		/// |  /+ + +\+ -B- +/+ + +\# -W- #/+ + +\  |
		/// | /+ ANT +\+_+_+/+ ANT +\#_#_#/+ HOP +\ |
		/// | \+ -B- +/+ + +\+ -B- +/     \+ -B- +/ |
		/// |  \+_+_+/+ ANT +\+_+_+/       \+_+_+/  |
		/// |        \+ -B- +/                      |
		/// |         \+_+_+/                       |
		/// |                                       |
		/// | = = = = = = = = = = = = = = = = = = = |
		public static Game SureWinInTwoTurns(Game state) {
			Board board = state.Board;
			Player white = state.WhitePlayer;
			Player black = state.BlackPlayer;

			// Black setup
			board.AddToken(black.GetFromSupply(BugType.QUEEN_BEE), 0, 3);
			board.AddToken(black.GetFromSupply(BugType.SOLDIER_ANT), 0, 4);
			board.AddToken(black.GetFromSupply(BugType.SOLDIER_ANT), -1, 4);
			board.AddToken(black.GetFromSupply(BugType.BEETLE), -1, 3);
			board.AddToken(black.GetFromSupply(BugType.SOLDIER_ANT), 1, 3);
			board.AddToken(black.GetFromSupply(BugType.GRASSHOPPER), 3, 2);
			black.SetTokensMoved(6);

			// White setup
			board.AddToken(white.GetFromSupply(BugType.SOLDIER_ANT), 1, 0);
			board.AddToken(white.GetFromSupply(BugType.SPIDER), 1, 1);
			board.AddToken(white.GetFromSupply(BugType.QUEEN_BEE), 1, 2);
			board.AddToken(white.GetFromSupply(BugType.SOLDIER_ANT), 2, -1);
			board.AddToken(white.GetFromSupply(BugType.SPIDER), 2, 2);
			board.AddToken(white.GetFromSupply(BugType.SOLDIER_ANT), 3, 1);
			black.SetTokensMoved(6);

			state.ActivePlayer = black;
			return state;
		}
	}
}


