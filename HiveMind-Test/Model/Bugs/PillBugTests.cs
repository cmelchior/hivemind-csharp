using NUnit.Framework;
using System;
using HiveMind.Debug;
using HiveMind.Model;

namespace HiveMindTest
{
	[TestFixture ()]
	public class PillBugTests
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
			p1.UsePillBugExpansion();
			p2 = new Player("Black", PlayerType.BLACK);
			p2.FillBaseSupply();
			p2.UsePillBugExpansion();
		}
	}
}
