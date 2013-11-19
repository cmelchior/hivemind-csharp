using NUnit.Framework;
using System;
using HiveMind.Debug;
using HiveMind.Model;
using System.Collections.Generic;
using HiveMind.Rules;

namespace HiveMindTest
{
	[TestFixture ()]
	public class MosquitoTests
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
			p1.UseMosquitoExpansion();
			p2 = new Player("Black", PlayerType.BLACK);
			p2.FillBaseSupply();
			p2.UseMosquitoExpansion();
		}

		/// <summary>
		/// A mosquito has no move, so copying a mosquito does nothing.
		/// </summary>
		[Test]
		public void TestTargetSquares_noMove() 
		{
			Board board = new Board(p1, p2);
			Token mos1 = p1.GetFromSupply(BugType.MOSQUITO);
			Token mos2 = p2.GetFromSupply(BugType.MOSQUITO);
			mos1.mimic(mos2);

			board.AddToken(mos1, 0, 0);
			board.AddToken(mos2, 1, 0);

			List<Hex> targets = Rules.GetInstance().GetTargetHexes(mos1, board);
			Assert.AreEqual(0, targets.Count);
		}

		///  Copying another bug grants movement.
		[Test]
		public void testTargetSquares_copyMovement() {
			Board board = new Board(p1, p2);
			Token mos1 = p1.GetFromSupply(BugType.MOSQUITO);
			Token bee = p2.GetFromSupply(BugType.QUEEN_BEE);
			mos1.mimic(bee);

			board.AddToken(mos1, 0, 0);
			board.AddToken(bee, 1, 0);

			List<Hex> targets = Rules.GetInstance().GetTargetHexes(mos1, board);
			Assert.AreEqual(2, targets.Count);
			Assert.IsTrue(targets.Contains(board.GetHex(1, -1)));
			Assert.IsTrue(targets.Contains(board.GetHex(0,1)));
		}
	}
}
