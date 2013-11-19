using System;
using System.Collections.Generic;

namespace HiveMind.AI.Statistics
{
	public class AIStatistics
	{

		private const bool DEBUG = true;

		// Result arrays
		public List<long> MillisecondsPrMove { get; private set; }  // Time in milliseconds pr. move of the game
		public List<int> GameStatesEvaluatedPrSecond { get; private set; } // Normalized "performance" value
		public List<int> PositionsEvaluatedPrMove { get; private set; } // Number of positions looked at when finding a move.
		public List<int> CacheHits { get; private set; } // Number of cachehits

		int numberOfCutoffs = 0;
		int totalCutoffTurns = 0; // Average number of moves before cutoffs is totalCutoffTurns/numberOfCutoffs

		int branches = 0;
		int nodes = 0;
		int cacheHit = 0;

		// Temporary data
		String currentKey;
		int aiDepth = 3;
		int positionsEvaluated = 0;    // How many moves has been considered when getting the next move.

		public AIStatistics() 
		{
			MillisecondsPrMove = new List<long>();
			GameStatesEvaluatedPrSecond = new List<int>();
			PositionsEvaluatedPrMove = new List<int>(); 
			CacheHits = new List<int>();
		}


		/// <summary>
		/// A new move request has been made.
		/// </summary>
		public void startCalculatingNextMove() 
		{
			currentKey = System.Guid.NewGuid().ToString();
			StopWatch.GetInstance().Start(currentKey);
			positionsEvaluated = 0;
		}

		/// <summary>
		/// The AI has returned a move.
		/// </summary>
		public void moveCalculated() 
		{
			long time = StopWatch.GetInstance().Stop(currentKey).GetElapsedTimeInMillis();
			int movesPrSecond = (int) (positionsEvaluated / (time / 1000d));

			MillisecondsPrMove.Add(time);
			GameStatesEvaluatedPrSecond.Add(movesPrSecond);
			PositionsEvaluatedPrMove.Add(positionsEvaluated);
			CacheHits.Add(cacheHit);

			if (DEBUG) 
			{
				Console.Out.WriteLine("Turn length: " + (time/1000d) + " s.");
			}
		}

		/// <summary>
		/// The AI has evaluated a game state using the heuristic function
		/// </summary>
		public void boardEvaluated() 
		{
			positionsEvaluated++;
		}

		public double getAverageTimePrMove() 
		{
			long result = 0;
			foreach (long i in MillisecondsPrMove) 
			{
				result += i;
			}

			return result / (double) MillisecondsPrMove.Count;
		}

		public long getMeanTimePrMove() 
		{
			if (MillisecondsPrMove.Count == 0) return 0;
			long[] list = MillisecondsPrMove.ToArray();
			Array.Sort(list);
			return list[list.Length/2];
		}

		public long getMaxTimePrMove() {
			long result = 0;
			foreach (long i in MillisecondsPrMove) 
			{
				if (i > result) 
				{
					result = i;
				}
			}
			return result;
		}

		public double getAveragePositionsEvaluatedPrMove() 
		{
			int result = 0;
			foreach (int i in PositionsEvaluatedPrMove) 
			{
				result += i;
			}

			return result / (double) PositionsEvaluatedPrMove.Count;
		}

		public int getMeanPositionsEvaluatedPrMove() 
		{
			if (PositionsEvaluatedPrMove.Count == 0) return 0;
			int[] list = PositionsEvaluatedPrMove.ToArray();
			Array.Sort(list);
			return list[list.Length/2];
		}

		public int getMaxPositionsEvaluatedPrMove() {
			int result = 0;
			foreach (int i in PositionsEvaluatedPrMove) 
			{
				if (i > result) 
				{
					result = i;
				}
			}

			return result;
		}

		public int getAverageBranchFactor() 
		{
			return (nodes > 0) ? (int) Math.Round(branches/(double) nodes) : 0;
		}

		public void nodeBranched(int size) 
		{
			nodes++;
			branches += size;
		}

		public void CacheHit() 
		{
			cacheHit++;
		}

		public void cutOffAfter(int moveEvaluated) 
		{
			numberOfCutoffs++;
			totalCutoffTurns += moveEvaluated;
		}

		public double getAverageMovesEvaluatedBeforeCutoff() 
		{
			if (totalCutoffTurns == 0) return -1d;
			return totalCutoffTurns/(double) numberOfCutoffs;
		}
	}
}

