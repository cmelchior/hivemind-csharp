using System;
using System.Collections.Generic;

///
/// Describe a Hex on the board. (0,0) is considered the "center" of the board.
///
namespace HiveMind.Model
{
	public class Hex : IComparable<Hex>
	{
		public const short SUPPLY = short.MaxValue; // Coordinates at this value are considered "outside" the board

		public int Q { get; private set; }
		public int R { get; private set; }

		private Stack<Token> tokens = new Stack<Token>();

		public Hex (int q, int r)
		{
			Q = q;
			R = r;
		}

		public bool IsEmpty()
		{
			return tokens.Count == 0;
		}

		public void AddToken(Token token)
		{
			if (token == null) return; 
			tokens.Push(token);
		}

		public Token RemoveToken()
		{
			return tokens.Pop();
		}

		public Token SeeTopToken() {
			if (tokens.Count == 0) return null;
			return tokens.Peek();
		}

		public int GetHeight() {
			return tokens.Count;
		}

		/// <summary>Return token at given height.</summary>
		/// <param name="height">@param height Height to find token. Ground level is 1</param>
		public Token GetTokenAt(int height) {
			if (tokens.Count < height) {
				return null;
			} else {
				return tokens.ToArray()[height - 1];
			}
		}

		public override string ToString ()
		{
			return string.Format ("({0}, {1})", Q, R);
		}

		#region IComparable implementation
		public int CompareTo (Hex other)
		{
			if (other == null) return 1;
			if (Q == other.Q) 
				return compareInt(R, other.R);
			else 
				return compareInt(Q, other.Q);
		}

		private int compareInt(int a, int b) {
			if (a == b) return 0;
			if (a > b)
				return 1;
			else 
				return -1;
		}
		#endregion
	}
}