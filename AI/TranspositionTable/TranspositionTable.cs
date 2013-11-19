using System;
using Lucene.Net.Support;
using HiveMind.Game;
using System.Collections.Generic;

namespace HiveMind.AI.TranspositionTable
{
	/// <summary>
	/// Implementation of a Transposition table for an AlphaBetaMinimax algorithm
	/// @see http://en.wikipedia.org/wiki/Transposition_table
	/// @see http://www.gamedev.net/topic/503234-transposition-table-question/
	/// @see http://homepages.cwi.nl/~paulk/theses/Carolus.pdf
	/// @see https://groups.google.com/forum/#!topic/rec.games.chess.computer/p8GbiiLjp0o
	/// </summary>
	public class TranspositionTable 
	{
		private Dictionary<long, TranspositionTableEntry> table = new Dictionary<long, TranspositionTableEntry>();

		public void addResult(long zobristKey, int value, int depth, int valueType, GameCommand bestMove) 
		{
			TranspositionTableEntry existingEntry; 
			if (!table.TryGetValue(zobristKey, out existingEntry) || depth >= existingEntry.Depth) {
				table[zobristKey] = new TranspositionTableEntry(value, depth, valueType, bestMove);
			}
		}

		/// <summary>
		/// Return the already calculated value for a zobrist key or null if no key exists.
		/// </summary>
		/// <returns>The result.</returns>
		/// <param name="zobristKey">Zobrist key.</param>
		public TranspositionTableEntry getResult(long zobristKey) 
		{
			TranspositionTableEntry entry; 
			if (table.TryGetValue(zobristKey, out entry)) 
			{
				return entry;
			} 
			else 
			{
				return null;
			}
		}
	}
}

