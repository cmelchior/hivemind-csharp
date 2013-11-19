using NUnit.Framework;
using System;
using HiveMind.Debug;
using HiveMind.Model;
using HiveMind.Rules;
using System.Collections.Generic;

namespace HiveMindTest
{
	[TestFixture ()]
	public class RulesTests
	{

		HiveAsciiPrettyPrinter printer;
		Player p1;
		Player p2;

		[SetUp]
		public void Setup() {
			printer = new HiveAsciiPrettyPrinter();
			p1 = new Player("White", PlayerType.WHITE);
			p2 = new Player("Black", PlayerType.BLACK);

			p1.FillBaseSupply();
			p1.UseAllExpansions();
			p2.FillBaseSupply();
			p2.UseAllExpansions();
		}

		[Test]
		public void CanSlideTo_NoSpace() 
		{
			Board board = new Board(p1, p2);
			board.AddToken(p1.GetFromSupply(BugType.QUEEN_BEE), 0, 0);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 1, -1);

			Assert.IsFalse(Rules.GetInstance().CanSlideTo(board.GetHex(0,0), board.GetHex(1,-1), board));
		}

		[Test]
		public void CanSlideTo_Success() 
		{
			Board board = new Board(p1, p2);
			board.AddToken(p1.GetFromSupply(BugType.QUEEN_BEE), 0, 0);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 0, -1);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 0, 1);

			Assert.IsTrue(Rules.GetInstance().CanSlideTo(board.GetHex(0,0), board.GetHex(1,-1), board));
		}

		[Test]
		public void CanSlideTo_Blocked() 
		{
			Board board = new Board(p1, p2);
			board.AddToken(p1.GetFromSupply(BugType.QUEEN_BEE), 0, 0);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 0, -1);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 1, 0);

			Assert.IsFalse(Rules.GetInstance().CanSlideTo(board.GetHex(0,0), board.GetHex(1,-1), board));
		}

		[Test]
		public void CanCrawlDown_Success() 
		{
			Board board = new Board(p1, p2);
			board.AddToken(p1.GetFromSupply(BugType.QUEEN_BEE), 0, -1);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 0, -1);

			Assert.IsTrue(Rules.GetInstance().CanCrawlDown(board.GetHex(0, -1), board.GetHex(0, 0), board));
		}


	    /// Queen Bee cannot move to Hex A, as the One Hive rule is broken during the move
		///
		/// | = = = = = = = = = = = = |
		/// |           _ _           |
		/// |         /+ + +\         |
		/// |    _ _ /+ ANT +\ _ _    |
		/// |  /# # #\+ -B- +/# # #\  |
		/// | /# QBE #\+_+_+/# BTL #\ |
		/// | \# -W- #/     \# -W- #/ |
		/// |  \#_#_#/       \#_#_#/  |
		/// |  /     \       /+ + +\  |
		/// | /   A   \ _ _ /+ ANT +\ |
		/// | \       /+ + +\+ -B- +/ |
		/// |  \ _ _ /+ HOP +\+_+_+/  |
		/// |        \+ -B- +/        |
		/// |         \+_+_+/         |
		/// |                         |
		/// | = = = = = = = = = = = = |
		[Test]
		public void CanSlideTo_OneHiveBrokenDuringMove() 
		{
			Board board = new Board(p1, p2);
			board.AddToken(p1.GetFromSupply(BugType.QUEEN_BEE), 0, 0);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 1, -1);
			board.AddToken(p2.GetFromSupply(BugType.BEETLE), 2, -1);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 2, 0);
			board.AddToken(p2.GetFromSupply(BugType.GRASSHOPPER), 1, 1);

			Assert.IsFalse(Rules.GetInstance().CanSlideTo(board.GetHex(0, 0), board.GetHex(0, 1), board));
		}

		/// <summary>
		/// Trying to slide a empty hex to a occupied hex
		/// </summary>
		[Test]
		public void CanSlideTo_IllegalArguments() 
		{
			Board board = new Board(p1, p2);
			board.AddToken(p1.GetFromSupply(BugType.QUEEN_BEE), 0, 0);

			Assert.IsFalse(Rules.GetInstance().CanSlideTo(board.GetHex(0, 1), board.GetHex(0, 0), board));
		}

		[Test]
		public void CanSlideOnTopOfHive() 
		{
			Board board = new Board(p1, p2);
			board.AddToken(p1.GetFromSupply(BugType.QUEEN_BEE), 0, 0);
			board.AddToken(p1.GetFromSupply(BugType.LADY_BUG), 0, 0);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 1, 0);

			Assert.IsTrue(Rules.GetInstance().CanSlideTo(board.GetHex(0, 0), board.GetHex(1, 0), board));
		}

		/// <summary>
		/// Cannot slide to a filled hex (can crawl up though)
		/// </summary>
		[Test]
		public void CanSlideOnTopOfHive_BlockedSinglePiece() {
			Board board = new Board(p1, p2);
			board.AddToken(p1.GetFromSupply(BugType.QUEEN_BEE), 0, 0);
			board.AddToken(p1.GetFromSupply(BugType.LADY_BUG), 0, 0);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 1, 0);
			board.AddToken(p2.GetFromSupply(BugType.BEETLE), 1, 0);

			Assert.IsFalse(Rules.GetInstance().CanSlideTo(board.GetHex(0, 0), board.GetHex(1, 0), board));
		}

		[Test]
		public void CanSlideOnTopOfHive_BlockedGuards() 
		{
			Board board = new Board(p1, p2);

			board.AddToken(p1.GetFromSupply(BugType.QUEEN_BEE), 0, 0);
			board.AddToken(p1.GetFromSupply(BugType.LADY_BUG), 0, 0);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 1, -1);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 1, -1);
			board.AddToken(p1.GetFromSupply(BugType.SOLDIER_ANT), 1, 0);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 0, 1);
			board.AddToken(p2.GetFromSupply(BugType.BEETLE), 0, 1);

			Assert.IsFalse(Rules.GetInstance().CanCrawlDown(board.GetHex(0, 0), board.GetHex(1, 0), board));
		}

		[Test]
		public void CanCrawlDown_Blocked() 
		{
			Board board = new Board(p1, p2);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 1, -1);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 1, -1);
			board.AddToken(p1.GetFromSupply(BugType.SOLDIER_ANT), 1, 0);
			board.AddToken(p1.GetFromSupply(BugType.QUEEN_BEE), 1, 0);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 0, 1);
			board.AddToken(p2.GetFromSupply(BugType.BEETLE), 0, 1);

			Assert.IsFalse(Rules.GetInstance().CanCrawlDown(board.GetHex(1, 0), board.GetHex(0, 0), board));
		}

		/// <summary>
		/// Crawl from ground to top of hive
		/// </summary>
		[Test]
		public void CanCrawlTo_Success() {
			Board board = new Board(p1, p2);
			board.AddToken(p1.GetFromSupply(BugType.BEETLE), 0, 0);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 1, -1);

			Assert.IsTrue(Rules.GetInstance().CanCrawlUp(board.GetHex(0, 0), board.GetHex(1, -1), board));
		}

		/// <summary>
		/// Crawl from ground to top of hive, but blocked by guarding towers.
		/// </summary>
		[Test]
		public void CanCrawlTo_BlockedHigh() {
			Board board = new Board(p1, p2);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 0, -1);
			board.AddToken(p2.GetFromSupply(BugType.BEETLE), 0, -1);
			board.AddToken(p1.GetFromSupply(BugType.BEETLE), 0, 0);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 1, -1);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 1, 0);
			board.AddToken(p2.GetFromSupply(BugType.BEETLE), 1, 0);

			Assert.IsFalse(Rules.GetInstance().CanSlideTo(board.GetHex(0, 0), board.GetHex(1, -1), board));
		}

		/// <summary>
		/// Crawl between two guards (effectively sliding).
		/// </summary>
		[Test]
		public void CanCrawlTo_BlockedLow() {
			Board board = new Board(p1, p2);
			board.AddToken(p1.GetFromSupply(BugType.BEETLE), 0, 0);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 0, -1);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 1, 0);

			Assert.IsFalse(Rules.GetInstance().CanCrawlUp(board.GetHex(0, 0), board.GetHex(1, -1), board));
		}

		[Test]
		public void getMimicList_onGround() {
			Board board = new Board(p1, p2);
			Token mos1 = p1.GetFromSupply(BugType.MOSQUITO);
			Token bee = p2.GetFromSupply(BugType.QUEEN_BEE);
			mos1.mimic(bee);

			board.AddToken(mos1, 0, 0);
			board.AddToken(bee, 1, 0);

			List<Token> result = Rules.GetInstance().getMimicList(mos1, board);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(bee, result[0]);
		}

		[Test]
		public void getMimicList_onHive() {
			Board board = new Board(p1, p2);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 0, -1);
			board.AddToken(p1.GetFromSupply(BugType.BEETLE), 0, 0);
			board.AddToken(p2.GetFromSupply(BugType.SOLDIER_ANT), 1, 0);

			Token mos1 = p1.GetFromSupply(BugType.MOSQUITO);
			board.AddToken(mos1, 0, 0);

			List<Token> result = Rules.GetInstance().getMimicList(mos1, board);
			Assert.AreEqual(0, result.Count);
		}
	}
}

