using System;
using NUnit.Framework;
using HiveMind;
using HiveMind.Game;
using HiveMind.Model;
using HiveMind.AI;

namespace HiveMindTest
{
	[TestFixture()]
	public class KillerHeuristicTranspositiontableIDDFSAlphaBetaMiniMaxAITest {

		[Test]
		public void TestCanDetectWinTurnOne() 
		{
			HiveAI ai = new KillerHeuristicTranspostionTableIDDFSAlphaBetaMiniMaxAI("AdvancedAlphaBeta", new SimpleHeuristic(), 1, 30000);

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
		public void TestCanDetectWinTurnTwo() {
			HiveAI ai = new KillerHeuristicTranspostionTableIDDFSAlphaBetaMiniMaxAI("AdvancedAlphaBeta", new SimpleHeuristic(), 3, 30000);

			Game game = new Game();
			Player p1 = new Player("White", PlayerType.WHITE); p1.FillBaseSupply();
			Player p2 = new Player("Black", PlayerType.BLACK); p2.FillBaseSupply();

			game.AddPlayers(p1, p2);
			game.TurnLimit = 10;
			game = TestSetups.SureWinInTwoTurns(game);

			GameCommand command = ai.NextMove(game, game.Board);

			Assert.AreEqual(2, command.ToQ);
			Assert.AreEqual(1, command.ToR);
		}
	}
}

