using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PG4500_2015_Innlevering2
{
	class Node
	{
		private bool _walkable;
		private int _cost;
		private int _sector;
		public bool Walkable { get { return _walkable; } set { _walkable = value; } }
		public int Cost { get { return _cost; } set { _cost = value; } }
		public int Sector { get { return _sector; } set { _sector = value; } }

		public Node(bool walkable = false, int cost = 0, int sector = 0)
		{
			Walkable = walkable;
			Cost = cost;
			Sector = sector;
		}

		public static implicit operator bool(Node n)
		{
			return n.Walkable;
		}

		public static implicit operator Node(int i)
		{
			return new Node(i == 0, i);
		}
	}
}
