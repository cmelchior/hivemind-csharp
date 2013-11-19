using System;
using HiveMind.Model;
using HiveMind.Model.Rules;
using System.Collections.Generic;
using HiveMind.Debug;
using HiveMind.AI.Statistics;

namespace HiveMind.Game
{

	/// General game state class for a game of Hive.
	///
	/// A Zobrist key is maintained for the game state. It consists of zobrist key for the board + game state information.
	/// See <code>Board</code> for further details.
	///
	/// @see [1] Randy Ingersoll: Play Hive like a champion
	/// @see [2] http://en.wikipedia.org/wiki/Zobrist_hashing
	public class Game
	{
		private const bool DEBUG = false; 

		public string Name { get; set; }
		private Board _Board;
		public Board Board { 
			get { 
				if (_Board == null) {
					throw new InvalidOperationException ("Add players before board is available");
				}
				return _Board;
			}
			set { _Board = value; }
		}
		public Player WhitePlayer { get; private set; }
		public Player BlackPlayer { get; private set; }
		private Player _activePlayer;
		public Player ActivePlayer 
		{ 
			get { return _activePlayer; } 
			set {
				if (value != WhitePlayer && value != BlackPlayer) {
					throw new InvalidOperationException("Active player must eiter be white or black: " + value.Name);
				}
				_activePlayer = value;
			}		
		}

		private bool useZobristKey = false;      // If true, a Zobrist key is maintained for the board position and game state.
		public long zobristKey { get; private set; } // Zobrist key for board + game state
		private long[] playerHash = new long[2];

		private bool isRunning = false;      // Game is started and progressing
		public bool manualStepping { get; set; } // If true, continue() must be called after every move to progress the game (for debugging/testing)
		private bool replayMode = false;     // If true, the game cannot progress any futher, but forward(), backwards() can be called to navigate the game. When set to to false, game is forwarded to last position again.
		private int replayIndex = 0;            // Pointer to current move (that has not been played).

		public int TurnLimit { get; set; }             // If above 0, the game ends in a draw after so many moves.
		private GameStatus _status;
		public GameStatus Status {
			get { return _status; }
			set {
				_status = value;
				this.Statistics.SetStatus(value);
			}
		} 

		private List<GameCommand> moves = new List<GameCommand>();

		// Keeping track of draws
		private bool enforceForcedDraw = true;       // If true, game is declared a draw after a set number of repeat moves by each player.
		private int repeatMovesBeforeForcedDraw = 3;    // Number of moves each player must make that are "the same" before forcing a draw.
		private int whiteDuplicateMoves = 0;
		private int blackDuplicateMoves = 0;

		// Debug properties
		public bool printGameStateAfterEachMove { get; set; }
		public GameStatistics Statistics { get; private set; }
		private HiveAsciiPrettyPrinter mapPrinter = new HiveAsciiPrettyPrinter();


		public Game ()
		{
			TurnLimit = -1;
			Statistics = new GameStatistics();
			Status = GameStatus.RESULT_NOT_STARTED;
		}

		public void AddPlayers (Player white, Player black)
		{
			WhitePlayer = white;
			BlackPlayer = black;
			Statistics.WhiteName = white.Name;
			Statistics.BlackName = black.Name;
			Board = new Board(white, black);
		}

		
		/// <summary>
		/// Resets the game and start a new game.
		/// </summary>
		public void Start() {
			if (isRunning) return;
			Board.Clear();
			moves.Clear();
			ActivePlayer = WhitePlayer;

			// Start the game loop
			Statistics.startGame();
			Status = GameStatus.RESULT_MATCH_IN_PROGRESS;
			isRunning = true;

			while (isRunning && !manualStepping) 
			{
				ContinueGame();
			}
		}

		public void SetReplayMode(bool enabled) {
			if (replayMode == enabled) return;

			if (enabled) 
			{
				// Enable replay mode and put replay index at the start of the game
				isRunning = false;
				replayMode = true;
				replayIndex = moves.Count;
				while (replayIndex > 0) 
				{
					Backwards();
				}

			} 
			else 
			{
				// Disable replay mode and forward the game to last move.
				while (replayIndex < moves.Count) 
				{
					Forward();
				}
				isRunning = true;
				replayMode = false;
			}
		}

