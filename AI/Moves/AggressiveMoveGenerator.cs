using System;
using System.Collections.Generic;
using HiveMind.Game;
using HiveMind.Model;

namespace HiveMind.Moves
{
	/// Move generator that generate all possible moves, but puts moves that makes them neighbors to the opposing queen
	/// first on the list.
	public class AggressiveMovesFirstGenerator : MoveGenerator {

		public override List<GameCommand> generateMoves(List<GameCommand> initialList, Game.Game state) {
			List<GameCommand> result = new List<GameCommand>(initialList);
			Player player = state.ActivePlayer;
			Board board = state.Board;

			// If in game ending position, no moves are possible
			if (Rules.Rules.GetInstance().isQueenSurrounded(player, board) || Rules.Rules.GetInstance().isQueenSurrounded(state.getOtherPlayer(), board)) {
				return result;
			}

			// If turn 4 and not placed queen, it must be placed now
			HashSet<Token> availableTokensFromSupply;
			if (player.Moves == 3 && !player.HasPlacedQueen()) 
			{
				availableTokensFromSupply = new HashSet<Token>();
				availableTokensFromSupply.Add(player.GetFromSupply(BugType.QUEEN_BEE));
			}  
			else 
			{
				availableTokensFromSupply = player.Supply;
			}

			// Get opposite queen coordinates
			Hex oppositeQueenHex = state.getOtherPlayer().Queen.Hex;
			int queenQ = (oppositeQueenHex != null) ? oppositeQueenHex.Q : Hex.SUPPLY;
			int queenR = (oppositeQueenHex != null) ? oppositeQueenHex.R : Hex.SUPPLY;

			// 1) Get moves for all tokens on the board (only allowed if player has placed queen)
			if (player.HasPlacedQueen()) {
				HashSet<Token> inPlayTokens = Rules.Rules.GetInstance().getFreeTokens(player, board);
				foreach (Token token in inPlayTokens) {
					List<Hex> hexes = Rules.Rules.GetInstance().GetTargetHexes(token, board);
					foreach (Hex hex in hexes) 
					{
						if (HexagonUtils.Distance(queenQ, queenR, hex.Q, hex.R) <= 1) 
						{
							result.Insert(0, createGameCommand(token, hex)); // Moves near opposing queens added in front of the list.
						} 
						else 
						{
							result.Add(createGameCommand(token, hex)); // else add to end of list.
						}
					}
				}
			}

			// 2) Get all moves adding tokens from supply
			foreach (Token token in availableTokensFromSupply) 
			{
				List<Hex> hexes = Rules.Rules.GetInstance().getStartHexes(player, board);
				foreach (Hex hex in hexes) 
				{
					if (HexagonUtils.Distance(queenQ, queenR, hex.Q, hex.R) <= 1) 
					{
						result.Insert(0, createGameCommand(token, hex)); // Moves near opposing queens added in front of the list.
					}
					else
					{
						result.Add(createGameCommand(token, hex)); // else add to end of list.
					}
				}
			}

			if (result.Count == 0) 
			{
				result.Add(GameCommand.PASS);
			}

			return result;
		}
	}
}

