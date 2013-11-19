using System;

namespace HiveMind
{
	static class RandomExtension 
	{
		public static Int64 NextInt64(this Random random)
		{
			var buffer = new byte[sizeof(Int64)];
			random.NextBytes(buffer);
			return BitConverter.ToInt64(buffer, 0);
		}	

		public static bool NextBool(this Random random) 
		{
			return random.NextDouble () > 0.5;
		}
	}
}

