using Robocode;
using Util = Robocode.Util.Utils;
using System;
using System.Collections.Generic;


namespace PG4500_2015_Innlevering2
{
    public class horjan_malseb_Tracky : AdvancedRobot
    {
        /*
         TODO: Find target <- Done 
         TODO: DEBUG: Just go target, then wait for new target. Repeat. <- Done
         TODO: A* Algorithm
         * Use an array for collision map
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


        private int[,] collisionMap = {
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




        //Point to go to.
        private int nodeX, nodeY;
        private const int tilesize = 50;
        private const int walkable = 0, unwalkable = 1;
        private const int mapWidth = 16, mapHeight = 12;
        private const int found = 1, nonexistent = 2;
        private bool enemyStopped;


        private int[] Fcost = new int[mapWidth * mapHeight + 1];
        private int[,] Gcost = new int[mapWidth+1, mapHeight+1];
        private int[] Hcost = new int[mapWidth * mapHeight + 1];
        private int[] openList = new int[mapWidth * mapHeight + 1];
        private int[,] whichList = new int[mapWidth+1, mapHeight+1];
        private int[] openX = new int[mapWidth * mapHeight + 1];
        private int[] openY = new int[mapWidth * mapHeight + 1];
        private int[,] parentX = new int[mapWidth, mapHeight];
        private int[,] parentY = new int[mapWidth, mapHeight];

        private Queue<int> pathBank;

        private int pathLength;
        private int pathLocation;
        private int onClosedList = 10; //Change this when we know what the fuck it does

        //For path-reading
        private int pathStatus;
        private int xPath;
        private int yPath;

        private RobotStatus robotStatus;

        public override void Run()
        {
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
                    //Force scan
                    //SetTurnRadarRight(360);
                    //Execute();
                    if (enemyStopped)
                    {
                        DebugProperty["Headed to coord"] = "(" + nodeX.ToString() + "," + nodeY.ToString() + ")";
                       // DebugProperty["Headed to tile"] = "(" + nodeX / tilesize + "," + nodeY / tilesize + ")";
                        Out.WriteLine("Starting FindPath()");
                        if (FindPath((int)(X), (int)(Y), nodeX, nodeY) == 1)
                        {
                            GoToPoint(25, 25);
                        }
                        // GoToPoint(nodeX, nodeY);
                    }

                    //WaitFor(new MoveCompleteCondition(this));
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

        public int FindPath(int startingX, int startingY, int targetX, int targetY)
        {
			int onOpenList = 0, parentXval = 0, parentYval = 0, temp = 0, corner = 0, numberOfOpenListItems = 0,
	addedGCost = 0, path = 0, pathX, pathY, cellPosition,
    newOpenListItemID = 0;

            int startX = (startingX / tilesize) + 1;
            int startY = (startingY / tilesize) + 1;
            targetX = (targetX / tilesize) + 1;
            targetY = (targetY / tilesize) + 1;

            Out.WriteLine("Found target at tile: [" + targetX + "," + targetY + "]");
            DebugProperty["Target Tile"] = "[" + targetX + "," + targetY + "]";
            //Not sure why I need this:
            //Resettings stuff
            if (onClosedList > 1000000)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    for (int y = 0; y < mapHeight; y++)
						whichList[x, y] = 0;
                }
                onClosedList = 10;
            }
			onClosedList += 2;
            onOpenList = onClosedList - 1;
            pathLength = 0;//i.e, = not started
            pathLocation = 0;//i.e, = not started
			Gcost[startX, startY] = 0; //reset starting square's G value to 0
            Out.WriteLine("Stop Point 1");
            //Add start location to the open list
            numberOfOpenListItems = 1;
            openList[1] = 1;//assign it as the top (and currently only) item in the open list, which is maintained as a binary heap (explained below)
            openX[1] = startX; openY[1] = startY;
            Out.WriteLine("Stop Point 2");
            do
            {
                if (numberOfOpenListItems != 0)
                {
                    parentXval = openX[openList[1]];
                    parentYval = openY[openList[1]];
                    whichList[parentXval, parentYval] = onClosedList;

                    numberOfOpenListItems -= 1;

                    openList[1] = openList[numberOfOpenListItems + 1];
					int v = 1;
                    Out.WriteLine("Stop Point 3");
					int u;

                    do
                    {
						u = v;
                        if (2 * u + 1 <= numberOfOpenListItems)
                        {
                            if (Fcost[openList[u]] >= Fcost[openList[2 * u]])
                            {
                                v = 2 * u;
                            }
                            if (Fcost[openList[v]] >= Fcost[openList[2 * u + 1]])
                            {
                                v = 2 * u + 1;
                            }
                        }
                        else
                        {
                            if (2 * u <= numberOfOpenListItems)
                            {
                                if (Fcost[openList[u]] >= Fcost[openList[2 * u]])
                                {
                                    v = 2 * u;
                                }
                            }
                        }
                        if (u != v)
                        {
                            temp = openList[u];
                            openList[u] = openList[v];
                            openList[v] = temp;
                        }
					} while (u != v);

					for (int b = parentYval - 1; b <= parentYval + 1; b++)
                    {
						for (int a = parentXval - 1; a <= parentXval + 1; b++)
                        {
                            Out.WriteLine("Stop Point 5a");
                            if (a >= 0 && b >= 0 && a <= mapWidth && b <= mapHeight)
                            {
                                Out.WriteLine("Stop Point 5b");
                                if (whichList[a, b] != onClosedList) //<- FOUND THE BUG
                                {
                                    Out.WriteLine("Stop Point 5c");
                                    if (collisionMap[a, b] != unwalkable)
                                    {
                                        Out.WriteLine("Stop Point 6");
                                        corner = walkable;
                                        if (a == parentYval - 1)
                                        {
                                            if (b == parentYval - 1)
                                            {
                                                if (collisionMap[parentXval - 1, parentYval] == unwalkable || collisionMap[parentXval, parentYval - 1] == unwalkable)
                                                {
                                                    corner = unwalkable;
                                                }
                                            }
                                            else if (b == parentYval + 1)
                                            {
                                                if (collisionMap[parentXval, parentYval + 1] == unwalkable || collisionMap[parentXval - 1, parentYval] == unwalkable)
                                                {
                                                    corner = unwalkable;
                                                }
                                            }
                                        }
                                        else if (a == parentXval + 1)
                                        {
                                            Out.WriteLine("Stop Point 7");
                                            if (b == parentYval - 1)
                                            {
                                                if (collisionMap[parentXval, parentYval - 1] == unwalkable || collisionMap[parentXval + 1, parentYval] == unwalkable)
                                                    corner = unwalkable;
                                            }
                                            else if (b == parentYval + 1)
                                            {
                                                if (collisionMap[parentXval + 1, parentYval] == unwalkable || collisionMap[parentXval, parentYval + 1] == unwalkable)
                                                    corner = unwalkable;
                                            }
                                        }
                                        if (corner == walkable)
                                        {
                                            Out.WriteLine("Stop Point 8");
											int m;
                                            if (whichList[a, b] != onOpenList)
                                            {
                                                Out.WriteLine("Stop Point 8a");
                                                newOpenListItemID += 1;
                                                m = numberOfOpenListItems + 1;
                                                openList[m] = newOpenListItemID;
                                                openX[newOpenListItemID] = a;
                                                openY[newOpenListItemID] = b;
                                                Out.WriteLine("Stop Point 8b");
                                                if (Math.Abs(a - parentXval) == 1 && Math.Abs(b - parentYval) == 1)
                                                {
                                                    addedGCost = 14;
                                                }
                                                else
                                                {
                                                    addedGCost = 10;
                                                }
                                                Out.WriteLine("Stop Point 8c");
                                                Gcost[a, b] = Gcost[parentXval, parentYval] + addedGCost; // <- THIS FUCKER 
                                                Out.WriteLine("Stop Point 9");
                                                Hcost[openList[m]] = 10 * (Math.Abs(a - targetX) + Math.Abs(b - targetY));
                                                Fcost[openList[m]] = Gcost[a, b] + Hcost[openList[m]];
                                                parentX[a, b] = parentXval;
                                                parentY[a, b] = parentYval;

                                                while (m != 1)
                                                {
                                                    if (Fcost[openList[m]] <= Fcost[openList[m / 2]])
                                                    {
                                                        temp = openList[m / 2];
                                                        openList[m / 2] = openList[m];
                                                        openList[m] = temp;
                                                        m /= 2;
                                                        Out.WriteLine("Stop Point 10a");
                                                    }
                                                    else
                                                    {
                                                        Out.WriteLine("Stop Point 10b");
                                                        break;
                                                    }
                                                }
                                                numberOfOpenListItems += 1;

                                                whichList[a, b] = onOpenList;
                                            }
                                            else
                                            {
                                                Out.WriteLine("Stop Point 11");
                                                if (Math.Abs(a - parentXval) == 1 && Math.Abs(b - parentYval) == 1)
                                                {
                                                    addedGCost = 14;
                                                }
                                                else
                                                {
                                                    addedGCost = 10;
                                                }
												int tempGcost = Gcost[parentXval, parentYval] + addedGCost;

                                                if (tempGcost < Gcost[a, b])
                                                {
                                                    parentX[a, b] = parentXval;
                                                    parentY[a, b] = parentYval;
                                                    Gcost[a, b] = tempGcost;
                                                    Out.WriteLine("Stop Point 12");
                                                    for (int x = 1; x <= numberOfOpenListItems; x++)
                                                    {
                                                        if (openX[openList[x]] == a && openY[openList[x]] == b)
                                                        {
                                                            Fcost[openList[x]] = Gcost[a, b] + Hcost[openList[x]];

                                                            m = x;
                                                            while (m != 1)
                                                            {
                                                                Out.WriteLine("Stop Point 13");
                                                                if (Fcost[openList[m]] < Fcost[openList[m / 2]])
                                                                {
                                                                    Out.WriteLine("Stop Point 14a");
                                                                    temp = openList[m / 2];
                                                                    openList[m / 2] = openList[m];
                                                                    openList[m] = temp;
                                                                    m /= 2;
                                                                }
                                                                else
                                                                {
                                                                    Out.WriteLine("Stop Point 14b");
                                                                    break;
                                                                }
                                                            }
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            Out.WriteLine("Stop Point After 10b");
                                        }
                                    }
                                }
                            }
							else
							{
								Out.WriteLine("Raise hell! And zombies!");
							}
                        }
                    }
                }
                else
                {
                    path = nonexistent;
                    break;
                }
                if (whichList[targetX, targetY] == onOpenList)
                {
                    Out.WriteLine("Stop Point before 15");
                    path = found;
                    break;
                }
                Out.WriteLine("Stop Point 15");
            } while (true);
            Out.WriteLine("Stop Point 16");
            if (path == found)
            {
                pathX = targetX;
                pathY = targetY;

				//This shit is all backwards...

                do
                {
					int tempx = parentX[pathX, pathY];
                    pathY = parentY[pathX, pathY];
                    pathX = tempx;

                    pathLength += 1;
                } while (pathX != startX || pathY != startY);

                pathX = targetX;
                pathY = targetY;
                cellPosition = pathLength * 2;

				//So is this...

                do
                {
                    pathBank.Enqueue(pathX);
                    pathBank.Enqueue(pathY);

					int tempx = parentX[pathX, pathY];
                    pathY = parentY[pathX, pathY];
                    pathX = tempx;
                } while (pathX != startX || pathY != startY);

                ReadPath(startingX, startingY);
            }
            int tempX = pathBank.Dequeue();
            int tempY = pathBank.Dequeue();
            Out.WriteLine("Last possible stop point");
            DebugProperty["Path"] = "[" + tempX + "," + tempY + "]";
            return path;
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
