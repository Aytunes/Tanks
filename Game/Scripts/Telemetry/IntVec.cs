using CryEngine;

namespace CryGameCode
{
	public struct IntVec3
	{
		public int X { get; private set; }
		public int Y { get; private set; }
		public int Z { get; private set; }

		public IntVec3(int x, int y, int z)
			: this()
		{
			X = x;
			Y = y;
			Z = z;
		}

		public IntVec3(float x, float y, float z)
			: this((int)x, (int)y, (int)z)
		{
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2}", X, Y, Z);
		}
	}

	public struct IntVec2
	{
		public int X { get; private set; }
		public int Y { get; private set; }

		public IntVec2(int x, int y)
			: this()
		{
			X = x;
			Y = y;
		}

		public IntVec2(float x, float y)
			: this((int)x, (int)y)
		{
		}

		public override string ToString()
		{
			return string.Format("{0} {1}", X, Y);
		}
	}

	public static class IntVecExtensions
	{
		public static IntVec3 ToIntVec3(this Vec3 vec)
		{
			return new IntVec3(vec.X, vec.Y, vec.Z);
		}

		public static IntVec2 ToIntVec2(this Vec3 vec)
		{
			return new IntVec2(vec.X, vec.Y);
		}
	}
}
