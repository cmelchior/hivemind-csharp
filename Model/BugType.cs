using System;
using System.Collections.Generic;

/// <summary>
/// Hive token types.
/// </summary>
namespace HiveMind.Model
{
	public class BugType
	{
		public static readonly BugType UNKNOWN = new BugType("?"); 
		public static readonly BugType QUEEN_BEE = new BugType("Q");
		public static readonly BugType BEETLE = new BugType("B");
		public static readonly BugType GRASSHOPPER = new BugType ("G");
		public static readonly BugType SPIDER = new BugType("S");
		public static readonly BugType SOLDIER_ANT = new BugType("A");
		public static readonly BugType MOSQUITO = new BugType("M");
		public static readonly BugType LADY_BUG = new BugType("L");
		public static readonly BugType PILL_BUG = new BugType("P");

		public static IEnumerable<BugType> Values
		{
			get {
				yield return UNKNOWN;
				yield return QUEEN_BEE;
				yield return BEETLE;
				yield return GRASSHOPPER;
				yield return SPIDER;
				yield return SOLDIER_ANT;
				yield return MOSQUITO;
				yield return LADY_BUG;
				yield return PILL_BUG;
			}
		}

		private string boardspaceKey;

		BugType(string boardspaceKey)
		{
			this.boardspaceKey = boardspaceKey;
		}

		/// <summary>
		/// Generate a unique token ID that corrosponds to the ID system used by Boardspace.net
		/// </summary>
		public String GenerateId(int number) {
			if (this == QUEEN_BEE || this == PILL_BUG)
				return boardspaceKey;
			else
				return boardspaceKey + number;
		}
	}
}