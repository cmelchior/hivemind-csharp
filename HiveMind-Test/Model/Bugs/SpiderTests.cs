using NUnit.Framework;
using System;
using HiveMind.Debug;
using HiveMind.Model;
using System.Collections.Generic;
using HiveMind.Rules;

namespace HiveMindTest
{
	[TestFixture ()]
	public class SpiderTests
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
		public void TestTargetSquares_aroundHive() {
			Board board = new Board(p1, p2);
			Token ant = p1.GetFromSupply(BugType.SPIDER);
			Token bee = p2.GetFromSupply(BugType.QUEEN_BEE);
			board.AddToken(ant, 0, 0);
			board.AddToken(bee, 1, 0);

			List<Hex> targets = Rules.GetInstance().GetTargetHexes(ant, board);
			Assert.AreEqual(1, targets.Count);
			Assert.AreEqual(board.GetHex(2,0), targets[0]);
		}

		
		/// Spider cannot enter the gate and go to Hex A, so doesn't count it when finding target hexes.
		///
		/// | = = = = = = = = = = = = |
		/// |           _ _           |
		/// |         /+ + +\         |
		/// |    _ _ /+ QBE +\ _ _    |
		/// |  /+ + +\+ -B- +/+ + +\  |
		/// | /+ ANT +\+_+_+/+ SPI +\ |
		/// | \+ -B- +/     \+ -B- +/ |
		/// |  \+_+_+/       \+_+_+/  |
		/// |  /+ + +\       /+ + +\  |
		/// | /+ BTL +\     /+ HOP +\ |
		/// | \+ -B- +/     \+ -B- +/ |
		/// |  \+_+_+/       \+_+_+/  |
		/// |  /# # #\                |
		/// | /# SPI #\               |
		/// | \# -W- #/               |
		/// |  \#_#_#/                |
		/// |                         |
		/// | = = = = = = = = = = = = |
		[Test]
		public void TestTargetSquares_CannotEnterGate() {
			Board board = new Board(p1, p2);
			Token ant = p1.GetFromSupply(BugType.SPIDER);
			board.AddToken(ant, 0, 3);
			board.AddToken(p2.GetFromSupply(BugType.QUEEN_BEE), 1, 0);
			board.AddToken(p2.GetFromSupply(BugType.SPIDER), 2, 0);
			board.AddToken(p2.GetFromSupply(BugType.GRASSHOPPER), 2, 1);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 0, 1);
			board.AddToken(p2.GetFromSupply(BugType.BEETLE), 0, 2);

			List<Hex> targets = Rules.GetInstance().GetTargetHexes(ant, board);
			Assert.AreEqual(2, targets.Count);
			Assert.IsTrue(targets.Contains(board.GetHex(-1, 1)));
			Assert.IsTrue(targets.Contains(board.GetHex(3, 1)));
		}
	}
}
