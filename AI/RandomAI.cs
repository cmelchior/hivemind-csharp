using System;
using HiveMind.AI.Statistics;
using HiveMind.Game;
using HiveMind.Model;
using System.Collections.Generic;

namespace HiveMind
{
	/// <summary>
	/// Test AI that randomly either places a bug or moves a existing bug.
	/// For the first 3 turns there is a 50% each turn to place the queen.
	/// If queen has not been placed by turn 3 it will always happen on turn 4.
	/// </summary>
	public class RandomAI : HiveAI
	{
		private String name;
		private Random random = new Random();
		private Game.Game state;
		private AIStatistics stats = new AIStatistics();

		public RandomAI(String name) 
		{
			this.name = name;
		}

		public HiveAI Copy() 
		{
			return new RandomAI(name);
		}

		public GameCommand NextMove (HiveMind.Game.Game state, Board board)
		{
			this.state = state;
			Player activePlayer = state.ActivePlayer;

			// If by 4th turn the queen hasn't been placed yet, it must be placed
			if (state.ActivePlayer.Moves == 3 && !activePlayer.HasPlacedQueen()) 
			{
				return moveQueenToRandomBoardLocation();
			}

			// 50% chance to place queen the first 3 turns
			if (!activePlayer.HasPlacedQueen() && random.NextBool()) 
			{
				return moveQueenToRandomBoardLocation();
			};

			// 50% to move token on board instead of adding new. Always add tokens until queen has been placed
			bool moveTokenOnBoard = activePlayer.HasPlacedQueen() && random.NextBool() && getRandomTokenFromBoard() != null;

			// Supply still has available tokens
			if (!moveTokenOnBoard && haveTokensInSupply()) 
			{
				Token token = getRandomTokenFromSupply();
				return moveTokenFromSupplyToRandomBoardLocation(token);
			} 
			else 
			{
				Token token = getRandomTokenFromBoard();
				if (token != null) 
				{
					return moveTokenToRandomLocation(token);
				} 
				else 
				{
					return GameCommand.PASS;
				}
			}
		}

		public AIStatistics GetAiStats() 
		{
			return stats;
		}

		private GameCommand moveQueenToRandomBoardLocation() 
		{
			Token token = state.ActivePlayer.GetFromSupply(BugType.QUEEN_BEE);
			return moveTokenFromSupplyToRandomBoardLocation(token);
		}

		private bool haveTokensInSupply() 
		{
			return state.ActivePlayer.Supply.Count > 0;
		}

		private GameCommand moveTokenToRandomLocation(Token token) 
		{
			List<Hex> targets = Rules.Rules.GetInstance().GetTargetHexes(token, state.Board);
			if (targets.Count == 0) 
			{
				return GameCommand.PASS;
			} 
			else 
			{
				Hex fromHex = token.Hex;
				Hex hex = targets[random.Next(targets.Count)];

				return new GameCommand(fromHex.Q, fromHex.R, hex.Q, hex.R, token, false);
			}
		}

		private GameCommand moveTokenFromSupplyToRandomBoardLocation(Token token) 
		{
			List<Hex> targets = Rules.Rules.GetInstance().getStartHexes(token.Player, state.Board);
			if (targets.Count == 0) 
			{
				return GameCommand.PASS;
			} 
			else 
			{
				Hex hex =  targets[random.Next(targets.Count)];
				return new GameCommand(Hex.SUPPLY, Hex.SUPPLY, hex.Q, hex.R, token, false);
			}
		}

		public Token getRandomTokenFromSupply() {
			Player player = state.ActivePlayer;
			HashSet<Token> supply = player.Supply;
			int size = supply.Count;
			if (size > 0) 
			{
				foreach (Token t in supply) 
				{
					return player.GetFromSupply(t.Type);
				}
			}

			return null;
		}

		public Token getRandomTokenFromBoard() 
		{
			Player activePlayer = state.ActivePlayer;
			HashSet<Token> tokens = Rules.Rules.GetInstance().getFreeTokens(activePlayer, state.Board);
			foreach (Token token in tokens) 
			{
				if (token.Player.Equals(activePlayer)) 
				{
					return token;
				}
			}

			return null;
		}

		public string GetName() 
		{
			return name;
		}

		public bool MaintainsStandardPosition() 
		{
			return false;
		}
	}
}

