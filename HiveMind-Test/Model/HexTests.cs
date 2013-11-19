using NUnit.Framework;
using System;
using HiveMind.Model;

namespace HiveMindTest
{
	[TestFixture ()]
	public class HexTests
	{
		[Test ()]
		public void GetCoordinates()
		{
			Hex hex = new Hex (1,2);
			Assert.AreEqual (1, hex.Q);
			Assert.AreEqual (2, hex.R);
		}

		[Test()]
		public void SortHex_smaller()
		{
			Hex hex1 = new Hex (1, 1);
			Hex hex2 = new Hex (2, 1);
			Assert.AreEqual (-1, hex1.CompareTo (hex2));
		}

		[Test()]
		public void SortHex_larger()
		{
			Hex hex1 = new Hex (1, 2);
			Hex hex2 = new Hex (1, 1);
			Assert.AreEqual (1, hex1.CompareTo (hex2));
		}
	}
}

