using System;
using HiveMind.AI.Statistics;
using HiveMind.Model;
using HiveMind.Game;
using System.Collections.Generic;
using HiveMind.Moves;

namespace HiveMind.AI
{
	public abstract class AbstractMiniMaxAI : HiveAI
	{
		protected String name;
		protected Game.Game state;
		protected AIStatistics aiStats = new AIStatistics();

		protected MoveGenerator moveGenerator = new AggressiveMovesFirstGenerator();
		protected BoardValueHeuristic heuristic;

		protected int searchDepth;      	// Search limit in depth
		protected int maxTimeInMillis;  	// Search limit in milliseconds
		protected long start; 				// Start time in millis when nextMove was called
		protected Player maximizingPlayer; 	// Player is who is acting as MAX player in the MinMax algorithm

		public AbstractMiniMaxAI(String name, BoardValueHeuristic heuristicFunction, int searchDepth, int maxTimeInMillis) {
			this.name = name;
			this.heuristic = heuristicFunction;
			this.searchDepth = searchDepth;
			this.maxTimeInMillis = maxTimeInMillis;
		}

		protected int calculateBoardValue(Game.Game state) 
		{
			aiStats.boardEvaluated();
			return heuristic.calculateBoardValue(state);
		}

		protected void setMaximizingPlayer(Player player) 
		{
			maximizingPlayer = player;
		}

		/// <summary>
		/// Heuristic function. Evaluate the value of the board.
		/// + is good for the maximizing player, - is good for the minimizing player
		/// </summary>
		/// <param name="state">State.</param>
		protected int Value(Game.Game state) {
			int result = calculateBoardValue(state);
			if (maximizingPlayer.IsBlackPlayer()) 
			{
				if (result == int.MinValue) result = int.MaxValue;
				else if (result == int.MaxValue) result = int.MinValue;
				else result = result * -1; // Negative values are good for black in the heuristic function, so if he is maximizing we invert all values
			}
			return result;
		}

		protected Game.Game applyMove(GameCommand command, Game.Game state) 
		{
			command.Execute(state);
			return state;
		}

		protected Game.Game undoMove(GameCommand command, Game.Game state) 
		{
			command.undo(state);
			return state;
		}

		protected bool isGameOver(Game.Game state, int depth) 
		{
			bool whiteDead = Rules.Rules.GetInstance().isQueenSurrounded(state.WhitePlayer, state.Board);
			bool blackDead = Rules.Rules.GetInstance().isQueenSurrounded(state.BlackPlayer, state.Board);
			bool turnLimit = false;
			if (state.TurnLimit > 0) 
			{
				int lookAhead = (depth - searchDepth) * -1;
				turnLimit = (state.ActivePlayer.Moves + lookAhead) > state.TurnLimit;
			}
			return whiteDead || blackDead || turnLimit;
		}

		/// <summary>
		/// Standard move generation.
		/// </summary>
		/// <returns>The moves.</returns>
		protected List<GameCommand> generateMoves(Game.Game state, GameCommand[] priorityMoves) {
			List<GameCommand> initialList = new List<GameCommand>();
			List<GameCommand> result = moveGenerator.generateMoves(initialList, state);

			for (int i = priorityMoves.Length; i > 0; i--) {
				GameCommand priorityMove = priorityMoves[i - 1];
				if (priorityMove != null && result.Contains(priorityMove)) {
					result.Remove(priorityMove);
					result.Insert(0, priorityMove);
				}
			}

			aiStats.nodeBranched(result.Count);
			return result;
		}

		#region HiveAI implementation

		public abstract GameCommand NextMove (HiveMind.Game.Game state, Board board);
		public abstract HiveAI Copy();

		public bool MaintainsStandardPosition() 
		{
			return true;
		}

		public AIStatistics GetAiStats() 
		{
			return aiStats;
		}

		public String GetName() 
		{
			return name;
		}

		#endregion
	}
}

