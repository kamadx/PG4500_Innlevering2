﻿using Robocode;
using roboUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using Util = Robocode.Util.Utils;

namespace PG4500_2015_Innlevering2
{
	public class horjan_malseb_Tracky : AdvancedRobot
	{
		/* Early pseudocode/checklist, abandoned. Kept for posterity.
		 *
		 * 
		 TODO: Find target <- Done 
		 TODO: DEBUG: Just go target, then wait for new target. Repeat. <- Done
		 TODO: A* Algorithm
		 * Use an array for collision map DONE
		 * Check whether the square (50x50) can be traversed
		 * Add next choice to a queue
		 * For cornering (turn 45 deg)
		 TODO: Draw path
		 * no clue on this part yet
		 TODO: Traverse path
		 * Follow the queue
		 * Travel either 25 pixels at a time or 50
		 TODO: Win
		 * W00t
		 */

		private readonly Node[,] collisionMap = { //[map height,map width]
		{0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0},
		{0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0},
		{1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0},
		{1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0},
		{0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
		{0, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0},
		{0, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0},
		{0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0},
		{0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0},
		{0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0},
		{0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0},
		{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}};


		private List<Vector2> queuedNodes = new List<Vector2>();

		private Stack<Vector2> pathStack = new Stack<Vector2>();
		List<Rectangle> pathRect = new List<Rectangle>();


		private Vector2 node;
		private Vector2 robotPosition;
		private const int tilesize = 50;
		private const int mapWidth = 16, mapHeight = 12;
		private readonly Vector2 centerOfTile = new Vector2(25, 25);
		private bool enemyStopped;
		//private bool paintPath;
		private bool pathDone;
		//private bool moveCompleted;

		public override void Run()
		{
			pathDone = false;
			IsAdjustGunForRobotTurn = true;
			//paintPath = false;
			SetColors(Color.LightBlue, Color.Blue, Color.Tan, Color.Yellow, Color.Tan);
			enemyStopped = false;
			node = new Vector2(0, 0);
			robotPosition = new Vector2((int)X, (int)Y);
			DebugProperty["Headed to coord"] = "(" + node.X.ToString() + "," + node.Y.ToString() + ")";
			DebugProperty["Headed to Tile"] = "(" + node.X / tilesize + "," + node.Y / tilesize + ")";
			GoToPoint(node);
			WaitFor(new MoveCompleteCondition(this));
			pathDone = true;
			Out.WriteLine("#{0}\t{1}", Time, "Arrived at (" + X.ToString() + "," + Y.ToString() + ").");

			SetTurnRadarRightRadians(Double.PositiveInfinity);
			Execute();
			//Main Loop
			while (true)
			{
				if (Velocity == 0)
				{
					if (enemyStopped && pathDone == true)
					{
						//paintPath = false;
						DebugProperty["Headed to coord"] = "(" + node.X.ToString() + "," + node.Y.ToString() + ")";
						Out.WriteLine("Starting FindPath()");
						FindPath(new Vector2((int)X, (int)Y), node);
					}
				}
				Scan();
			}
		}

		/// <summary>
		/// Instructs the robot to move to a specific place.
		/// </summary>
		/// <remarks>For some reason it startet refusing to turn, and towards the end of the project's lifetime it crashed completely and we bungled our attempt at recovering its functionality.
		/// It seems to drive in the general direction of things now.</remarks>
		/// <param name="point">Point you wish to go to.</param>
		private void GoToPoint(Vector2 point)
		{
			pathDone = false;
			point *= 50;
			point -= robotPosition;
			point += centerOfTile;
			int pointX = ((point.X * 50) + 25);
			int pointY = ((point.Y * 50) + 25);

			double distance = Math.Sqrt(Math.Pow(point.X, 2) + Math.Pow(point.Y, 2));
			double angle = Util.NormalRelativeAngle(Math.Atan2(point.X, point.Y) - HeadingRadians);

			double turnAngle = Math.Atan(Math.Tan(angle));
			SetTurnRightRadians(turnAngle);
			WaitFor(new TurnCompleteCondition(this));
			SetAhead(distance * (angle == turnAngle ? 1 : -1));
			Execute();

			if (X == point.X && Y == point.Y)
			{
				//Different testing of stuff
				pathDone = true;
				//moveCompleted = true;
			}
		}

		/// <summary>
		/// Find the shortest path to target by using the A* algorithm.
		/// </summary>
		/// <param name="start">Start point for the search.</param>
		/// <param name="target">Point you are trying to reach.</param>
		/// <returns>True if a path is found, else false.</returns>
		private bool FindPath(Vector2 start, Vector2 target)
		{
			//Empty the queue to avoid errors.
			queuedNodes.Clear();

			//Y-axis-reversed version of collisionMap to match coordinates of Robocode
			Node[,] bottomLeft = (Node[,])collisionMap.Clone();
			int y2 = 0;
			for (int y = collisionMap.GetLength(0) - 1; y >= 0; y--)
			{
				for (int x = 0; x < collisionMap.GetLength(1); x++)
				{
					bottomLeft[y2, x] = collisionMap[y, x];
				}
				y2++;
			}

			//Initialize all nodes
			foreach (Node n in bottomLeft)
			{
				n.Init();
			}

			target /= tilesize;
			start /= tilesize;

			Node startNode = bottomLeft[start.Y, start.X];
			Node targetNode = bottomLeft[target.Y, target.X];

			Out.WriteLine("Start:[" + (start.X) + "," + (start.Y) + "]");
			Out.WriteLine("Target:[" + (target.X) + "," + (target.Y) + "]");

			startNode.Visited = true;
			startNode.GScore = 0;
			startNode.HScore = CalculateHScore(start, target);

			queuedNodes.Add(start);
			while (queuedNodes.Count > 0)
			{
				//Acting sort of like a queue.
				Vector2 current = queuedNodes[0];
				queuedNodes.RemoveAt(0);

				Node currentNode = bottomLeft[current.Y, current.X];
				if (currentNode == targetNode)
				{
					//We arrived!
					Out.WriteLine("WE DID IT, REDDIT!");
					makePath(start, current, bottomLeft);

					return true;
				}
				//Set current node as a visited node.
				currentNode.Visited = true;

				#region Check neighbours
				//find neighboring nodes
				List<Vector2> neighbours = new List<Vector2>();
				if (current.X > 0)
				{
					neighbours.Add(new Vector2(current.X - 1, current.Y));

					if (current.Y < mapHeight)
					{
						neighbours.Add(new Vector2(current.X - 1, current.Y + 1));
					}

					if (current.Y > 0)
					{
						neighbours.Add(new Vector2(current.X - 1, current.Y - 1));
					}
				}

				if (current.X < mapWidth)
				{
					neighbours.Add(new Vector2(current.X + 1, current.Y));

					if (current.Y > 0)
					{
						neighbours.Add(new Vector2(current.X + 1, current.Y - 1));
					}

					if (current.Y < mapHeight)
					{
						neighbours.Add(new Vector2(current.X + 1, current.Y + 1));
					}
				}

				if (current.Y < mapHeight)
				{
					neighbours.Add(new Vector2(current.X, current.Y + 1));
				}

				if (current.Y > 0)
				{
					neighbours.Add(new Vector2(current.X, current.Y - 1));
				}

				//remove all visited nodes.
				for (int i = 0; i < neighbours.Count; i++)
				{
					if (bottomLeft[neighbours[i].Y, neighbours[i].X].Visited)
					{
						neighbours.RemoveAt(i);
						i--;
						continue;
					}
					if (!bottomLeft[neighbours[i].Y, neighbours[i].X].Walkable)
					{
						neighbours.RemoveAt(i);
						i--;
					}
				}
				#endregion

				#region Calculate distance
				//calculate distance by A* method
				foreach (Vector2 neighbourCoord in neighbours)
				{
					Node neighbour = bottomLeft[neighbourCoord.Y, neighbourCoord.X];
					if (neighbour.GScore > currentNode.GScore + neighbour.Cost)
					{
						neighbour.GScore = currentNode.GScore + neighbour.Cost;
						neighbour.Parent = current;
					}
					neighbour.HScore = CalculateHScore(neighbourCoord, target);

				}
				#endregion

				#region Sort nodes
				//sort nodes by FCost.
				queuedNodes.AddRange(neighbours);
				sortNodes(queuedNodes, bottomLeft);
				#endregion

				//remove duplicates
				for (int i = 0; i < queuedNodes.Count - 1; i++)
				{
					for (int j = i + 1; j < queuedNodes.Count; j++)
					{
						if (queuedNodes[i] == queuedNodes[j])
						{
							queuedNodes.RemoveAt(j);
							j--;
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Constructs a path from start to target, based on the provided map.
		/// </summary>
		/// <param name="start">Start point for the path</param>
		/// <param name="target">Point you are trying to reach.</param>
		/// <param name="map">Map containing your nodes, prepared with FCost, Parent and the like.</param>
		private void makePath(Vector2 start, Vector2 target, Node[,] map)
		{
			pathRect.Clear();
			pathStack.Push(target);
			while (map[target.Y, target.X].Parent != start)
			{
				target = map[target.Y, target.X].Parent;
				pathRect.Add(new Rectangle(target.X, target.Y, 50, 50));
				pathStack.Push(target);
			}
			//paintPath = true;
			ReadPath();
		}

		/// <summary>
		/// Calculates the HScore (IE cost to go from here to target) of a node.
		/// </summary>
		/// <param name="current">Current node.</param>
		/// <param name="target">Node you wish to reach.</param>
		/// <returns></returns>
		private double CalculateHScore(Vector2 current, Vector2 target)
		{
			int dMax = Math.Max(Math.Abs(current.X - target.X), Math.Abs(current.Y - target.Y));
			int dMin = Math.Min(Math.Abs(current.X - target.X), Math.Abs(current.Y - target.Y));
			int nonDiagCost = 1;
			double diagCost = 1.414;
			double hScore = diagCost * dMin + nonDiagCost * (dMax - dMin);
			return hScore;
		}

		private void sortNodes(List<Vector2> list, Node[,] map)
		{
			for (int i = 0; i < list.Count - 1; i++)
			{
				Node n1 = map[list[i].Y, list[i].X];
				Node n2 = map[list[i + 1].Y, list[i + 1].X];

				//preliminarily a primitive bubble sort.
				if (n1.FScore > n2.FScore)
				{
					Vector2 temp = list[i];
					list[i] = list[i + 1];
					list[i + 1] = temp;
				}
			}
		}

		//We had some major problems using OnPaint()
		//More explained in the documentation.

		//public override void OnPaint(IGraphics graphics)
		//{
		//    //graphics.FillRectangle(Brushes.Red, Current.X * 50, Current.Y * 50, 50, 50);
		//    if (paintPath)
		//    {
		//        Out.WriteLine("WHY YOU NO PAINT?!");
		//            for (int i = 0; i > pathRect.Count; i++)
		//            {
		//                Out.WriteLine("ARE YOU FUCKING PAINTING?");
		//                graphics.FillRectangle(Brushes.Red, pathRect[i]);
		//            }
		//    }
		//}

		/// <summary>
		/// Reads the path and sends instructions to move across it.
		/// </summary>
		private void ReadPath()
		{
			Vector2 temp;
			while (pathStack.Count > 0)
			{
				temp = pathStack.Pop();
				GoToPoint(temp);
				Out.WriteLine("Next Point: [" + temp.X + "," + temp.Y + "]");
			}
			pathDone = true;

		}

		public override void OnScannedRobot(ScannedRobotEvent e)
		{
			double radarTurn = HeadingRadians + e.BearingRadians - RadarHeadingRadians;
			SetTurnRadarRightRadians(Util.NormalRelativeAngle(radarTurn));

			if (e.Velocity == 0 && Velocity == 0)
			{
				FindEnemyCoords(e);
				enemyStopped = true;
			}
			else if (e.Velocity != 0)
			{
				enemyStopped = false;
			}
		}

		/// <summary>
		/// Calculates the position of the "enemy"
		/// </summary>
		/// <param name="e">Passed down Scan event from OnScannedRobot</param>
		public void FindEnemyCoords(ScannedRobotEvent e)
		{
			double angleToEnemy = e.Bearing;

			// Calculate the angle to the scanned robot
			double angle = Util.ToRadians(Heading + angleToEnemy % 360);


			// Calculate the coordinates of the robot
			node.X = (int)(X + Math.Sin(angle) * e.Distance);
			node.Y = (int)(Y + Math.Cos(angle) * e.Distance);
		}
	}
}
