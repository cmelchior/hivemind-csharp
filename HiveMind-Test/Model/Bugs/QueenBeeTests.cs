using NUnit.Framework;
using System;
using HiveMind.Debug;
using HiveMind.Model;
using System.Collections.Generic;
using HiveMind.Rules;

namespace HiveMindTest
{
	[TestFixture ()]
	public class QueenBeeTests
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
		public void testTargetSquares_startingPosition() {
			Board board = new Board(p1, p2);
			Token bee = p1.GetFromSupply(BugType.QUEEN_BEE);
			Token ant = p2.GetFromSupply(BugType.SOLDIER_ANT);
			board.AddToken(bee, 0, 0);
			board.AddToken(ant, 1, -1);
	
			List<Hex> targets = Rules.GetInstance().GetTargetHexes(bee, board);
			Assert.AreEqual(2, targets.Count);
			Assert.AreEqual(board.GetHex(0,-1), targets[0]);
			Assert.AreEqual(board.GetHex(1,0), targets[1]);
		}
	}
}
