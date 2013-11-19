using System;
using System.Text;
using HiveMind.Model;

namespace HiveMind.AI.Statistics
{
	public class GameStatistics
	{

		private static DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		private long start; // Start time in milliseconds from epox
		private long durationInMillis; // Duration of game in milliseconds

		public String WhiteName { private get; set; }
		private int whiteTurns = 0;
		private int whiteMoves = 0;
		private int whitePasses = 0;

		public String BlackName { private get; set; }
		private int blackTurns = 0;
		private int blackMoves = 0;
		private int blackPasses = 0;

		private GameStatus status = GameStatus.RESULT_NOT_STARTED;

		public AIStatistics WhiteAI { get; set; }
		public AIStatistics BlackAI { get; set; }

		public GameStatistics()
		{
			WhiteName = "?";
			BlackName = "?";
		}

		public String ShortSummary() 
		{
			return String.Format("{0} ({1}) vs. {2} ({3}) - {4} ms.: {5}", WhiteName, whiteTurns, BlackName, blackTurns, durationInMillis, status);
		}

		public String LongSummary() 
		{
			StringBuilder sb = new StringBuilder(ShortSummary());
			sb.Append('\n');
			sb.Append("Branching: " + WhiteAI.getAverageBranchFactor() + " vs. " + BlackAI.getAverageBranchFactor());
			sb.Append('\n');
			sb.Append("Time pr move (max.): "  + WhiteAI.getMaxTimePrMove() + " vs. " + BlackAI.getMaxTimePrMove());
			sb.Append('\n');
			sb.Append("Time pr move (avg.): "  + WhiteAI.getAverageTimePrMove() + " vs. " + BlackAI.getAverageTimePrMove());
			sb.Append('\n');
			sb.Append("Time pr move (mean): "  + WhiteAI.getMeanTimePrMove() + " vs. " + BlackAI.getMeanTimePrMove());
			sb.Append('\n');
			sb.Append("Positions pr move (max.): "  + WhiteAI.getMaxPositionsEvaluatedPrMove() + " vs. " + BlackAI.getMaxPositionsEvaluatedPrMove());
			sb.Append('\n');
			sb.Append("Positions pr move (avg.): "  + WhiteAI.getAveragePositionsEvaluatedPrMove() + " vs. " + BlackAI.getAveragePositionsEvaluatedPrMove());
			sb.Append('\n');
			sb.Append("Positions pr move (mean): "  + WhiteAI.getMeanPositionsEvaluatedPrMove() + " vs. " + BlackAI.getMeanPositionsEvaluatedPrMove());
			sb.Append('\n');
			sb.Append("------------------");
			sb.Append('\n');
			sb.Append(WhiteName + " time pr. move: [" + string.Join(",", WhiteAI.MillisecondsPrMove.ToArray()) + "]");
			sb.Append('\n');
			sb.Append(WhiteName + " game states pr. sec.: [" + string.Join(",", WhiteAI.GameStatesEvaluatedPrSecond.ToArray()) + "]");
			sb.Append('\n');
			sb.Append(WhiteName + " game states pr. move: [" + string.Join(",", WhiteAI.PositionsEvaluatedPrMove.ToArray()) + "]");
			sb.Append('\n');
			sb.Append(WhiteName + " cache hits pr. move: [" + string.Join(",", WhiteAI.CacheHits.ToArray()) + "]");
			sb.Append('\n');
			sb.Append(WhiteName + " average cutoff at move: " + WhiteAI.getAverageMovesEvaluatedBeforeCutoff());
			sb.Append('\n');
			sb.Append(BlackName + " time pr. move: [" + string.Join(",", BlackAI.MillisecondsPrMove.ToArray()) + "]");
			sb.Append('\n');
			sb.Append(BlackName + " game states pr. sec.: [" + string.Join(",", BlackAI.GameStatesEvaluatedPrSecond.ToArray()) + "]");
			sb.Append('\n');
			sb.Append(BlackName + " game states pr. move: [" + string.Join(",", BlackAI.PositionsEvaluatedPrMove.ToArray()) + "]");
			sb.Append('\n');
			sb.Append(BlackName + " cache hits pr. move: [" + string.Join(",", BlackAI.CacheHits.ToArray()) + "]");
			sb.Append('\n');
			sb.Append(BlackName + " average cutoff at move: [" + BlackAI.getAverageMovesEvaluatedBeforeCutoff());
			sb.Append('\n');
			sb.Append("------------------");

			return sb.ToString();
		}

		public void SetStatus(GameStatus status) 
		{
			this.status = status;
		}

		private void BlackMoves() {
			blackMoves++;
			blackTurns++;
		}

		private void BlackPasses() {
			blackPasses++;
			blackTurns++;
		}

		private void WhiteMoves() {
			whiteMoves++;
			whiteTurns++;
		}

		private void WhitePasses() {
			whitePasses++;
			whiteTurns++;
		}

		public void playerMoves(Player player) {
			if (player.Name == WhiteName) {
				WhiteMoves();
			} else {
				BlackMoves();
			}
		}

		public void playerPasses(Player player) {
			if (player.Name == WhiteName) {
				WhitePasses();
			} else {
				BlackPasses();
			}
		}

		public void stopGame() {
			durationInMillis = CurrentTimeMillis() - start;
		}

		public void startGame() {
			start = CurrentTimeMillis();
		}	
	
		private long CurrentTimeMillis()
		{
			return (long) ((DateTime.UtcNow - Jan1st1970).TotalMilliseconds);
		}
	}
}

