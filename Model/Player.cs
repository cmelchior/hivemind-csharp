using System;
using HiveMind.Game;

/// <summary>
/// Describes a Hive player
/// </summary>
using System.Collections.Generic;


namespace HiveMind.Model
{
	public enum PlayerType { UNKNOWN, BLACK, WHITE };

	/// <summary>
	/// Make the next move based on the current game state.
	/// </summary>
	/// <returns>The next legal move</returns>
	/// <param name="currentState">The current game state.</param>
	/// <param name="board">Easy reference to the board state.</param>
	public delegate GameCommand CommandProviderDelegate (Game.Game currentState, Board board);

	public class Player
	{
		public CommandProviderDelegate CommandProvider { get; set; }
		public string Name { get; private set; }
		public PlayerType Type { get; private set; }
		public int ranking { get; set; }

		public int Moves { get; private set; } 	// Number of moves made in the game
		public int Passes { get; private set; } 	// Number of passes made in the game
		public int Turns { get { return Moves + Passes; } }

		public Token Queen { get; set; }
		private Dictionary<string, Token> bugs = new Dictionary<string, Token>();
		private Dictionary<BugType, int> supplyCreatureCounter = new Dictionary<BugType, int>(); // Easy lookup to determine number of tokens of a specific type in the supply.
		public HashSet<Token> Supply { get; private set; }	// Tokens in the supply
		public long PlayTime { get; private set; } // How much time in ms. has the player used so far

		public Player (string name, PlayerType type)
		{
			Name = name;
			Type = type;
			Supply = new HashSet<Token>();
		}

		public void FillBaseSupply() 
		{
			AddToSupply(BugType.QUEEN_BEE, 1);
			AddToSupply(BugType.SPIDER, 2);
			AddToSupply(BugType.GRASSHOPPER,3);
			AddToSupply(BugType.SOLDIER_ANT, 3);
			AddToSupply(BugType.BEETLE, 2);
		}

		public void UseAllExpansions()
		{
			UseMosquitoExpansion();
			UseLadyBugExpansion();
			UsePillBugExpansion();
		}

		public void UseMosquitoExpansion() 
		{
			AddToSupply(BugType.MOSQUITO, 1);
		}

		public void UseLadyBugExpansion() 
		{
			AddToSupply(BugType.LADY_BUG, 1);
		}

		public void UsePillBugExpansion() 
		{
			AddToSupply(BugType.PILL_BUG, 1);
		}

		/*	*
	     * Add a number of bugs to the supply.
	     */
		public void AddToSupply(BugType type, int count) 
		{
			int currentCount = supplyCreatureCounter.ContainsKey(type) ? supplyCreatureCounter[type] : 0;
			for (int i = 0; i < count; i++) 
			{
				Token t = new Token(this, type);
				t.Id = t.OriginalType.GenerateId(currentCount + 1 + i); // Add ID mimicing the ID scheme used by Boardspace.net
				Supply.Add(t);
				bugs.Add(t.Id, t);
			}

			supplyCreatureCounter.Add(type, currentCount + count);

			// Set queen if needed
			if (Queen == null && type == BugType.QUEEN_BEE) 
			{
				Queen = GetFromSupply(BugType.QUEEN_BEE);
			}
		}

		public void MovedToken() 
		{
			Moves++;
		}

		public void UndoTokenMoved() 
		{
			Moves--;
		}

		/// <summary>
		/// Helper function to explicitly set how many tokens the player has moved already.
		/// </summary>
		public void SetTokensMoved(int tokensMoved) 
		{
			Moves = tokensMoved;
		}

		public bool IsBlackPlayer() 
		{
			return Type == PlayerType.BLACK;
		}

		public bool IsWhitePlayer() 
		{
			return Type == PlayerType.WHITE;
		}

		/// <summary>
		/// Adds a token to the stash.
		/// </summary>
		public void AddToSupply(Token token) 
		{
			Supply.Add(token);
			BugType type = token.Type;
			if (supplyCreatureCounter.ContainsKey(type)) {
				supplyCreatureCounter[type] = supplyCreatureCounter[type] + 1;
			} else {
				supplyCreatureCounter.Add(type, 1);
			}
		}

		public void RemoveFromSupply(Token token) 
		{
			if (Supply.Contains(token)) 
			{
				Supply.Remove(token);
				int count = supplyCreatureCounter[token.OriginalType] - 1;
				supplyCreatureCounter[token.OriginalType] = count;
			} 
			else 
			{
				throw new System.ArgumentException("Supply doesn't contain: " + token);
			}
		}

		/// <summary>
		/// Fetches a token from the supply. It is not removed until removeFromSupply() is called.
		/// An IllegalStateException will be thrown if no such is available.
		/// <returns>The from supply.</returns>
		/// <param name="type">Type.</param>
		public Token GetFromSupply(BugType type) 
		{
			int count = supplyCreatureCounter.ContainsKey(type) ? supplyCreatureCounter[type] : 0;
			if (count > 0) 
			{
				Token result = null;
				foreach (Token token in Supply) 
				{
					if (token.Type == type) 
					{
						result = token;
						break;
					}
				}
				return result;
			}

			throw new System.ArgumentException("No more " + type + " + tokens available");
		}

		/// <summary>
		/// Returns a reference to the bug with the given Id, no matter where it is.
		/// </summary>
		/// <param name="bugId">Bug identifier.</param>
		public Token Get(String bugId) 
		{
			return bugs[bugId.ToUpper()];
		}

		/// <summary>
		/// Returns true if the stash contains more tokens of the given creature type
		/// </summary>
		public bool HaveMoreTokens(BugType type) 
		{
			int count = supplyCreatureCounter.ContainsKey(type) ? supplyCreatureCounter[type] : 0;
			return count > 0;
		}

		public bool HasPlacedQueen() 
		{
			return Queen.Hex != null;
		}

		/// <summary>
		/// Return the number of bugs that make up each side
		/// </summary>
		public int GetNoStartingBugs() {
			return bugs.Count;
		}

		public void addPlayTime(long timeInMs) {
			PlayTime += timeInMs;
		}
	}
}
