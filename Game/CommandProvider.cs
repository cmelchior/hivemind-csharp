using System;
using HiveMind.Model;

namespace HiveMind.Game
{
	public interface CommandProvider
	{

		GameCommand GetCommand(Game currentState, Board board);
	}
}