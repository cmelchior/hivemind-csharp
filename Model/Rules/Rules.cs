using System;
using HiveMind.Model;
using System.Collections.Generic;
using HiveMind.Model.Rules;

namespace HiveMind.Rules
{
	public class Rules
	{

		private static Rules instance;
		private PillBug pillbug;

		private QueenBee bee;
		private Beetle beetle;
		private SoldierAnt ant;
		private Spider spider;
		private Grasshopper grasshopper;
		private LadyBug ladyBug;
		private Mosquito mosquito;
		private UnknownBug unknownBug;

		public static Rules GetInstance() 
		{
			if (instance == null) 
			{
				instance = new Rules();
			}

			return instance;
		}

		private Rules() 
		{
			ant = new SoldierAnt();
			bee = new QueenBee();
			beetle = new Beetle();
			spider = new Spider();
			grasshopper = new Grasshopper();
			ladyBug = new LadyBug();
			mosquito = new Mosquito();
			pillbug = new PillBug();
			unknownBug = new UnknownBug();
		}

		public bool IsFreeToMove(Token token, Board board) 
		{

			// 1. Pick a random neighbor and mark it.
			// 2. Visit all neighbors recursively (ignoring startGame) and mark them
			// 3. When marking is done. Counht marked. Must be equal to hexes - 1 to be a consistent hive
			List<Token> neighbors = board.GetNeighborTokens(token.Hex);
			if (neighbors.Count == 0) return false;

			Token startToken = neighbors[0];
			HashSet<Token> marked = mark(startToken, token, board, new HashSet<Token>());
			if (marked.Count != board.GetFilledHexes().Count - 1) {
				return false;  // Marked set doesn't match whole board -> Disconnected hive
			}

			// Futher restrict based on type (Mosquito mimics other types)
			Bug bug = getBugSpecificRules(token);
			return bug.IsFreeToMove(token, board);
		}

		/// <summary>
		/// Recursively mark all tokens connected to a token.
		/// </summary>
		private HashSet<Token> mark(Token startToken, Token ignore, Board board, HashSet<Token> marked) 
		{
			marked.Add(startToken);

			List<Token> neighbors = board.GetNeighborTokens(startToken.Hex);
			foreach (Token t in neighbors) 
			{
				if (t.Equals(ignore)) continue;
				if (marked.Contains(t)) continue;
				mark(t, ignore, board, marked);
			}

			return marked;
		}

		public List<Hex> GetTargetHexes(Token token, Board board) 
		{
			Bug bug = getBugSpecificRules(token);
			return bug.getTargetHexes(token, board);
		}

		/// <summary>
		/// Get list of hexes a new pieces can be added to.
		/// </summary>
		public List<Hex> getStartHexes(Player player, Board board)  {

			// Start
			List<Hex> result = new List<Hex> ();
			if (board.GetFilledHexes().Count == 0) 
			{
				result.Add(board.GetHex (0, 0));
				return result;
			}

			// Start for 2nd player
			if (board.GetFilledHexes().Count == 1) 
			{
				return board.GetNeighborHexes(board.GetFilledHexes()[0]);
			}

			HashSet<Hex> targets = new HashSet<Hex>();

			// 1. Get all empty neighbor hexes to player tokens.
			List<Hex> hexes = board.GetFilledHexes();
			foreach (Hex hex in hexes) 
			{
				if (hex.SeeTopToken().Player.Equals(player)) 
				{
					List<Hex> neighbors = board.GetNeighborHexes(hex);
					foreach (Hex h in neighbors) 
					{
						if (h.IsEmpty()) 
						{
							targets.Add(h);
						}
					}
				}
			}

			// 2. For each empty hex, check colors of neighbor hexes
			// 3. If only 1 color, add to startGame hex set.
			foreach (Hex hex in targets) 
			{
				List<Hex> neighbors = board.GetNeighborHexes(hex);
				bool isTarget = true;
				foreach (Hex h in neighbors) 
				{
					if (h.IsEmpty()) continue;
					Player p = h.SeeTopToken().Player;
					if (p != player) 
					{
						isTarget = false;
						break; // different token colors, ignore
					}
				}

				if (isTarget) 
				{
					result.Add(hex);
				}
			}
			return result;

		}

		/// <summary>
		/// Returns true if a token can slide between the two hexes.
		/// IF the "One Hive" rule must be maintained during the slide,
		/// either the left or right hex must be filled, but if both are filled
		/// they block instead.
		/// INVARIANT: Hexes are assumed to be neighbors.
		/// </summary>
		/// <returns>True if a token can slide to the new hex.</returns>
		/// <param name="from">Hex to move from.</param>
		/// <param name="to">Hex to move to.</param>
		/// <param name="board">Board.</param>
		public bool CanSlideTo(Hex from, Hex to, Board board) 
		{
			Hex left = board.GetClockwiseHex(from, to, board);
			Hex right = board.GetCounterClockwiseHex(from, to, board);

			if (from.GetHeight() == 1 && to.IsEmpty()) {
				// Slide at ground level
				bool followRightSide = left.IsEmpty() && !right.IsEmpty();
				bool followLeftSide = !left.IsEmpty() && right.IsEmpty();
				return followLeftSide || followRightSide;

			} 
			else if (from.GetHeight() > 1 && to.GetHeight() < from.GetHeight()) 
			{
				// Slide on top of hive
				if (left.GetHeight() > from.GetHeight() && right.GetHeight() > from.GetHeight()) 
				{
					return false;
				} 
				else 
				{
					return true;
				}

			} 
			else 
			{
				return false;
			}
		}

