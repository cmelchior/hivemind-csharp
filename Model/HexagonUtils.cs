using System;

namespace HiveMind.Model
{
	public class HexagonUtils
	{

		/// <summary>
		/// Rotates the basic vectors counter clockwise.
		/// [x, y, z] -> [-y, -z, -x]
		/// </summary>
		public static int[] RotateLeft(int[] rotation) 
		{
			int newX = -rotation[1];
			int newY = -rotation[2];
			int newZ = -rotation[0];
			rotation[0] = newX;
			rotation[1] = newY;
			rotation[2] = newZ;
			return rotation;
		}

		/// <summary>
		/// Rotates the basis vectors clockwise.
		/// [x, y, z] -> [-z, -x, -y]
		/// </summary>
		public static int[] RotateRight(int[] rotation) 
		{
			int newX = -rotation[2];
			int newY = -rotation[0];
			int newZ = -rotation[1];
			rotation[0] = newX;
			rotation[1] = newY;
			rotation[2] = newZ;
			return rotation;
		}

		public static int[] ConvertToAxialCoordinates(int x, int y, int z) 
		{
			return new int[] { x, z };
		}

		/// <summary>
		/// Cube coordinates fulfill x + y + z = 0
		/// </summary>
		public static int[] ConvertToCubeCoordinates(int q, int r) 
		{
			int x = q;
			int z = r;
			int y = -x - z;
			return new int[]{x, y, z};
		}

		/// <summary>
		/// Distance between two hexes. If neighbors the distance is 1.
		/// </summary>
		public static int Distance(int q1, int r1, int q2, int r2) 
		{
			int x1 = q1;
			int z1 = r1;
			int x2 = q2;
			int z2 = r2;
			int y1 = -(x1 + z1);
			int y2 = -(x2 + z2);
			return (Math.Abs(x1 - x2) + Math.Abs(y1 - y2) + Math.Abs(z1 - z2)) / 2;
		}	
	}
}

