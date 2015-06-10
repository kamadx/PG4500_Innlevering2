
namespace roboUtil
{
	class Vector2
	{
		private int _x;
		private int _y;
		public int X { get { return _x; } set { _x = value; } }
		public int Y { get { return _y; } set { _y = value; } }

		public Vector2(int x, int y)
		{
			X = x;
			Y = y;
		}

		public static Vector2 operator -(Vector2 v1, Vector2 v2)
		{
			return new Vector2(v1.X - v2.X, v1.Y - v2.Y);
		}
		public static Vector2 operator +(Vector2 v1, Vector2 v2)
		{
			return new Vector2(v1.X + v2.X, v1.Y + v2.Y);
		}

		public static Vector2 operator *(Vector2 v, double d)
		{
			return new Vector2((int)(v.X * d), (int)(v.Y * d));
		}

		public static Vector2 operator /(Vector2 v, double d)
		{
			return new Vector2((int)(v.X / d), (int)(v.Y / d));
		}
	}
}
