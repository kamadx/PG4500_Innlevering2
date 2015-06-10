using Robocode;
using Util = Robocode.Util.Utils;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;
using roboUtil;

namespace PG4500_2015_Innlevering2
{
	public class horjan_malseb_Tracky : AdvancedRobot
	{
		/*
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


		private List<int> queuedNodes = new List<int>();

		//Path Queue for reading path
		private Queue<int> pathQueue = new Queue<int>();



		//Point to go to.
		private int nodeX, nodeY;
		private const int tilesize = 50;
		private const int mapWidth = 16, mapHeight = 12;
		// private const int found = 1, nonexistent = 2;
		private bool enemyStopped;

		private RobotStatus robotStatus;

		public override void Run()
		{
            SetColors(Color.LightBlue, Color.Blue, Color.Tan, Color.Yellow, Color.Tan);
			enemyStopped = false;
			//Startup - Go to (25, 25) and wait.
			nodeX = 25;
			nodeY = 25;
			DebugProperty["Headed to coord"] = "(" + nodeX.ToString() + "," + nodeY.ToString() + ")";
			DebugProperty["Headed to Tile"] = "(" + nodeX / tilesize + "," + nodeY / tilesize + ")";
			GoToPoint(nodeX, nodeY);
			WaitFor(new MoveCompleteCondition(this));
			Out.WriteLine("#{0}\t{1}", Time, "Arrived at (" + X.ToString() + "," + Y.ToString() + ").");
			//SetTurnRight(180);

			IsAdjustGunForRobotTurn = true;

			SetTurnRadarRightRadians(Double.PositiveInfinity);
			Execute();
			//Main Loop
			while (true)
			{

				if (Velocity == 0)
				{
					if (enemyStopped)
					{
						DebugProperty["Headed to coord"] = "(" + nodeX.ToString() + "," + nodeY.ToString() + ")";
						// DebugProperty["Headed to tile"] = "(" + nodeX / tilesize + "," + nodeY / tilesize + ")";
						Out.WriteLine("Starting FindPath()");
                        if (FindPath((int)(X), (int)(Y), nodeX, nodeY))
                        {
                            //ReadPath()
                        }
						
					}

				}
				Scan();
			}
		}

		//Instructs the robot to move to a specific place.
		public void GoToPoint(double pointX, double pointY)
		{
			//Go to point specified
			pointX -= X;
			pointY -= Y;

			double distance = Math.Sqrt(Math.Pow(pointX, 2) + Math.Pow(pointY, 2));
			double angle = Util.NormalRelativeAngle(Math.Atan2(pointX, pointY) - HeadingRadians);

			double turnAngle = Math.Atan(Math.Tan(angle));
			SetTurnRightRadians(turnAngle);
			WaitFor(new TurnCompleteCondition(this));
			SetAhead(distance * (angle == turnAngle ? 1 : -1));
			Execute();
		}

		public bool FindPath(int startX, int startY, int targetX, int targetY)
		{

            //Empty the queue to avoid errors.
            queuedNodes.Clear();

			//Y-axis-reversed version of collisionMap to match coordinates of Robocode
			Node[,] bottomLeft = (Node[,])collisionMap.Clone();
			for (int y = collisionMap.GetLength(0), y2 = 0; y >= 0; y--)
			{
				for (int x = 0; x <= collisionMap.GetLength(1); y++)
				{
					bottomLeft[y2, x] = collisionMap[y, x];
				}
				y2++;
			}

			//Set every Node to not visited.
			foreach (Node n in bottomLeft)
			{
				n.Init();
			}

			targetX /= tilesize;
			targetY /= tilesize;
			startX /= tilesize;
			startY /= tilesize;

			Node startNode = bottomLeft[startY, startX];
			Node targetNode = bottomLeft[targetY, targetX];

			Out.WriteLine("Start:[" + (startX + 1) + "," + (startY + 1) + "]");
			Out.WriteLine("Target:[" + (targetX + 1) + "," + (targetY + 1) + "]");


			startNode.Visited = true;
			startNode.GScore = startNode.Cost;
			startNode.HScore = CalculateHScore(startX, startY, targetX, targetY);

			queuedNodes.Add(startY);
			queuedNodes.Add(startX);

			while (queuedNodes.Count > 0)
			{
				//Acting sort of like a queue.
				int currentY = queuedNodes[0];
				int currentX = queuedNodes[1];
				queuedNodes.RemoveAt(0);
				queuedNodes.RemoveAt(0);

				Node currentNode = bottomLeft[currentY, currentX];
				if (currentNode == targetNode)
				{
					//We arrived!
					return true;
				}

				//Set current node as a visited node.
				currentNode.Visited = true;

				#region Check neighbours
				//find neighboring nodes
				List<int> neighbours = new List<int>();

				if (currentX > 0)
				{
					neighbours.Add(currentY);
					neighbours.Add(currentX - 1);

					if (currentY < bottomLeft.GetLength(0))
					{
						neighbours.Add(currentY + 1);
						neighbours.Add(currentX - 1);
					}

					if (currentY > 0)
					{
						neighbours.Add(currentY - 1);
						neighbours.Add(currentX - 1);
					}
				}

				if (currentX < bottomLeft.GetLength(1))
				{
					neighbours.Add(currentY);
					neighbours.Add(currentX + 1);

					if (currentY > 0)
					{
						neighbours.Add(currentY - 1);
						neighbours.Add(currentX + 1);
					}

					if (currentY < bottomLeft.GetLength(0))
					{
						neighbours.Add(currentY + 1);
						neighbours.Add(currentX + 1);
					}
				}

				if (currentY < bottomLeft.GetLength(0))
				{
					neighbours.Add(currentY + 1);
					neighbours.Add(currentX);
				}

				if (currentY > 0)
				{
					neighbours.Add(currentY - 1);
					neighbours.Add(currentX);
				}

				//remove all visited nodes.
				for (int i = 0; i < neighbours.Count; i += 2)
				{
					if (bottomLeft[i, i + 1].Visited)
					{
						neighbours.RemoveAt(i);
						neighbours.RemoveAt(i);
						i -= 2;
					}
					if (!bottomLeft[i, i + 1].Walkable)
					{
						neighbours.RemoveAt(i);
						neighbours.RemoveAt(i);
						i -= 2;
					}
				}
				#endregion

				#region Calculate distance
				//calculate distance by A* method
				for (int i = 0; i < neighbours.Count; i += 2)
				{
					Node neighbour = bottomLeft[neighbours[i], neighbours[i + 1]];
					neighbour.GScore = currentNode.GScore + neighbour.Cost;
					neighbour.HScore = CalculateHScore(neighbours[i + 1], neighbours[i], targetX, targetY);
				}
				#endregion

				#region Sort nodes
				//sort nodes by FCost.
				queuedNodes.AddRange(neighbours);
				sortNodes(queuedNodes, bottomLeft);
				#endregion

				//remove duplicates
				for (int i = 0; i < queuedNodes.Count - 2; i += 2)
				{
					if (queuedNodes[i] == queuedNodes[i + 2] && queuedNodes[i + 1] == queuedNodes[i + 3])
					{
						queuedNodes.RemoveAt(i);
						queuedNodes.RemoveAt(i);
						i -= 2;
					}
				}

			}
			return false;
		}

		private double CalculateHScore(int currentX, int currentY, int targetX, int targetY)
		{
			int dMax = Math.Max(Math.Abs(currentX - targetX), Math.Abs(currentY - targetY));
			int dMin = Math.Min(Math.Abs(currentX - targetX), Math.Abs(currentY - targetY));
			int nonDiagCost = 1;
			double diagCost = 1.414;
			double hScore = diagCost * dMin + nonDiagCost * (dMax - dMin);
			return hScore;
		}

		private void sortNodes(List<int> list, Node[,] map)
		{
			for (int i = 0; i < list.Count - 2; i += 2)
			{
				Node n1 = map[i, i + 1];
				Node n2 = map[i + 2, i + 3];
				//preliminarily a primitive bubble sort.
				if (n1.FScore > n2.FScore)
				{
					int temp = list[i];
					list[i] = list[i + 2];
					list[i + 2] = temp;
					temp = list[i + 1];
					list[i + 1] = list[i + 3];
					list[i + 3] = temp;
				}
			}
		}

        public void DrawPath()
        {
            //queuedNodes[0] = y, [1] = x
            

            /*
             Drawing path here
             */
        }

        public override void OnPaint(IGraphics graphics)
        {
            
        }

		public void ReadPath(int currentX, int currentY)
		{
			GoToPoint(nodeX, nodeY);
		}

		public int ReadPathX(int pathLocation)
		{
			return 0;
		}

		public int ReadPathY(int pathLocation)
		{
			return 0;
		}

		public override void OnStatus(StatusEvent e)
		{
			robotStatus = e.Status;
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

		public void FindEnemyCoords(ScannedRobotEvent e)
		{
			double angleToEnemy = e.Bearing;

			// Calculate the angle to the scanned robot
			double angle = Util.ToRadians(robotStatus.Heading + angleToEnemy % 360);


			// Calculate the coordinates of the robot
			nodeX = (int)(robotStatus.X + Math.Sin(angle) * e.Distance);
			nodeY = (int)(robotStatus.Y + Math.Cos(angle) * e.Distance);
		}
	}
}
