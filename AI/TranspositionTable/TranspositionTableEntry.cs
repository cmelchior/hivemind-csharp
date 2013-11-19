using System;
using HiveMind.Game;

namespace HiveMind.AI.TranspositionTable
{
	/// <summary>
	/// Wrapper for transposition table entries.
	/// @see http://chessprogramming.wikispaces.com/Node+Types#ALL
	/// </summary>
	public class TranspositionTableEntry 
	{
		public const int PV_NODE = 0;        // Exact match. For bounds [a,b]: a < x < b
		public const int CUT_NODE = 1;       // Lower bound - Beta cutoff. Fail high. For bounds [a,b]: x >= b
		public const int ALL_NODE = 2;       // No move higher than alpha. Fail low. For bounds [a,b]: x <= a

		public readonly int Value; // Value for the node
		public readonly int Depth; // Search depth
		public readonly int Type;  // Type of value [PV_NODE, CUT_NODE, ALL_NODE]
		public readonly GameCommand Move;

		public TranspositionTableEntry(int value, int depth, int type, GameCommand move) 
		{
			this.Value = value;
			this.Depth = depth;
			this.Type = type;
			this.Move = move;
		}
	}
}

