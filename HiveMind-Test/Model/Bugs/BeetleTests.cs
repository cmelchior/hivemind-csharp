using NUnit.Framework;
using System;
using HiveMind.Model;
using HiveMind.Debug;
using System.Collections.Generic;
using HiveMind.Rules;

namespace HiveMindTest
{
	[TestFixture ()]
	public class BeetleTests
	{

		HiveAsciiPrettyPrinter printer;
		Player p1;
		Player p2;

		[SetUp]
		public void Setup() 
		{
			printer = new HiveAsciiPrettyPrinter();
			p1 = new Player("White", PlayerType.WHITE);
			p1.FillBaseSupply();
			p2 = new Player("Black", PlayerType.BLACK);
			p2.FillBaseSupply();
		}

		[Test]
		public void TestTargetSquares_StartingPosition() {
			Board board = new Board(p1, p2);
			Token bee = p1.GetFromSupply(BugType.BEETLE);
			Token ant = p2.GetFromSupply(BugType.SOLDIER_ANT);
			board.AddToken(bee, 0, 0);
			board.AddToken(ant, 1, -1);

			List<Hex> targets = Rules.GetInstance().GetTargetHexes(bee, board);
			Assert.AreEqual(3, targets.Count);
			Assert.AreEqual(board.GetHex(0,-1), targets[0]);
			Assert.AreEqual(board.GetHex(1,-1), targets[1]);
			Assert.AreEqual(board.GetHex(1,0), targets[2]);
		}

		/// <summary>
		/// Testing scenario described here:
		/// http://boardgamegeek.com/wiki/page/Hive_FAQ
		/// </summary>
		[Test]
		public void TestTargetSquares_freedom_to_move_between_gates_blocked() {
			Board board = new Board(p1, p2);
			Token beetle = p1.GetFromSupply(BugType.BEETLE);
			board.AddToken(beetle, 0, 0);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 0, -1);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 0, -1);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 1, -1);
			board.AddToken(p2.GetFromSupply(BugType.SPIDER), 1, 0);
			board.AddToken(p2.GetFromSupply(BugType.SPIDER), 1, 0);

			List<Hex> targets = Rules.GetInstance().GetTargetHexes(beetle, board);
			Assert.AreEqual(4, targets.Count);
			Assert.IsFalse(targets.Contains(board.GetHex(1, -1)));
		}


	/// <summary>
	/// Testing scenario described here:
	/// http://boardgamegeek.com/wiki/page/Hive_FAQ
	/// </summary>
	[Test]
	public void testTargetSquares_freedom_to_move_between_gates() {
		Board board = new Board(p1, p2);
		Token bee = p1.GetFromSupply(BugType.BEETLE);
		board.AddToken(bee, 0, 0);
		board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 0, -1);
		board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 0, -1);
		board.AddToken(p2.GetFromSupply(BugType.GRASSHOPPER), 1, -1);
		board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 1, 0);

		List<Hex> targets = Rules.GetInstance().GetTargetHexes(bee, board);
		Assert.AreEqual(5, targets.Count);
			Assert.IsTrue(targets.Contains(board.GetHex(1, -1)));
	}

	}
}