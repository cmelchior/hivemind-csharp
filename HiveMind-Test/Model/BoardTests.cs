using NUnit.Framework;
using System;
using HiveMind;
using HiveMind.Model;
using System.Diagnostics;
using HiveMind.Game;
using NUnit.Framework.SyntaxHelpers;
using HiveMind.Debug;

namespace HiveMindTest
{
	[TestFixture ()]
	public class BoardTests
	{

		HiveAsciiPrettyPrinter printer;
		Player p1;
		Player p2;

		[SetUp]
		public void Init()
		{
			printer = new HiveAsciiPrettyPrinter();
			p1 = new Player("John", PlayerType.WHITE);
			p1.FillBaseSupply();
			p2 = new Player("Susan", PlayerType.BLACK);
			p2.FillBaseSupply();
		}

		[Test]
		public void TestCreateZobristMap ()
		{
			Board board = new Board(p1, p2);
			long start = Environment.TickCount;
			board.setStandardPositionMode(StandardPositionMode.ENABLED);
			Console.Out.WriteLine("Creating board in: " + (Environment.TickCount - start));
		}

		[Test]
		public void TestClockwiseNeighbor() 
		{
			Board board = new Board(p1, p2);
			Hex hex = board.GetClockwiseHex(new Hex(0,0), new Hex(1, -1), board);

			Assert.AreEqual(1, hex.Q);
			Assert.AreEqual(0, hex.R);
		} 

		[Test]
		public void TestCounterClockwiseNeighbor() 
		{
			Board board = new Board(p1, p2);
			Hex hex = board.GetCounterClockwiseHex(new Hex(0,0), new Hex(1, -1), board);

			Assert.AreEqual(0, hex.Q);
			Assert.AreEqual(-1, hex.R);
		}

		[Test]
		public void TestHexEquals() {
			Board board = new Board(p1, p2);
			Token bee = p1.GetFromSupply(BugType.QUEEN_BEE);
			board.AddToken(bee, 1, -1);
			Assert.AreEqual(bee.Hex, board.GetHex(1, -1));
		}

		[Test]
		public void TestNeighborTokens() {
			Game game = new Game();
			Player p1 = new Player("White", PlayerType.WHITE); p1.FillBaseSupply();
			Player p2 = new Player("Black", PlayerType.BLACK); p2.FillBaseSupply();

			game.AddPlayers(p1, p2);
			game.TurnLimit = 10;
			game = TestSetups.SureWinInOneTurn(game);
			Assert.AreEqual(6, game.Board.GetNeighborTokens(game.Board.GetHex(0,3)).Count);
		}

		[Test]
		public void TestStandardPosition_rotate2ndToken() {
			Game game = new Game();
			game.AddPlayers(p1, p2);

			Board board = game.Board;
			board.setStandardPositionMode(StandardPositionMode.ENABLED);

			Token grasshopper = p1.GetFromSupply(BugType.GRASSHOPPER);
			Token bee = p2.GetFromSupply(BugType.QUEEN_BEE);

			board.AddToken(grasshopper, 1, 1);
			board.AddToken(bee, 1, 2);

			int[] grasshopperCoords = board.GetSPCoordinatesFor(grasshopper.Hex);
			int[] beeCoords = board.GetSPCoordinatesFor(bee.Hex);

			Assert.That(new int[] { 0, 0 }, Is.EqualTo (grasshopperCoords));
			Assert.That(new int[] { 1, 0 }, Is.EqualTo (beeCoords)); 
		}


		[Test]
		public void TestStandardPosition_rotate2ndToken_fromOrigin() {
			Game game = new Game();
			game.AddPlayers(p1, p2);

			Board board = game.Board;
			board.setStandardPositionMode(StandardPositionMode.LIMITED);

			Token grasshopper = p1.GetFromSupply(BugType.GRASSHOPPER);
			Token spider = p2.GetFromSupply(BugType.SPIDER);

			board.AddToken(grasshopper, 0, 0);
			board.AddToken(spider, 1, -1);

			int[] grasshopperCoords = board.GetSPCoordinatesFor(grasshopper.Hex);
			int[] spiderCoords = board.GetSPCoordinatesFor(spider.Hex);

			Assert.That(new int[] { 0, 0 }, Is.EqualTo (grasshopperCoords));
			Assert.That(new int[] { 1, 0 }, Is.EqualTo (spiderCoords)); 
		}

