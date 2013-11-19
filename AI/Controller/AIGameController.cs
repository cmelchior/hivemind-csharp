using System;
using System.Collections.Generic;
using HiveMind.AI.Statistics;
using HiveMind.Utils;
using System.Threading;
using HiveMind.Model;
using HiveMind.Game;
using System.Text;

namespace HiveMind.AI.Controller
{
	/// <summary>
	/// Controller class for executing AI battles.
	/// Given x number of opponents, y number of battles are executed among all pairs.
	/// Results are then printed to the screen.
	/// </summary>
	public class AIGameController
	{

		// How many games can run simultaneously. Max should be #CPUs - 1 to avoid CPU contention.
		// Look into if it is possible to force Java to use different CPU's. Right now we just cross fingers and hope.
		private const int THREADS = 3;

		private int turnLimit;
		private int numberOfMatches;

		private long duration;
		private HashSet<HiveAI> opponents = new HashSet<HiveAI>();
		private ThreadSafeList<GameStatistics> gameResults = new ThreadSafeList<GameStatistics>();

		public void AddOpponent(HiveAI opponent) 
		{
			opponents.Add(opponent);
		}

		/// <summary>
		/// Set turn limit pr. player
		/// </summary>
		/// <param name="turnLimit">Turn limit.</param>
		public void SetTurnLimit(int turnLimit) 
		{
			this.turnLimit = turnLimit;
		}

		public void SetNumberOfMatches(int numberOfMatches) 
		{
			this.numberOfMatches = numberOfMatches;
		}

		public void start() 
		{
			ThreadPool.SetMaxThreads(THREADS, THREADS);
			long start = Environment.TickCount;
			int games = 0;
			foreach (HiveAI oppA in opponents) 
			{
				foreach (HiveAI oppB in opponents) 
				{
					if (oppA.Equals(oppB)) continue;
					for (int i = 0; i < numberOfMatches; i++) 
					{
						games++;
						HiveAI a = oppA.Copy();
						HiveAI b = oppB.Copy();
						ThreadPool.QueueUserWorkItem(state => {
							object[] array = state as object[];
							HiveAI opponentA = (HiveAI) array[0];
							HiveAI opponentB = (HiveAI) array[1];
							runGame(opponentA, opponentB, false);

						}, new object[] { a, b });
					}
				}
			}

			duration = Environment.TickCount - start;
		}

		public void StartSingleGame(HiveAI whitePlayer, HiveAI blackPlayer, bool printGameState) 
		{
			runGame(whitePlayer, blackPlayer, printGameState);
		}

		private void runGame(HiveAI whiteAI, HiveAI blackAI, bool printGameState) 
		{
			Player whitePlayer = new Player(whiteAI.GetName(), PlayerType.WHITE);
			whitePlayer.FillBaseSupply();
			whitePlayer.CommandProvider = delegate(Game.Game state, Board board) {
				whiteAI.GetAiStats().startCalculatingNextMove();
				GameCommand command = whiteAI.NextMove(state, board);
				whiteAI.GetAiStats().moveCalculated();
				return command;
			};

			Player blackPlayer = new Player(blackAI.GetName(), PlayerType.BLACK);
			blackPlayer.FillBaseSupply();
			blackPlayer.CommandProvider = delegate(Game.Game state, Board board) {
				blackAI.GetAiStats().startCalculatingNextMove();
				GameCommand command = blackAI.NextMove(state, board);
				blackAI.GetAiStats().moveCalculated();
				return command;
			};

			Game.Game game = new Game.Game();
			game.TurnLimit = turnLimit;
			game.printGameStateAfterEachMove = printGameState;
			game.AddPlayers(whitePlayer, blackPlayer);
			game.SetStandardPositionMode((whiteAI.MaintainsStandardPosition() || blackAI.MaintainsStandardPosition()) ? StandardPositionMode.ENABLED : StandardPositionMode.DISABLED);
			try {
				game.Start();
			} catch (Exception e) {
				Console.Out.WriteLine (e);
			}

			GameStatistics statistics = game.Statistics;
			statistics.WhiteAI = whiteAI.GetAiStats();
			statistics.BlackAI = blackAI.GetAiStats();

			gameResults.Add(game.Statistics);
			Console.Out.WriteLine(game.Statistics.ShortSummary());
		}

		public void printLog(bool longSummary) 
		{
			StringBuilder sb = new StringBuilder("Games done: " + (duration/1000f) + "s.\n");
			sb.Append("================\n");
			foreach (GameStatistics stats in gameResults.GetList()) 
			{
				sb.Append((longSummary ? stats.LongSummary() : stats.ShortSummary()));
				sb.Append('\n');
			}
			sb.Append("================");
			Console.Out.WriteLine(sb.ToString());
		}	
	}
}

