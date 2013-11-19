using NUnit.Framework;
using System;
using HiveMind.Debug;
using HiveMind.Model;
using System.Collections.Generic;
using HiveMind.Rules;

namespace HiveMindTest
{
	[TestFixture ()]
	public class LadyBugTests
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
			p1.UseLadyBugExpansion();
			p2 = new Player("Black", PlayerType.BLACK);
			p2.FillBaseSupply();
			p2.UseLadyBugExpansion();
		}

		[Test]
		public void TestTargetSquares_noMoves() 
		{
			Board board = new Board(p1, p2);
			Token ant = p1.GetFromSupply(BugType.LADY_BUG);
			Token bee = p2.GetFromSupply(BugType.QUEEN_BEE);
			board.AddToken(ant, 0, 0);
			board.AddToken(bee, 1, 0);

			List<Hex> targets = Rules.GetInstance().GetTargetHexes(ant, board);
			Assert.AreEqual(0, targets.Count);
		}

		
		/// LadyBug has a complex movement pattern.
		/// Following assumptions:
		///  - A LadyBug can either "Slide" or "Crawl" on top of a Hive
		///  - A LadyBug cannot end her move where she started.
		///
		/// | = = = = = = = = = = = = |
		/// |           _ _           |
		/// |         /# # #\         |
		/// |    _ _ /# LDY #\        |
		/// |  /+ + +\# -W- #/        |
		/// | /+ ANT +\#_#_#/         |
		/// | \+ -B- +/+ + +\         |
		/// |  \+_+_+/+ SPI +\ _ _    |
		/// |  /+ + +\+ -B- +/+ + +\  |
		/// | /+ BTL +\+_+_+/+ HOP +\ |
		/// | \+ -B- +/     \+ -B- +/ |
		/// |  \+_+_+/       \+_+_+/  |
		/// |  /+ + +\                |
		/// | /+ QBE +\               |
		/// | \+ -B- +/               |
		/// |  \+_+_+/                |
		/// |                         |
		/// | = = = = = = = = = = = = |
		[Test]
		public void TestTargetSquares_largeHive() 
		{
			Board board = new Board(p1, p2);
			Token ant = p1.GetFromSupply(BugType.LADY_BUG);
			board.AddToken(ant, 1, 0);
			board.AddToken(p2.GetFromSupply(BugType.QUEEN_BEE), 0, 3);
			board.AddToken(p2.GetFromSupply(BugType.SPIDER), 1, 1);
			board.AddToken(p2.GetFromSupply(BugType.GRASSHOPPER), 2, 1);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 0, 1);
			board.AddToken(p2.GetFromSupply(BugType.BEETLE), 0, 2);

			List<Hex> targets = Rules.GetInstance().GetTargetHexes(ant, board);
			Assert.AreEqual(9, targets.Count);
			Assert.IsFalse(targets.Contains(board.GetHex(0, 4)));
			Assert.IsFalse(targets.Contains(board.GetHex(1, 3)));
		}

		/// A LadyBug can either crawl or slide when on top of the hive.
		/// When faced with a 2 stack, she just climbs up instead of sliding.
		///
		/// | = = = = = = = = = = = = |
		/// |           _ _           |
		/// |         /# # #\         |
		/// |    _ _ /# LDY #\        |
		/// |  /+ + +\# -W- #/        |
		/// | /+ ANT +\#_#_#/         |
		/// | \+ -B- +/+ + +\         |
		/// |  \+_+_+/+ SPI +\ _ _    |
		/// |  /+ + +\+ -B- +/+ + +\  |
		/// | /+ 2xB +\+_+_+/+ HOP +\ |
		/// | \+ -B- +/     \+ -B- +/ |
		/// |  \+_+_+/       \+_+_+/  |
		/// |  /+ + +\                |
		/// | /+ QBE +\               |
		/// | \+ -B- +/               |
		/// |  \+_+_+/                |
		/// |                         |
		/// | = = = = = = = = = = = = |
		[Test]
		public void TestTargetSquares_movingOnTop() 
		{
			Board board = new Board(p1, p2);
			Token ant = p1.GetFromSupply(BugType.LADY_BUG);
			board.AddToken(ant, 1, 0);
			board.AddToken(p2.GetFromSupply(BugType.QUEEN_BEE), 0, 3);
			board.AddToken(p2.GetFromSupply(BugType.SPIDER), 1, 1);
			board.AddToken(p2.GetFromSupply(BugType.GRASSHOPPER), 2, 1);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 0, 1);
			board.AddToken(p2.GetFromSupply(BugType.BEETLE), 0, 2);
			board.AddToken(p2.GetFromSupply(BugType.BEETLE), 0, 2);

			List<Hex> targets = Rules.GetInstance().GetTargetHexes(ant, board);
			Assert.AreEqual(9, targets.Count);
			Assert.IsFalse(targets.Contains(board.GetHex(0, 4)));
			Assert.IsFalse(targets.Contains(board.GetHex(1, 3)));
		}
	}
}