		[Test]
		public void TestStandardPosition_flip3rdToken() {
			Game game = new Game();
			game.AddPlayers(p1, p2);
	
			Board board = game.Board;
			board.setStandardPositionMode(StandardPositionMode.ENABLED);
	
			Token grasshopper = p1.GetFromSupply(BugType.GRASSHOPPER);
			Token blackBee = p2.GetFromSupply(BugType.QUEEN_BEE);
			Token spider = p1.GetFromSupply(BugType.SPIDER);
	
			board.AddToken(grasshopper, 1, 1);
			board.AddToken(blackBee, 1, 2);
			board.AddToken(spider, 0, 1);
	
			int[] spiderCoords = board.GetSPCoordinatesFor(spider.Hex);
			Assert.That(new int[] { 0, -1 }, Is.EqualTo(spiderCoords));
		}

		[Test]
		public void TestStandardPosition_flip4thToken() {
			Game game = new Game();
			game.AddPlayers(p1, p2);
	
			Board board = game.Board;
			board.setStandardPositionMode(StandardPositionMode.ENABLED);
	
			Token grasshopper = p1.GetFromSupply(BugType.GRASSHOPPER);
			Token beetle = p2.GetFromSupply(BugType.BEETLE);
			Token whiteQueen = p1.GetFromSupply(BugType.QUEEN_BEE);
			Token blackQueen = p2.GetFromSupply(BugType.QUEEN_BEE);
	
			board.AddToken(grasshopper, 0, 0);
			p1.MovedToken();
			board.AddToken(beetle, 1, 0);
			p2.MovedToken();
			board.AddToken(whiteQueen, -1, 0);
			p1.MovedToken();
			board.AddToken(blackQueen,1,1);
			p2.MovedToken();
	
			int[] blackQueenCoords = board.GetSPCoordinatesFor(blackQueen.Hex);
			Assert.That(new int[] { 3, -2 }, Is.EqualTo(blackQueenCoords));
		}

		[Test]
		public void TestStandardPosition_zOpening() {
			Game game = new Game();
			game.AddPlayers(p1, p2);

			Board board = game.Board;
			board.setStandardPositionMode(StandardPositionMode.ENABLED);

			Token gh1 = p1.GetFromSupply(BugType.GRASSHOPPER);
			Token gh2 = p2.GetFromSupply(BugType.GRASSHOPPER);
			Token whiteQueen = p1.GetFromSupply(BugType.QUEEN_BEE);
			Token blackQueen = p2.GetFromSupply(BugType.QUEEN_BEE);

			board.AddToken(gh1, 0, 0);
			board.AddToken(gh2, 1, 0);
			board.AddToken(whiteQueen, -1, 1);
			board.AddToken(blackQueen,2,-1);

			int[] blackQueenCoords = board.GetSPCoordinatesFor(blackQueen.Hex);
			Assert.That(new int[] { 3, -2 }, Is.EqualTo(blackQueenCoords));
		}

	
		[Test]
		public void TestStandardPosition_reverse() {
			Game game = new Game();
			game.AddPlayers(p1, p2);
	
			Board board = game.Board;
			board.setStandardPositionMode(StandardPositionMode.ENABLED);
	
			Token gh1 = p1.GetFromSupply(BugType.GRASSHOPPER);
			Token gh2 = p2.GetFromSupply(BugType.GRASSHOPPER);
			Token s1 = p1.GetFromSupply(BugType.SPIDER);
			Token s2 = p2.GetFromSupply(BugType.SPIDER);
			Token whiteQueen = p1.GetFromSupply(BugType.QUEEN_BEE);
			Token blackQueen = p2.GetFromSupply(BugType.QUEEN_BEE);
	
			board.AddToken(gh1, 0, 0);
			board.AddToken(gh2, 1, 0);
			board.AddToken(s1, -1, 0);
			board.AddToken(s2, 2, 0);
			board.AddToken(whiteQueen, 0, -1);
			board.AddToken(blackQueen,1,1);
	
			int[] blackQueenCoords = board.GetSPCoordinatesFor(blackQueen.Hex);
			Hex originalHex = board.GetHexForStandardPosition(blackQueenCoords[0], blackQueenCoords[1]);
			Assert.AreEqual(blackQueen, originalHex.SeeTopToken());
		}


		[Test]
		public void TestZobristKey_empty() {
			Board board = new Board(p1, p2);
			board.setStandardPositionMode(StandardPositionMode.ENABLED);
			Assert.AreEqual(0, board.GetZobristKey());
		}

		
		[Test]
		public void TestZobristKey_playerState() {
			Board board = new Board(p1, p2);
			Token whiteBee = p1.GetFromSupply(BugType.QUEEN_BEE);
			Token blackBee = p2.GetFromSupply(BugType.QUEEN_BEE);

			board.AddToken(whiteBee, 0, 0);
			board.AddToken(blackBee, 0, 1);
			long firstKey = board.GetZobristKey();

			board.RemoveToken(0, 1);
			board.RemoveToken(0, 0);
			board.AddToken(blackBee, 0, 1);
			board.AddToken(whiteBee, 0, 0);

		 	// Last player move is encoded into the key. Similar
	        // board layouts are not the same if next player differs.
			Assert.AreEqual(firstKey, board.GetZobristKey());
	    }

