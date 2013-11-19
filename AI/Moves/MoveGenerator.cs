using System;
using System.Collections.Generic;
using HiveMind.Game;
using HiveMind.Model;

namespace HiveMind.Moves
{

	/// <summary>
	/// Interface for move generators used by HiveAI's.
	/// </summary>
	public abstract class MoveGenerator {

		public abstract List<GameCommand> generateMoves(List<GameCommand> initialList, Game.Game state);

		protected GameCommand createGameCommand(Token token, Hex hex) 
		{
			if (token.Hex == null) 
			{
				return new GameCommand(Hex.SUPPLY, Hex.SUPPLY, hex.Q, hex.R, token, false);
			} 
			else 
			{
				return new GameCommand(token.Hex.Q, token.Hex.R, hex.Q, hex.R, token, false);
			}
		}
	}
}