		/// <summary>
		/// Forward game state and return command that caused the state change.
		/// </summary>
		public GameCommand Forward() 
		{
			if (!replayMode) throw new InvalidOperationException("Replay mode not enabled");
			if (replayIndex == moves.Count) throw new InvalidOperationException("Cannot go forward any further, at end of game.");
			GameCommand command = moves[replayIndex];
			command.Execute(this);
			replayIndex++;
			return command;
		}

		/// <summary>
		/// Rollback game state and return command that caused the state change.
		/// </summary>
		public GameCommand Backwards() 
		{
			if (!replayMode) throw new InvalidOperationException("Replay mode not enabled");
			if (replayIndex == 0) throw new InvalidOperationException("Cannot go back any further, at beginning of game.");
			replayIndex--;
			GameCommand command = moves[replayIndex];
			command.undo(this);
			return command;
		}

		/// <summary>
		/// Move the game forward one step with the given command.
		/// This overrides the CommandProvider if any i set
		/// </summary>
		public void continueGame(GameCommand command) 
		{
			if (!isRunning || replayMode) return;

			// Use CommandProvider if command is not provided
			if (command == null) 
			{
				command = ActivePlayer.CommandProvider(this, Board);
			}

			if (isLegalCommand(command)) 
			{
				ExecuteCommand(command);
				if (printGameStateAfterEachMove) 
				{
					printBoardState(command);
				}

				if (isEndOfGame()) 
				{
					if (DEBUG) Console.Out.WriteLine("Game over!");
					isRunning = false;
					Statistics.stopGame();
				}

			} 
			else 
			{
				throw new InvalidOperationException("Illegal command: " + ActivePlayer.Name + " trying to execute " + command.ToString());
			}
		}

		/// <summary>
		/// Move the game forward one step.
		/// Only call this if manual stepping is enabled.
		/// Doesn't work in replay mode,
		/// </summary>
		public void ContinueGame() 
		{
			continueGame(null);
		}
		/// <summary>
		/// Output the current board state to the screen
		/// </summary>
		private void printBoardState(GameCommand command) 
		{
			Console.Out.WriteLine(getOtherPlayer().Name + ": Turn " + getOtherPlayer().Turns);
			Console.Out.WriteLine("DuplicateMoves: " + whiteDuplicateMoves + " vs. " + blackDuplicateMoves);
			Console.Out.WriteLine(command.ToString());
			mapPrinter.print(Board);
		}

		private void ExecuteCommand(GameCommand command) 
		{
			if (command.Equals(GameCommand.PASS)) 
			{
				Statistics.playerPasses(ActivePlayer);
			} 
			else 
			{
				Statistics.playerMoves(ActivePlayer);
			}

			keepTrackOfDuplicateMoves(command);
			command.Execute(this);
			moves.Add(command);
		}
	
		private int getColorIndex(Player player) 
		{
			return player.IsWhitePlayer() ? 0 : 1;
		}

		/// <summary>
		/// Keeps track of players executing duplicate moves.
		/// Should be called before the move is actually executed.
		/// </summary>
		private void keepTrackOfDuplicateMoves(GameCommand command) {
			if (moves.Count >= 4) 
			{
				String lastMove = moves[moves.Count - 4].getTargetSquareDesc();
				if (command.getTargetSquareDesc() == lastMove) 
				{
					if (ActivePlayer.IsWhitePlayer()) 
					{
						whiteDuplicateMoves++;
					} 
					else 
					{
						blackDuplicateMoves++;
					}
				} 
				else 
				{
					if (ActivePlayer.IsWhitePlayer()) 
					{
						whiteDuplicateMoves = 0;
					} 
					else 
					{
						blackDuplicateMoves = 0;
					}
				}
			}
		}

