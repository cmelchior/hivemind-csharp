using NUnit.Framework;
using System;
using HiveMind.Debug;
using HiveMind.Model;
using System.Collections.Generic;
using HiveMind.Rules;

namespace HiveMindTest
{
	[TestFixture ()]
	public class SoldierAntTests
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
		public void TestTargetSquares_aroundHive() 
		{
			Board board = new Board(p1, p2);
			Token ant = p1.GetFromSupply(BugType.SOLDIER_ANT);
			Token bee = p2.GetFromSupply(BugType.QUEEN_BEE);
			board.AddToken(ant, 0, 0);
			board.AddToken(bee, 1, 0);

			List<Hex> targets = Rules.GetInstance().GetTargetHexes(ant, board);
			Assert.AreEqual(5, targets.Count);
			Assert.AreEqual(board.GetHex(1,-1), targets[0]);
			Assert.AreEqual(board.GetHex(0,1), targets[4]);
		}

		
		/// White Ant cannot enter the gate and go to Hex A.
		///
		/// | = = = = = = = = = = = = |
		/// |    _ _                  |
		/// |  /# # #\                |
		/// | /# ANT #\ _ _           |
		/// | \# -W- #/+ + +\         |
		/// |  \#_#_#/+ QBE +\ _ _    |
		/// |  /+ + +\+ -B- +/+ + +\  |
		/// | /+ ANT +\+_+_+/+ SPI +\ |
		/// | \+ -B- +/     \+ -B- +/ |
		/// |  \+_+_+/   A   \+_+_+/  |
		/// |  /+ + +\       /+ + +\  |
		/// | /+ BTL +\ _ _ /+ HOP +\ |
		/// | \+ -B- +/     \+ -B- +/ |
		/// |  \+_+_+/       \+_+_+/  |
		/// |                         |
		/// | = = = = = = = = = = = = |
		[Test]
		public void TestTargetSquares_cannotEnterGate() {
			Board board = new Board(p1, p2);
			Token ant = p1.GetFromSupply(BugType.SOLDIER_ANT);
			board.AddToken(ant, 0, 0);
			board.AddToken(p2.GetFromSupply(BugType.QUEEN_BEE), 1, 0);
			board.AddToken(p2.GetFromSupply(BugType.SPIDER), 2, 0);
			board.AddToken(p2.GetFromSupply(BugType.GRASSHOPPER), 2, 1);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 0, 1);
			board.AddToken(p2.GetFromSupply(BugType.BEETLE), 0, 2);

			List<Hex> targets = Rules.GetInstance().GetTargetHexes(ant, board);
			Assert.AreEqual(11, targets.Count);
			Assert.IsFalse(targets.Contains(board.GetHex(1, 1)));
		}

		[Test]
		public void freeToMove_surrounded() {
			Board board = new Board(p1, p2);
			Token ant = p1.GetFromSupply(BugType.SOLDIER_ANT);
			board.AddToken(ant, 0, 0);

			board.AddToken(p2.GetFromSupply(BugType.QUEEN_BEE), 0, -1);
			board.AddToken(p2.GetFromSupply(BugType.SPIDER), 1, 0);
			board.AddToken(p2.GetFromSupply(BugType.GRASSHOPPER), -1, 1);

			Assert.IsFalse(Rules.GetInstance().IsFreeToMove(ant, board));
		}
	}
}