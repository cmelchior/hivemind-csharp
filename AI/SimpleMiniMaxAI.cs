using System;
using HiveMind.Game;
using System.Collections.Generic;
using HiveMind.Model;
using HiveMind.AI;

namespace HiveMind.AI
{
	/// AI that implements standard MinMax tree search.
	public class SimpleMiniMaxAI : AbstractMiniMaxAI 
	{
		private Random random = new Random();
		private new int searchDepth = 0; // Search depth for tree

		public SimpleMiniMaxAI(String name, BoardValueHeuristic heuristic, int depth, int maxTimeInMillis) : base(name, heuristic, depth, maxTimeInMillis)
		{
			this.searchDepth = depth;
		}

		public override HiveAI Copy() 
		{
			return new SimpleMiniMaxAI(name, heuristic, searchDepth, maxTimeInMillis);
		}

		public override GameCommand NextMove(Game.Game state, Board board) 
		{
			maximizingPlayer = state.ActivePlayer;
			start = Environment.TickCount;

			// Minimax traversal of game tree
			List<GameCommand> moves = generateMoves(state, new GameCommand[] {});
			int bestValue = int.MinValue;
			GameCommand bestMove = GameCommand.PASS;

			foreach (GameCommand move in moves) 
			{
				applyMove(move, state);
				int value = minimax(state, searchDepth - 1, false);
				if (value > bestValue || value == bestValue && random.NextBool()) {
					bestValue = value;
					bestMove = move;
				}
				undoMove(move, state);
			}

			return bestMove;
		}

		/// <summary>
		/// Minimax traversal. Returns a number between Integer.MIN and Integer.MAX
		/// + means current player is winning, - he is loosing
		/// </summary>
		private int minimax(Game.Game state, int depth, bool maximizingPlayer) 
		{
			if (isGameOver(state, depth) || depth <= 0 || Environment.TickCount - start > maxTimeInMillis) {
				// Positive values are good for the maximizing player, negative values for minimizing player
				return Value(state);

			} 
			else 
			{

				List<GameCommand> moves = generateMoves(state, new GameCommand[]{});
				int bestValue = int.MinValue;

				foreach (GameCommand move in moves) 
				{
					// Update game state and continue traversel
					applyMove(move, state);
					int value = minimax(state, depth - 1, !maximizingPlayer);
					if (maximizingPlayer) 
					{
						if (value > bestValue || (value == bestValue && random.NextBool())) 
						{
							bestValue = value;
						}
					} 
					else 
					{
						if (value < bestValue || (value == bestValue && random.NextBool())) 
						{
							bestValue = value;
						}
					}
					undoMove(move, state);
				}

				return bestValue;
			}
		}
	}
}

