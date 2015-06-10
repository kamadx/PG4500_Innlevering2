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
		private float _gScore;
		private float _hScore;

		public bool Walkable { get { return _walkable; } set { _walkable = value; } }
		public int Cost { get { return _cost; } set { _cost = value; } }
		public int Sector { get { return _sector; } set { _sector = value; } }
		public bool Visited { get { return _visited; } set { _visited = value; } }
		public double GScore { get { return _gScore; } set { _gScore = value; } }
		public double HScore { get { return _hScore; } set { _hScore = value; } }
		//returns a calculated value, but I hold that the value is simple enough to be considered a property.
		public double FScore { get { return HScore + GScore; } }

		public Node(bool walkable = false, int cost = 1, int sector = 0)
		{
			Walkable = walkable;
			Cost = cost;
			Sector = sector;
			Init();
		}

		public void Init()
		{
			Visited = false;
			GScore = 0;
			HScore = 0;
		}

		//Shorthand-functions for compatibility with initial map received.

		//for printing the map back the way it was received.
		public static implicit operator int(Node n)
		{
			return n.Walkable ? 0 : 1;
		}

		//for parsing the map the way it was received.
		public static implicit operator Node(int i)
		{
			return new Node(i == 0);
		}
	}
}
