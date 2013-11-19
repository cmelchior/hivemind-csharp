using NUnit.Framework;
using System;
using HiveMind.Debug;
using HiveMind.Model;
using HiveMind;
using HiveMind.Game;
using HiveMind.AI;

namespace HiveMindTest
{

	internal class OneDepthHeuristic : BoardValueHeuristic
	{
		private Player p2;
		public OneDepthHeuristic(Player player) {
			p2 = player;
		}

		#region BoardValueHeuristic implementation
		public int calculateBoardValue (HiveMind.Game.Game state)
		{
			// Give queen at 0,1 max priority for player 2
			Hex queenHex = p2.Queen.Hex;
			if (queenHex != null && queenHex.Q == 0 && queenHex.R == 1) {
				return -100;
			} else {
				return 0;
			}
		}
		#endregion
	}

	internal class TwoDepthHeuristic : BoardValueHeuristic 
	{
		public int calculateBoardValue(Game state) {
			// White really wants his queen at (0,1), so black will put his token there.
			Hex whiteQueenHex = state.WhitePlayer.Queen.Hex;
			if (whiteQueenHex.Q == 0 && whiteQueenHex.R == 1) {
				return 100; // White really likes this move
			} else {
				return -25;
			}
		}
	}


	[TestFixture ()]
	public class SimpleMiniMaxAITests
	{

		HiveAsciiPrettyPrinter printer;
		Player p1;
		Player p2;

		[SetUp]
		public void setup() 
		{
			printer = new HiveAsciiPrettyPrinter();
			p1 = new Player("White", PlayerType.WHITE);
			p2 = new Player("Black", PlayerType.BLACK);

			// Very simplistic game
			p1.AddToSupply(BugType.QUEEN_BEE, 1);
			p1.AddToSupply(BugType.SPIDER, 1);
			p2.AddToSupply(BugType.QUEEN_BEE, 1);
			p2.AddToSupply(BugType.SPIDER, 1);
		}

		[Test]
		public void testOneDepth() 
		{
			HiveAI ai = new SimpleMiniMaxAI("OneDepth", new OneDepthHeuristic(p2), 1, 30000);

			p1.CommandProvider = ((currentState, board) => {
				return new GameCommand (Hex.SUPPLY, Hex.SUPPLY, 0, 0, p1.GetFromSupply(BugType.QUEEN_BEE), false);
			});

			p2.CommandProvider = ((currentState, board) => {
				return ai.NextMove(currentState, board);
			});

			// Test placement of first bug once another bug has been placed
			Game game = new Game();
			game.manualStepping = true;
			game.AddPlayers(p1, p2);
			game.TurnLimit = 1;
			game.Start();
			game.ContinueGame(); // Turn 1 (White)
			GameCommand command = ai.NextMove(game, game.Board); // Turn 1 (Black)

			Assert.AreEqual (p2.Queen, command.Token);
			Assert.AreEqual (0, command.ToQ);
			Assert.AreEqual (1, command.ToR); 
		}

		[Test]
		public void testTwoDepth() 
		{
			HiveAI ai = new SimpleMiniMaxAI("TwoDepth", new TwoDepthHeuristic(), 2, 30000);

			p1.CommandProvider = (currentState, board) => {
				int turn = currentState.ActivePlayer.Turns;
				if (turn == 0) {
					return new GameCommand(Hex.SUPPLY, Hex.SUPPLY, 0, 0, p1.GetFromSupply(BugType.QUEEN_BEE), false);
				} else {
					return GameCommand.PASS;
				}

			};

			p2.CommandProvider = ((currentState, board) => {
				return ai.NextMove(currentState, board);
			});


			// Test placement of first bug once another bug has been placed
			Game game = new Game();
			game.SetStandardPositionMode (StandardPositionMode.ENABLED);
			game.manualStepping = true;
			game.AddPlayers(p1, p2);
			game.TurnLimit = 2;
			game.Start();
			game.ContinueGame(); // Turn 1 (White)

			GameCommand command = ai.NextMove(game, game.Board); // Turn 1 (Black)

			Assert.AreEqual(0, command.ToQ);
			Assert.AreEqual(1, command.ToR);
		}

		[Test]
		public void testCanDetectOneTurnWin() 
		{
			HiveAI ai = new SimpleMiniMaxAI("OneDepth", new SimpleHeuristic(), 1, 30000);

			Game game = new Game();
			Player p1 = new Player("White", PlayerType.WHITE); p1.FillBaseSupply();
			Player p2 = new Player("Black", PlayerType.BLACK); p2.FillBaseSupply();

			game.AddPlayers(p1, p2);
			game.TurnLimit = 10;
			game = TestSetups.SureWinInOneTurn(game);

			GameCommand command = ai.NextMove(game, game.Board);
			Assert.AreEqual(1, command.ToQ);
			Assert.AreEqual(1, command.ToR);
		}

		[Test]
		public void testCanDetectWinTurnOne() 
		{
			HiveAI ai = new SimpleMiniMaxAI("SimpleMinMax", new SimpleHeuristic(), 1, 30000);

			Game game = new Game();
			Player p1 = new Player("White", PlayerType.WHITE); p1.FillBaseSupply();
			Player p2 = new Player("Black", PlayerType.BLACK); p2.FillBaseSupply();

			game.AddPlayers(p1, p2);
			game.TurnLimit = 10;
			game = TestSetups.SureWinInOneTurn(game);

			GameCommand command = ai.NextMove(game, game.Board);

			Assert.AreEqual(1, command.ToQ);
			Assert.AreEqual(1, command.ToR);
		}


		[Test]
		public void testCanDetectWinTurnTwo() 
		{
			HiveAI ai = new SimpleMiniMaxAI("SimpleMinMax", new SimpleHeuristic(), 3, 30000);

			Game game = new Game();
			Player p1 = new Player("White", PlayerType.WHITE); p1.FillBaseSupply();
			Player p2 = new Player("Black", PlayerType.BLACK); p2.FillBaseSupply();

			game.AddPlayers(p1, p2);
			game.TurnLimit = 10;
			game = TestSetups.SureWinInTwoTurns(game);

			GameCommand command = ai.NextMove(game, game.Board);

			Assert.AreEqual (2, command.ToQ);
			Assert.AreEqual (1, command.ToR);
		}	
	}
}

