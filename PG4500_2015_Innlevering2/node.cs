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
		public bool Walkable
		{
			get
			{
				return _walkable;
			}
			set
			{
				_walkable = value;
			}
		}
		public Node(bool walkable)
		{
			Walkable = walkable;
		}

		public static implicit operator Node(bool b)
		{
			return new Node(b);
		}
	}
}
