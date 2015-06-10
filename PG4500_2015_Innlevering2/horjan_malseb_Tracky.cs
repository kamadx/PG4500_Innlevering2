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


		private List<Vector2> queuedNodes = new List<Vector2>();
		//private List<int> queuedNodes = new List<int>();

		//Path Queue for reading path
		private Queue<int> pathQueue = new Queue<int>();



		//Point to go to.
		private int nodeX, nodeY;
		private Vector2 node;
		private Vector2 robotPosition;
		private const int tilesize = 50;
		private const int mapWidth = 16, mapHeight = 12;
		// private const int found = 1, nonexistent = 2;
		private bool enemyStopped;
        private bool pathDone;
        private bool paintPath;


        //DEBUG STUFF
        private int CurrentX, CurrentY;


		private RobotStatus robotStatus;

		public override void Run()
		{
            paintPath = false;
            pathDone = false;
            SetColors(Color.LightBlue, Color.Blue, Color.Tan, Color.Yellow, Color.Tan);
			enemyStopped = false;
			//Startup - Go to (25, 25) and wait.
			node = new Vector2(25, 25);
			robotPosition = new Vector2((int)X, (int)Y);
			//nodeX = 25;
			//nodeY = 25;
			DebugProperty["Headed to coord"] = "(" + nodeX.ToString() + "," + nodeY.ToString() + ")";
			DebugProperty["Headed to Tile"] = "(" + nodeX / tilesize + "," + nodeY / tilesize + ")";
			GoToPoint(nodeX, nodeY, true);
			WaitFor(new MoveCompleteCondition(this));
            pathDone = true;
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
					if (enemyStopped && pathDone)
					{
						DebugProperty["Headed to coord"] = "(" + nodeX.ToString() + "," + nodeY.ToString() + ")";
						// DebugProperty["Headed to tile"] = "(" + nodeX / tilesize + "," + nodeY / tilesize + ")";
						Out.WriteLine("Starting FindPath()");
						if (FindPath(robotPosition, node))
						//if (FindPath((int)(X), (int)(Y), nodeX, nodeY))
                        {
                            paintPath = true;
                            int x = 0, y = 0;
							Vector2 position;
                            for (int i = 0; i <= queuedNodes.Count / 2; i++)
                            {
                                //Debug
								position = queuedNodes[0];
								queuedNodes.RemoveAt(0);
								//y = queuedNodes[0];
								//queuedNodes.RemoveAt(0);
								//x = queuedNodes[0];
								//queuedNodes.RemoveAt(0);
                                GoToPoint(x, y, false);
                            }
                        }
						
					}

				}
				Scan();
			}
		}

		//Instructs the robot to move to a specific place.
		//public void GoToPoint(Vector2 point, bool startPoint)
		public void GoToPoint(double pointX, double pointY, bool startPoint)
		{
            pathDone = false;
            Out.WriteLine("Next point: [" + pointX + " , " + pointY + "]");
			
            //Go to point specified
            if (startPoint == true)
            {
				//point -= robotPosition;
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
            else
            {
                double angle = Util.NormalRelativeAngle(Math.Atan2((pointX*50), (pointY*50)) - HeadingRadians);
                double turnAngle = Math.Atan(Math.Tan(angle));
                WaitFor(new TurnCompleteCondition(this));
                
            }
                
            if (X == pointX && Y == pointY)
            {
                pathDone = true;
            }
            

			
		}

		public bool FindPath(Vector2 start, Vector2 target)
		//public bool FindPath(int startX, int startY, int targetX, int targetY)
		{
            //Out.WriteLine("Stop Point 1");
            //Empty the queue to avoid errors.
            queuedNodes.Clear();

			//Y-axis-reversed version of collisionMap to match coordinates of Robocode
			Node[,] bottomLeft = (Node[,])collisionMap.Clone();
			int y2 = 0;
            for (int y = collisionMap.GetLength(0) - 1; y >= 0; y--)
			{
                //Out.WriteLine("Stop Point 1a");
				for (int x = 0; x < collisionMap.GetLength(1); x++)
				{
                   // Out.WriteLine("Stop Point 1b");
					bottomLeft[y2, x] = collisionMap[y, x];
                    //Out.WriteLine("Stop Point 1c");
				}
				y2++;
               // Out.WriteLine("Stop Point 1d");
			}

               // Out.WriteLine("Stop Point 2");
			//Set every Node to not visited.
			foreach (Node n in bottomLeft)
			{
				n.Init();
			}
			target /= tilesize;
           // Out.WriteLine("Stop Point 3");
			//targetX /= tilesize;
			//targetY /= tilesize;
			start /= tilesize;
			//startX /= tilesize;
			//startY /= tilesize;

			Node startNode = bottomLeft[start.Y, start.X];
			//Node startNode = bottomLeft[startY, startX];
			Node targetNode = bottomLeft[target.Y, target.X];
			//Node targetNode = bottomLeft[targetY, targetX];

			Out.WriteLine("Start:[" + (start.X) + "," + (start.Y) + "]");
			Out.WriteLine("Target:[" + (target.X) + "," + (target.Y) + "]");
            //Out.WriteLine("Stop Point 1");

			startNode.Visited = true;
			startNode.GScore = 0;
			startNode.HScore = CalculateHScore(start, target);
			//startNode.HScore = CalculateHScore(startX, startY, targetX, targetY);

			queuedNodes.Add(start);
			//queuedNodes.Add(startY);
			//queuedNodes.Add(startX);
           // Out.WriteLine("Stop Point 4");
			while (queuedNodes.Count > 0)
			{
               
                //Out.WriteLine("Stop Point 1 (inside whileLoop)");
				//Acting sort of like a queue.
				Vector2 current = queuedNodes[0];
				//int currentY = queuedNodes[0];
				//int currentX = queuedNodes[1];
				queuedNodes.RemoveAt(0);
				//queuedNodes.RemoveAt(0);
				//queuedNodes.RemoveAt(0);


                Out.WriteLine("CurrentNode: ["+current.X+ ","+current.Y+"]");

				Node currentNode = bottomLeft[current.Y, current.X];
				//Node currentNode = bottomLeft[currentY, currentX];
				if (currentNode == targetNode)
				{
					//We arrived!
					return true;
				}
                //Out.WriteLine("Stop Point 2 (inside whileLoop)");
				//Set current node as a visited node.
				currentNode.Visited = true;

				#region Check neighbours
				//find neighboring nodes
				List<Vector2> neighbours = new List<Vector2>();
				//List<int> neighbours = new List<int>();
                //Out.WriteLine("Stop Point 3 (inside whileLoop)");
				if (current.X > 0)
				//if (currentX > 0)
				{
					neighbours.Add(new Vector2(current.X - 1, current.Y));
					//neighbours.Add(currentY);
					//neighbours.Add(currentX - 1);

					if (current.Y < mapHeight)
					//if (currentY < bottomLeft.GetLength(0))
					{
						neighbours.Add(new Vector2(current.X - 1, current.Y + 1));
						//neighbours.Add(currentY + 1);
						//neighbours.Add(currentX - 1);
					}

					if (current.Y > 0)
					//if (currentY > 0)
					{
						neighbours.Add(new Vector2(current.X - 1, current.Y - 1));
						//neighbours.Add(currentY - 1);
						//neighbours.Add(currentX - 1);
					}
				}

				if (current.X < mapWidth)
				//if (currentX < bottomLeft.GetLength(1))
				{
					neighbours.Add(new Vector2(current.X + 1, current.Y));
					//neighbours.Add(currentY);
					//neighbours.Add(currentX + 1);

					if (current.Y > 0)
					//if (currentY > 0)
					{
						neighbours.Add(new Vector2(current.X + 1, current.Y - 1));
						//neighbours.Add(currentY - 1);
						//neighbours.Add(currentX + 1);
					}

					if (current.Y < mapHeight)
					//if (currentY < bottomLeft.GetLength(0))
					{
						neighbours.Add(new Vector2(current.X + 1, current.Y + 1));
						//neighbours.Add(currentY + 1);
						//neighbours.Add(currentX + 1);
					}
				}

				if (current.Y < mapHeight)
					//if (currentY < bottomLeft.GetLength(0))
				{
					neighbours.Add(new Vector2(current.X, current.Y + 1));
					//neighbours.Add(currentY + 1);
					//neighbours.Add(currentX);
				}

				if (current.Y > 0)
				//if (currentY > 0)
				{
					neighbours.Add(new Vector2(current.X, current.Y - 1));
					//neighbours.Add(currentY - 1);
					//neighbours.Add(currentX);
				}
                //Out.WriteLine("Stop Point 4 (inside whileLoop)");
				//remove all visited nodes.
				for (int i = 0; i < neighbours.Count; i++)
				//for (int i = 0; i < neighbours.Count; i += 2)
				{
                    //Out.WriteLine("Stop Point 1 - Inside NeighborCheck");
					if (bottomLeft[neighbours[i].Y, neighbours[i].X].Visited)
					//if (bottomLeft[neighbours[i], neighbours[i + 1]].Visited)
					{
                        //Out.WriteLine("Stop Point 2 - Inside NeighborCheck");
						neighbours.RemoveAt(i);
						//neighbours.RemoveAt(i);
						//neighbours.RemoveAt(i);
						i--;
						//i -= 2;
                      //  Out.WriteLine("Stop Point 3 - Inside NeighborCheck");
						continue;
						//continue;
					}
                   // Out.WriteLine("Stop Point 4 - Inside NeighborCheck"); //<- Crashes here after several iterations
					if (!bottomLeft[neighbours[i].Y, neighbours[i].X].Walkable)
					//if (!bottomLeft[neighbours[i], neighbours[i + 1]].Walkable)
					{
                       // Out.WriteLine("Stop Point 5 - Inside NeighborCheck");
						//neighbours.RemoveAt(i);
						//neighbours.RemoveAt(i);
						neighbours.RemoveAt(i);
						i--;
						//i -= 2;
                       // Out.WriteLine("Stop Point 6 - Inside NeighborCheck");
					}
				}
				//TODO remove this crap
				//for (int i = 0; i < neighbours.Count; i += 2)
				//{
					//Node test = bottomLeft[neighbours[i], neighbours[i + 1]];
					//Out.WriteLine("If this throws errors, neighbour check is wrong.");
				//}
				#endregion

				#region Calculate distance
               // Out.WriteLine("Stop Point 5 (inside whileLoop)");
				//calculate distance by A* method
				for (int i = 0; i < neighbours.Count; i++)
					foreach (Vector2 neighbourCoord in neighbours)
				{
				Node neighbour = bottomLeft[neighbours[i].Y,neighbours[i].X];
				neighbour.GScore = currentNode.GScore + neighbour.Cost;
				neighbour.HScore = CalculateHScore(neighbourCoord,target);
				}
				//for (int i = 0; i < neighbours.Count; i += 2)
				//{
				//	Node neighbour = bottomLeft[neighbours[i].Y, neighbours[i].X];
				//	neighbour.GScore = currentNode.GScore + neighbour.Cost;
				//	neighbour.HScore = CalculateHScore(neighbours[i + 1], neighbours[i], targetX, targetY);
				//}
				#endregion
               // Out.WriteLine("Stop Point 6 (inside whileLoop)");
				#region Sort nodes
				//sort nodes by FCost.
				//queuedNodes.AddRange(neighbours);
				queuedNodes.AddRange(neighbours);
               // Out.WriteLine("Stop Point 6a (inside whileLoop)");
				sortNodes(queuedNodes, bottomLeft);
                //Out.WriteLine("Stop Point 6b (inside whileLoop)");
				#endregion
                //Out.WriteLine("Stop Point 7 (inside whileLoop)");
				//remove duplicates
				for (int i = 0; i < queuedNodes.Count - 1; i++)
				//for (int i = 0; i < queuedNodes.Count - 2; i += 2)
				{
					for (int j = i + 1; j < queuedNodes.Count; j++)
						if (queuedNodes[i] == queuedNodes[j])
					//if (queuedNodes[i] == queuedNodes[i + 2] && queuedNodes[i + 1] == queuedNodes[i + 3])
					{
						//queuedNodes.RemoveAt(i);
						//queuedNodes.RemoveAt(i);
						queuedNodes.RemoveAt(i);
						i--;
						//i -= 2;
					}
				}
				//CurrentX = currentX;
				//CurrentY = currentY;
               // Out.WriteLine("Stop Point 8 (inside whileLoop)");
			}
			return false;
		}
		private double CalculateHScore(Vector2 current, Vector2 target)
		//private double CalculateHScore(int currentX, int currentY, int targetX, int targetY)
		{
			int dMax = Math.Max(Math.Abs(current.X - target.X), Math.Abs(current.Y - target.Y));
			int dMin = Math.Min(Math.Abs(current.X - target.X), Math.Abs(current.Y - target.Y));
			int nonDiagCost = 1;
			double diagCost = 1.414;
			double hScore = diagCost * dMin + nonDiagCost * (dMax - dMin);
			return hScore;
		}
		private void sortNodes(List<Vector2> list, Node[,] map)
		//private void sortNodes(List<int> list, Node[,] map)
		{
			if (list.Count % 2 != 0)
			{
				//Out.WriteLine("The list isn't even. What the fuck?");
			}
          //  Out.WriteLine("Stop Point 1 (inside SortNode())");
			for (int i = 0; i < list.Count - 1; i--) 
			//for (int i = 0; i < list.Count - 2; i += 2)
			{
				//   Out.WriteLine("Stop Point 2 (inside SortNode())");
				Node n1 = map[list[i].Y, list[i].X];
				Node n2 = map[list[i+1].Y, list[i+1].X];
				//  Out.WriteLine("Stop Point 3 (inside SortNode())");
				//preliminarily a primitive bubble sort.
				if (n1.FScore > n2.FScore)
				{
					//     Out.WriteLine("Stop Point 4 (inside SortNode())");
					Vector2 temp = list[i];
					list[i] = list[i + 2];
					list[i + 2] = temp;
					//temp = list[i + 1];
					//list[i + 1] = list[i + 3];
					//list[i + 3] = temp;
					//   Out.WriteLine("Stop Point 5 (inside SortNode())");
				}
			}
		}

       

        public override void OnPaint(IGraphics graphics)
        {
            graphics.FillRectangle(Brushes.Red, CurrentX * 50, CurrentY * 50, 50, 50);
            if (paintPath)
        {
            
                for (int i = 0; i <= queuedNodes.Count / 2; i++)
                {

                }
        }

        }

		public void ReadPath(int currentX, int currentY)
		{
			
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