		[Test]
		public void TestZobristKey_boardState() {
			Board board = new Board(p1, p2);
			board.setStandardPositionMode(StandardPositionMode.ENABLED);
			Token whiteBee = p1.GetFromSupply(BugType.QUEEN_BEE);
			Token whiteSoldier = p1.GetFromSupply(BugType.SOLDIER_ANT);
			Token blackBee = p2.GetFromSupply(BugType.QUEEN_BEE);
			Token blackSoldier = p2.GetFromSupply(BugType.SOLDIER_ANT);
	
			board.AddToken(whiteBee, 0, 0);
			board.AddToken(blackBee, 0, 1);
			board.AddToken(whiteSoldier, 1, 0);
			board.AddToken(blackSoldier, 1, 1);
	
			long firstKey = board.GetZobristKey();
	
			board.RemoveToken(1, 1);
			board.RemoveToken(1, 0);
			board.RemoveToken(0, 1);
			board.RemoveToken(0, 0);
	
			board.AddToken(whiteBee, 0, 0);
			board.AddToken(blackSoldier, 1, 1);
			board.AddToken(whiteSoldier, 1, 0);
			board.AddToken(blackBee, 0, 1);
	
			// If board state is the same and next player also. It doesn't matter how that state was reached.
			Assert.AreEqual(firstKey, board.GetZobristKey());
		}

	
		[Test]
		public void TestZobristKey_sameForStandardPositionForMultiplePositions() {
			Board board = new Board(p1, p2);
			board.setStandardPositionMode(StandardPositionMode.ENABLED);
			Token whiteBee = p1.GetFromSupply(BugType.QUEEN_BEE);
			Token whiteSoldier = p1.GetFromSupply(BugType.SOLDIER_ANT);
			Token blackBee = p2.GetFromSupply(BugType.QUEEN_BEE);
			Token blackSoldier = p2.GetFromSupply(BugType.SOLDIER_ANT);
	
			board.AddToken(whiteBee, 0, 0);
			board.AddToken(blackBee, 1, 0);
			board.AddToken(whiteSoldier, 2, 0);
			board.AddToken(blackSoldier, 3, -1);
	
			long firstKey = board.GetZobristKey();
	
			board.RemoveToken(0, 0);
			board.RemoveToken(1, 0);
			board.RemoveToken(2, 0);
			board.RemoveToken(3, -1);
	
			board.AddToken(whiteBee, 0, 0);
			board.AddToken(blackBee, -1, 0);
			board.AddToken(whiteSoldier, -2, 0);
			board.AddToken(blackSoldier, -3, 1);
	
			// If the board maintain Standard Position the Zobrist keys should be equal.
			Assert.AreEqual(firstKey, board.GetZobristKey());
		}


		[Test]
		public void TestGetHexForStandardPosition() {
			Game game = new Game();
			game.AddPlayers(p1, p2);

			Board board = game.Board;
			board.setStandardPositionMode(StandardPositionMode.ENABLED);

			Token grasshopper = p1.GetFromSupply(BugType.GRASSHOPPER);
			Token bee = p2.GetFromSupply(BugType.QUEEN_BEE);

			board.AddToken(grasshopper, 0, 0);
			board.AddToken(bee, 1, -1);

			int[] grasshopperCoords = board.GetSPCoordinatesFor(grasshopper.Hex);
			int[] beeCoords = board.GetSPCoordinatesFor(bee.Hex);

			Assert.That(new int[] { 0, 0 }, Is.EqualTo(grasshopperCoords));
			Assert.That(new int[] { 1, 0 }, Is.EqualTo(beeCoords));
		}

		
		[Test]
		public void TestOpeningStandardPosition() {
			Game game = new Game();
			p1.UseAllExpansions();
			p2.UseAllExpansions();
			game.AddPlayers(p1, p2);

			Board board = game.Board;
			board.setStandardPositionMode(StandardPositionMode.ENABLED);

			Token ladybug = p1.GetFromSupply(BugType.LADY_BUG);
			Token mosquito = p2.GetFromSupply(BugType.MOSQUITO);
			Token spider = p1.GetFromSupply(BugType.SPIDER);
			Token grasshopper = p2.GetFromSupply(BugType.GRASSHOPPER);

			board.AddToken(ladybug, 0, 0);
			board.AddToken(mosquito, -1, 1);
			board.AddToken(spider, 0, -1);
			board.AddToken(grasshopper, -2, 2);

			Assert.That(new int[] { 1, 0 }, Is.EqualTo(board.GetSPCoordinatesFor(mosquito.Hex)));
			Assert.That(new int[] { 0, -1 }, Is.EqualTo(board.GetSPCoordinatesFor(spider.Hex)));
		}
	}
}