		/// <summary>
		/// Returns true if a token can crawl up on a target token.
		/// </summary>
		public bool CanCrawlUp(Hex from, Hex to, Board board) {
			Hex left = board.GetClockwiseHex(from, to, board);
			Hex right = board.GetCounterClockwiseHex(from, to, board);

			bool isBlocked = (from.GetHeight() - 1) < left.GetHeight()
			                 && (from.GetHeight() - 1) < right.GetHeight()
			                 && to.GetHeight() < left.GetHeight()
			                 && to.GetHeight() < right.GetHeight();

			return !isBlocked;
		}

		/// <summary>
		/// Returns true if a token can crawl down from top the hive to ground level.
		/// </summary>
		/// <returns>True if a token can crawl up on the new hex.</returns>
		/// <param name="from">Hex to move from.</param>
		/// <param name="to">Hex to move to.</param>
		public bool CanCrawlDown(Hex from, Hex to, Board board) {
			if (from.GetHeight() < 2 || to.GetHeight() > 0) return false;

			Hex left = board.GetClockwiseHex(from, to, board);
			Hex right = board.GetCounterClockwiseHex(from, to, board);

			return left.GetHeight() < from.GetHeight() || right.GetHeight() < from.GetHeight();
		}

		/// <summary>
		/// Returns all tokens free to move for the given player
		/// </summary>
		public HashSet<Token> getFreeTokens(Player p, Board board) {
			HashSet<Token> result = new HashSet<Token>();

			List<Hex> hexes = board.GetFilledHexes();
			foreach (Hex hex in hexes) 
			{
				if (hex.SeeTopToken() == null) continue;
				Token token = hex.SeeTopToken();
				if (token.Player.Equals(p)) 
				{
					if (IsFreeToMove(token, board)) {
						result.Add(token);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Returns true if moving a hex from A to B maintains the "One Hive" rule.
		/// Ie. by induction this is always true if any neighbor have a token.
		/// </summary>
		public bool IsOneHiveIntact(Hex a, Hex b, Board board) {
			int i = b.GetHeight(); // Crawling on top of of the hive still maintains the "One Hive" rule
			List<Token> neighbors = board.GetNeighborTokens(b);
			foreach (Token t in neighbors) 
			{
				if (!t.Hex.Equals(a)) 
				{
					i++;
				}
			}

			return i > 0;
		}

		public bool isQueenSurrounded(Player player, Board board) {
			Token queen = player.Queen;
			Hex hex = queen.Hex;
			if (hex != null) 
			{
				return board.GetNeighborTokens(hex).Count == 6;
			} 
			else 
			{
				return false;
			}
		}

		/// <summary>
		/// A bug tries to mimic the movement characteristics of another bug.
		/// Returns true, if successful, false otherwise.
		/// </summary>
		public bool mimicBug(Token copyTo, Token copyFrom, Board board) {
			Bug bug = getBugSpecificRules(copyTo);
			if (!bug.canMimic()) {
				throw new InvalidOperationException(copyTo.OriginalType + " cannot mimic other bugs.");
			}

			// Always mimic BEETLE when atop the hive
			if (copyTo.Hex.GetHeight() > 1) {
				copyTo.mimic(BugType.BEETLE);
				return true;

			} else {
				List<Token> targets = getMimicList(copyTo, board);
				if (!targets.Contains(copyFrom)) return false; // Only allowed to copy from legal bugs
				copyTo.mimic(copyFrom.OriginalType);
				return true;
			}
		}

		/// <summary>
		/// Returns a list of possible targets for mimicing.
		/// When atop the hive, it cannot copy anything (it is forced to be a beetle), and this returns no result
		/// </summary>
		/// <returns>The mimic list.</returns>
		/// <param name="token">Token that wants to mimic nearby bugs.</param>
		/// <param name="board">Current board</param>
		public List<Token> getMimicList(Token token, Board board) {
			if (token.Hex.GetHeight() > 1) {
				return new List<Token>();
			} else {
				return board.GetNeighborTokens(token.Hex);
			}
		}

		public Bug getBugSpecificRules(Token token) {
			BugType type = token.Type;
			if (type == BugType.UNKNOWN) return unknownBug;
			else if (type == BugType.PILL_BUG) return pillbug;
			else if (type == BugType.MOSQUITO) return mosquito;
			else if (type == BugType.LADY_BUG) return ladyBug;
			else if (type == BugType.GRASSHOPPER) return grasshopper;
			else if (type == BugType.SPIDER) return spider;
			else if (type == BugType.SOLDIER_ANT) return ant;
			else if (type == BugType.QUEEN_BEE) return bee;
			else if (type == BugType.BEETLE) return beetle;
			else return unknownBug;
		}	
	}
}