		private bool isLegalCommand(GameCommand command) 
		{
			if (command == null) {
				throw new InvalidOperationException("Command must not be null");
			}

			// Queen must be played at the 4 turn at the latest
			if (ActivePlayer.Moves == 3) {
				bool queenOnBoard = ActivePlayer.HasPlacedQueen();
				if (!queenOnBoard) {
					bool ok = ActivePlayer.Queen.Equals(command.Token);
					if (!ok) {
						if (DEBUG) Console.Out.WriteLine("Queen must be moved now, was: " + command);
					}
					return ok;
				}
			}

			// Hive must not move before queen has been placed
			if (!ActivePlayer.HasPlacedQueen()) {
				if (command != GameCommand.PASS && command.FromQ != Hex.SUPPLY) {
					if (DEBUG) Console.Out.WriteLine("Queen must be moved before placing queen: " + command);
					return false;
				}
			}

			return true;
		}

		public bool isEndOfGame() 
		{
			bool whiteQueenSurronded = Rules.Rules.GetInstance().isQueenSurrounded(WhitePlayer, Board);
			bool blackQueenSurronded = Rules.Rules.GetInstance().isQueenSurrounded(BlackPlayer, Board);
			bool forcedDraw = isForcedDraw();

			// End game conditions
			if (WhitePlayer.Turns + BlackPlayer.Turns == 2 * TurnLimit && TurnLimit > 0) 
			{
				Status = GameStatus.RESULT_TURN_LIMIT_REACHED;
				if (DEBUG) Console.Out.WriteLine("Move limit reached: " + TurnLimit);
				return true;
			} 
			else if (whiteQueenSurronded && blackQueenSurronded) 
			{
				Status = GameStatus.RESULT_DRAW;
				if (DEBUG) Console.Out.WriteLine("Game ended in a draw!");
				return true;
			} 
			else if (whiteQueenSurronded) 
			{
				Status = GameStatus.RESULT_BLACK_WINS;
				if (DEBUG) Console.Out.WriteLine("Black wins!");
				return true;
			} 
			else if (blackQueenSurronded) 
			{
				Status = GameStatus.RESULT_WHITE_WINS;
				if (DEBUG) Console.Out.WriteLine("White wins");
				return true;
			} 
			else if (forcedDraw) 
			{
				Status = GameStatus.RESULT_DRAW;
				if (DEBUG) Console.Out.WriteLine("Game ended in a draw!");
				return true;
			}

			// Continue game
			return false;
		}

		public void TogglePlayer() 
		{
			if (ActivePlayer == null) 
			{
				ActivePlayer = WhitePlayer;
			} 
			else 
			{
				ActivePlayer = (ActivePlayer == WhitePlayer) ? BlackPlayer : WhitePlayer;
			}
		}


		public Player getOtherPlayer() 
		{
			return (ActivePlayer == WhitePlayer) ? BlackPlayer : WhitePlayer;
		}

		/// <summary>
		/// Return the move for the player at the given turn (for that player)
		/// </summary>
		/// <returns>GameCommand for that move or null if it doesn't exists.</returns>
		/// <param name="player">Black or White player</param>
		/// <param name="turn">1 - GameEnd</param>
		public GameCommand getMove(Player player, int turn) 
		{
			if (turn <= 0) return null;
			int turnIndex = (turn - 1)*2  + (player.IsBlackPlayer() ? 1 : 0);
			return (moves.Count > turnIndex) ? moves[turnIndex] : null;
		}

		
		private bool isForcedDraw() 
		{
			if (!enforceForcedDraw) return false;
			int maxTurns = repeatMovesBeforeForcedDraw;
			return whiteDuplicateMoves >= maxTurns && blackDuplicateMoves >= maxTurns;
		}

		/// Enable the calculation of Zobrist keys for the board. Enabling Zobrist key also enabled Standard Position.
		public void SetStandardPositionMode(StandardPositionMode mode) 
		{
			Board.setStandardPositionMode(mode);
			useZobristKey = (mode != StandardPositionMode.DISABLED);

			// Initialize Game state hashes
			Random random = new Random();
			playerHash[0] = random.NextInt64();
			playerHash[1] = random.NextInt64();
		}

		public void UpdateZobristKey() 
		{
			if (useZobristKey) 
			{
				zobristKey = Board.GetZobristKey() ^ playerHash[getColorIndex(ActivePlayer)];
			}
		}
	}
}