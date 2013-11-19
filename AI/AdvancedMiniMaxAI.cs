using System;
using System.Collections.Generic;
using HiveMind.Game;
using HiveMind.Model;
using HiveMind.AI.TranspositionTable;

namespace HiveMind.AI
{
	public class AdvancedMiniMaxAI : AbstractMiniMaxAI
	{
		private Random random = new Random();
		private TranspositionTable.TranspositionTable table = new TranspositionTable.TranspositionTable();
		private List<LimitedBuffer<GameCommand>> killerMoves = new List<LimitedBuffer<GameCommand>>();

		public AdvancedMiniMaxAI(String name, BoardValueHeuristic heuristicFunction, int depth, int maxTimeInMillis) : base(name, heuristicFunction, depth, maxTimeInMillis)
		{
			for (int i = 0; i < depth; i++) {
				killerMoves.Add(new LimitedBuffer<GameCommand>(2));
			}
		}

		public override HiveAI Copy() 
		{
			return new AdvancedMiniMaxAI(name, heuristic, searchDepth, maxTimeInMillis);
		}

		public override GameCommand NextMove(Game.Game state, Board board) 
		{
			start = Environment.TickCount;
			maximizingPlayer = state.ActivePlayer;

			// Clear previous killer moves
			foreach (LimitedBuffer<GameCommand> buffer in killerMoves) {
				buffer.Clear();
			}

			// Iterate depths, effectively a breath-first search, where top nodes get visited multiple times
			int depth = 0;
			int bestValue = int.MinValue;
			GameCommand bestCommand = null;

			Object[] result = new Object[2];
			while(depth <= searchDepth && Environment.TickCount - start < maxTimeInMillis) {
				result = runMinMax(state, depth, result);
				int val = (int) result[0];
				if (val > bestValue || val == bestValue && random.NextBool()) {
					bestValue = val;
					bestCommand = (GameCommand) result[1];
					if (bestValue == int.MaxValue) {
						return bestCommand; // Game winning move
					}
				}

				depth++;
			}

			return bestCommand; // 2nd best move
		}

		private Object[] runMinMax(Game.Game state, int searchDepth, Object[] result) 
		{

			// Minimax traversal of game tree
			List<GameCommand> moves = generateMoves(state, new GameCommand[0]);
			int bestValue = int.MinValue;
			GameCommand bestMove = GameCommand.PASS;

			foreach (GameCommand move in moves) 
			{
				// Update game state and continue traversel
				applyMove(move, state);
				int value = alphabeta(state, searchDepth - 1, bestValue, int.MaxValue, false);
				if (value > bestValue || value == bestValue && random.NextBool()) {
					bestValue = value;
					bestMove = move;
				}
				undoMove(move, state);
			}

			result[0] = bestValue;
			result[1] = bestMove;
			return result;
		}

		private int alphabeta(Game.Game state, int depth, int alpha, int beta, bool maximizingPlayer) 
		{
			int originalAlpha = alpha;
			int originalBeta = beta;
			GameCommand bestMove = null;

			// Check transposition table and adjust values if needed or return result if possible
			long zobristKey = state.zobristKey;
			TranspositionTableEntry entry = table.getResult(zobristKey);
			if (entry != null && entry.Depth >= depth) {
				aiStats.CacheHit();
				bestMove = entry.Move;
				if (entry.Type == TranspositionTableEntry.PV_NODE) {
					return entry.Value;
				} else if (entry.Type == TranspositionTableEntry.CUT_NODE && entry.Value > alpha) {
					alpha = entry.Value;
				} else if (entry.Type == TranspositionTableEntry.ALL_NODE && entry.Value < beta) {
					beta = entry.Value;
				}

				if (alpha >= beta) {
					return entry.Value; // Lowerbound is better than upper bound
				}
			}


			// Run algorithm as usual
			int value;
			if (isGameOver(state, depth) || depth <= 0 || Environment.TickCount - start > maxTimeInMillis) {
				value = Value(state);
			} else {

				// Generate moves
				GameCommand[] killMoves = killerMoves[depth].ToArray();
				List<GameCommand> moves = generateMoves(state, new GameCommand[] { bestMove, killMoves[0], killMoves[1] });
				int moveEvaluated = 0;

				if (maximizingPlayer) 
				{
					foreach (GameCommand move in moves) 
					{
						moveEvaluated++;
						bestMove = move;
						applyMove(move, state);
						value = alphabeta(state, depth - 1, alpha, beta, !maximizingPlayer);
						if (value > alpha) 
						{
							alpha = value;
						}
						undoMove(move, state);

						// Beta cut-off
						if (beta <= alpha) 
						{
							aiStats.cutOffAfter(moveEvaluated);
							killerMoves[depth].Add(move);
							break;
						}
					}

					value = alpha;

				} 
				else 
				{
					foreach (GameCommand move in moves) 
					{
						moveEvaluated++;
						bestMove = move;
						applyMove(move, state);
						value = alphabeta(state, depth - 1, alpha, beta, !maximizingPlayer);
						if (value < beta) 
						{
							beta = value;
						}
						undoMove(move, state);

						// Alpha cut-off
						if (beta <= alpha) 
						{
							aiStats.cutOffAfter(moveEvaluated);
							killerMoves[depth].Add(move);
							break;
						}
					}

					value = beta;
				}
			}

			// Update transposition table
			if (value <= originalAlpha) 
			{
				table.addResult(zobristKey, value, depth, TranspositionTableEntry.CUT_NODE, bestMove);
			} 
			else if (value >= originalBeta) 
			{
				table.addResult(zobristKey, value, depth, TranspositionTableEntry.ALL_NODE, bestMove);
			} 
			else 
			{
				table.addResult(zobristKey, value, depth, TranspositionTableEntry.PV_NODE, bestMove);
			}

			return value;
		}

		new public bool MaintainsStandardPosition() 
		{
			return true;
		}
	}
}