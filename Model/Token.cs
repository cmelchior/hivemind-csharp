using System;
using HiveMind.Model;

namespace HiveMind.Model
{
	public class Token
	{
		public const string DEFAULT_ID = "?";

		public String Id { get; set; } // ID number = Number added to the board
		public Player Player { get; private set; }
		public BugType OriginalType { get; private set; }
		public BugType Type { get { return (mimics != BugType.UNKNOWN) ? mimics : OriginalType; }} 

		private BugType mimics = BugType.UNKNOWN;
		public Hex Hex { get; set; }

		public Token(Player player, BugType type) {
			Id = DEFAULT_ID; 
			Player = player;
			OriginalType = (type == null) ? BugType.UNKNOWN : type;
		}

		/// <summary>
		/// Set the Bug type this token currently mimics.
		/// If above ground, it can only mimic a beetle.
		/// </summary>
		/// <param name="type">BugType to mimic</param>
		public void mimic(BugType type) 
		{
			if (type == null) 
				mimics = BugType.UNKNOWN;
			else 
				mimics = type;
		}

		/// <summary>
		/// Mimics another token.
		/// </summary>
		/// <param name="otherToken">Other token to mimic.</param>
		public void mimic(Token otherToken) 
		{
			if (otherToken == null)
				mimic(BugType.UNKNOWN);
			else 
				mimic(otherToken.OriginalType);
		}

		public bool InSupply()
		{
			return Hex == null || Hex.Q == Hex.SUPPLY || Hex.R == Hex.SUPPLY;
		}


		public override string ToString ()
		{
			return Player.Name + "("+Id+"): " + OriginalType + (Hex != null ? " " + Hex.ToString() : " (SUPPLY)");
		}
	}
}