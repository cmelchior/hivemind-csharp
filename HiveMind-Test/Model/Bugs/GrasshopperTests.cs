using NUnit.Framework;
using System;
using HiveMind.Debug;
using HiveMind.Model;
using System.Collections.Generic;
using HiveMind.Rules;

namespace HiveMindTest
{
	[TestFixture ()]
	public class GrasshopperTests
	{
		HiveAsciiPrettyPrinter printer;
		Player p1;
		Player p2;

		[SetUp]
		public void SetUp() 
		{
			printer = new HiveAsciiPrettyPrinter();
			p1 = new Player("White", PlayerType.WHITE);
			p1.FillBaseSupply();
			p2 = new Player("Black", PlayerType.BLACK);
			p2.FillBaseSupply();
		}

		
		[Test]
		public void TestTargetSquares_overToken() {
			Board board = new Board(p1, p2);
			Token grasshopper = p1.GetFromSupply(BugType.GRASSHOPPER);
			Token bee = p2.GetFromSupply(BugType.QUEEN_BEE);
			board.AddToken(grasshopper, 0, 0);
			board.AddToken(bee, 1, 0);

			List<Hex> targets = Rules.GetInstance().GetTargetHexes(grasshopper, board);
			Assert.AreEqual(1, targets.Count);
			Assert.AreEqual(board.GetHex(2,0), targets[0]);
		}

		
		/// Grasshopper(W) cannot jump over empty spaces.
		///
		/// | = = = = = = = = = = = = |
		/// |           _ _           |
		/// |         /+ + +\         |
		/// |    _ _ /+ QBE +\ _ _    |
		/// |  /+ + +\+ -B- +/+ + +\  |
		/// | /+ ANT +\+_+_+/+ SPI +\ |
		/// | \+ -B- +/     \+ -B- +/ |
		/// |  \+_+_+/       \+_+_+/  |
		/// |  /# # #\       /+ + +\  |
		/// | /# HOP #\     /+ HOP +\ |
		/// | \# -W- #/     \+ -B- +/ |
		/// |  \#_#_#/       \+_+_+/  |
		/// |                         |
		/// | = = = = = = = = = = = = |
		[Test]
		public void testTargetSquares_cannotJumpOverEmptySpaces() {
			Board board = new Board(p1, p2);
			Token ant = p1.GetFromSupply(BugType.GRASSHOPPER);
			board.AddToken(ant, 0, 2);
			board.AddToken(p2.GetFromSupply(BugType.QUEEN_BEE), 1, 0);
			board.AddToken(p2.GetFromSupply(BugType.SPIDER), 2, 0);
			board.AddToken(p2.GetFromSupply(BugType.GRASSHOPPER), 2, 1);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 0, 1);

			List<Hex> targets = Rules.GetInstance().GetTargetHexes(ant, board);
			Assert.AreEqual(1, targets.Count);
			Assert.IsTrue(targets.Contains(board.GetHex(0, 0)));
		}
	}
}
