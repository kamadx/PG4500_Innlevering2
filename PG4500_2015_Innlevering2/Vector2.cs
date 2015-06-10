using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace roboUtil
{
	class Vector2
	{
		private double _x;
		private double _y;
		public double X { get { return _x; } set { _x = value; } }
		public double Y { get { return _y; } set { _y = value; } }

		public Vector2(double x, double y)
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
			return new Vector2(v.X * d, v.Y * d);
		}

		public static Vector2 operator /(Vector2 v, double d)
		{
			return new Vector2(v.X / d, v.Y / d);
		}
	}
}
