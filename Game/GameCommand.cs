using System;
using HiveMind.Model;
using System.Text;

namespace HiveMind.Game
{
	public class GameCommand
	{

		public static readonly GameCommand PASS = new GameCommand(0, 0, 0, 0, null, false);

		public int FromQ { get; private set; }
		public int FromR { get; private set; }
		public int ToQ { get; private set; }
		public int ToR { get; private set; }
		public Token Token { get; private set; }
		public bool MovedByPillbug;

		/// <summary>
		/// Helper constructor: Move a token
		/// </summary>
		public static GameCommand Move(Token token, int toQ, int toR) {
			return new GameCommand(token.Hex.Q, token.Hex.R, toQ, toR, token, false);
		}

		/// <summary>
		/// Helper constructor: Add to board from supply
		/// </summary>
		public static GameCommand AddFromSupply(Token token, int toQ, int toR) {
			return new GameCommand (Hex.SUPPLY, Hex.SUPPLY, toQ, toR, token, false);
		}

		public static GameCommand MoveByPillbug(Token token, int toQ, int toR) {
			return new GameCommand(token.Hex.Q, token.Hex.R, toQ, toR, token, true);
		}


		public GameCommand(int fromQ, int fromR, int toQ, int toR, Token token, bool movedByPillbug) {
			this.FromQ = fromQ;
			this.FromR = fromR;
			this.ToQ = toQ;
			this.ToR = toR;
			this.Token = token;
			this.MovedByPillbug = movedByPillbug;
		}

		public void Execute(Game game) {
			Board board = game.Board;

			if (GameCommand.PASS == this) {
				// Do nothing;
			} else if (FromQ != Hex.SUPPLY) {
				board.MoveToken(FromQ, FromR, ToQ, ToR);
			} else {
				board.AddToken(Token, ToQ, ToR);
			}

			game.ActivePlayer.MovedToken();
			game.TogglePlayer();
			game.UpdateZobristKey();
		}

		public void undo(Game game) {
			Board board = game.Board;

			if (GameCommand.PASS == this) {
				// Do nothing;
			} else if (FromQ != Hex.SUPPLY) {
				board.MoveToken(ToQ, ToR, FromQ, FromR);
			} else {
				board.RemoveToken(ToQ, ToR);
			}

			// Make sure that the old active players gets it turn correctly modified
			// by toggling player first.
			game.TogglePlayer();
			game.ActivePlayer.UndoTokenMoved();
			game.UpdateZobristKey();
		}

		public override String ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append((Token != null) ? Token.ToString() : "null");
			sb.Append(": ");
			if (FromQ == Hex.SUPPLY) {
				sb.Append("SUPPLY");
			} else {
				sb.Append("(" + FromQ + ", " + FromR + ")");
			}

			sb.Append(" -> ");
			sb.Append("(" + ToQ + ", " + ToR + ")");
			return sb.ToString();
		}

		public String getTargetSquareDesc() {
			if (ToQ == Hex.SUPPLY || ToR == Hex.SUPPLY) {
				return "SUPPLY";
			} else {
				return "(" + ToQ + ", " + ToR + ")";
			}
		}

		public override bool Equals(Object o) {
			if (o == null || !(o is GameCommand)) return false;
			GameCommand gc2 = (GameCommand) o;
			if (FromQ != gc2.FromQ) return false;
			if (FromR != gc2.FromR) return false;
			if (ToQ != gc2.ToQ) return false;
			if (ToR != gc2.ToR) return false;
			if (MovedByPillbug != gc2.MovedByPillbug) return false;
			return (Token == null && gc2.Token == null) || Token == gc2.Token;
		}

	}
}

