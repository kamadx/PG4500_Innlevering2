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
		private bool _visited;
		public bool Walkable { get { return _walkable; } set { _walkable = value; } }
		public int Cost { get { return _cost; } set { _cost = value; } }
		public int Sector { get { return _sector; } set { _sector = value; } }
		public bool Visited { get { return _visited; } set { _visited = value; } }
		public Node(bool walkable = false, int cost = 0, int sector = 0, bool visited = false)
		{
			Walkable = walkable;
			Cost = cost;
			Sector = sector;
			Visited = visited;
		}

		public static implicit operator bool(Node n)
		{
			return n.Walkable;
		}

		public static implicit operator int(Node n)
		{
			return n.Cost;
		}

		//implementation specific
		public static implicit operator Node(int i)
		{
			return new Node(i == 0, i);
		}
	}
}
