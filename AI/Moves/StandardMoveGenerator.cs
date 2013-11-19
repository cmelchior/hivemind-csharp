using System;
using System.Collections.Generic;

namespace HiveMind.Moves
{
	public class StandardMoveGenerator : MoveGenerator
	{
		public StandardMoveGenerator ()
		{
		}

		#region implemented abstract members of MoveGenerator

		public override List<HiveMind.Game.GameCommand> generateMoves (List<HiveMind.Game.GameCommand> initialList, Game.Game state)
		{

			throw new NotImplementedException ();
//			List<GameCommand> result = initialList;
//			Player player = state.getActivePlayer();
//			Board board = state.getBoard();
//
//			// If in game ending position, no moves are possible
//			if (Rules.getInstance().isQueenSurrounded(player, board) || Rules.getInstance().isQueenSurrounded(state.getOtherPlayer(), board)) {
//				return result;
//			}
//
//			// If turn 4 and not placed queen, it must be placed now
//			Set<Token> availableTokensFromSupply;
//			if (player.getMoves() == 3 && !player.hasPlacedQueen()) {
//				availableTokensFromSupply = new HashSet<Token>();
//				availableTokensFromSupply.add(player.getFromSupply(BugType.QUEEN_BEE));
//			}  else {
//				availableTokensFromSupply = player.getSupply();
//			}
//
//			// Get all moves adding tokens from supply
//			for (Token token : availableTokensFromSupply) {
//				List<Hex> hexes = Rules.getInstance().getStartHexes(player, board);
//				for (Hex hex : hexes) {
//					result.add(createGameCommand(token, hex));
//				}
//			}
//
//			// Get moves for all tokens on the board (only allowed if player has placed queen)
//			if (player.hasPlacedQueen()) {
//				Set<Token> inPlayTokens = Rules.getInstance().getFreeTokens(player, board);
//				for (Token token : inPlayTokens) {
//					List<Hex> hexes = Rules.getInstance().getTargetHexes(token, board);
//					for (Hex hex : hexes) {
//						result.add(createGameCommand(token, hex));
//					}
//				}
//			}
//
//			if (result.isEmpty()) {
//				result.add(GameCommand.PASS);
//			}
//
//			return result;
		}

		#endregion
	}
}

