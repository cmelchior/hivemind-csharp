using System;
using HiveMind.AI.Controller;
using HiveMind.AI;

namespace HiveMind
{
	public class MainClass
	{
		public static void Main (string[] args)
		{
			AIGameController gameController = new AIGameController();

			// Opponents
			HiveAI opponentA = new RandomAI("RandomA");
			HiveAI opponentB = new RandomAI("RandomB");
//			HiveAI opponentB = new SimpleMinMaxAI("SimpleMinMax", new SimpleHeuristicV2(), 2, 30000);
//			HiveAI opponentC = new AlphaBetaMiniMaxAI("AlphaBeta", new SimpleHeuristicV2(), 3, 20000);
//			HiveAI opponentCMark = new IDDFSAlphaBetaMiniMaxAI("Negamax", new SimpleHeuristicV2(), 3, 20000);
//			HiveAI opponentD = new IDDFSAlphaBetaMiniMaxAI("IDDFS", new SimpleHeuristicV3(), 2, 20000);
//			HiveAI opponentE = new MonteCarloTreeSearchAI("MCTS", 100, 30000);
//			HiveAI opponentF = new UCTMonteCarloTreeSearchAI("MCTS-UCT", 70, 20000);
//			HiveAI opponentG = new TranspostionTableIDDFSAlphaBetaMiniMaxAI("TranspositionTable-IDDFS", new SimpleHeuristicV3(), 2, 20000);
			HiveAI opponentH = new AdvancedMiniMaxAI("KillerMove-TT-IDDFS-AB", new SimpleHeuristic(), 4, 20000);
//			HiveAI opponentI = new MTDFAI("MTD(f)", new SimpleHeuristicV3(), 3, 20000);

			gameController.AddOpponent (opponentA);
			gameController.AddOpponent (opponentB);
			gameController.SetTurnLimit(30);
			gameController.SetNumberOfMatches(10);
//        gameController.start();
			gameController.StartSingleGame(opponentA, opponentH, false);
			gameController.printLog(true);
		}
	}
}

