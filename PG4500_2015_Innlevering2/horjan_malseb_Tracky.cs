using Robocode;
using Util = Robocode.Util.Utils;
using System;
using System.Collections.Generic;
using System.Collections;

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


        private readonly int[,] collisionMap = { //[y,x]
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

		private readonly Node[,] _collisionMap = { //[y,x]
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


        //new
        private int[,] checkedNodes = new int[mapHeight, mapWidth];
        //private int[,] uncheckedNodes = new int[mapWidth, mapHeight];
        private Queue<int> queuedNodes = new Queue<int>();
        private int[,] visitedNodes = new int[mapHeight, mapWidth];
        private bool visited = false;
        private int gScore, fScore; //Turn to arrays
        

        //Path Queue for reading path
        private Queue<int> pathQueue = new Queue<int>();
        


        //Point to go to.
        private int nodeX, nodeY;
        private const int tilesize = 50;
        private const int walkable = 0, unwalkable = 1;
        private const int mapWidth = 16, mapHeight = 12;
       // private const int found = 1, nonexistent = 2;
        private bool enemyStopped;

        //OLD
       // private int[] Fcost = new int[mapWidth * mapHeight + 1];
        //private int[,] Gcost = new int[mapWidth+1, mapHeight+1];
        //private int[] Hcost = new int[mapWidth * mapHeight + 1];
        //private int[] openList = new int[mapWidth * mapHeight + 1];
        //private int[,] whichList = new int[mapWidth+1, mapHeight+1];
        //private int[] openX = new int[mapWidth * mapHeight + 1];
        //private int[] openY = new int[mapWidth * mapHeight + 1];
        //private int[,] parentX = new int[mapWidth, mapHeight];
        //private int[,] parentY = new int[mapWidth, mapHeight];
        //private Queue<int> pathBank;
        //private int pathLength;
        //private int pathLocation;
        //private int onClosedList = 10; //Change this when we know what the fuck it does

        //For path-reading
        //private int pathStatus;
        //private int xPath;
        //private int yPath;

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
                        FindPath((int)(X), (int)(Y), nodeX, nodeY);
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

    //    public int FindPath(int startingX, int startingY, int targetX, int targetY)
    //    {
    //        int onOpenList = 0, parentXval = 0, parentYval = 0, temp = 0, corner = 0, numberOfOpenListItems = 0,
    //addedGCost = 0, path = 0, pathX, pathY, cellPosition,
    //newOpenListItemID = 0;

    //        int startX = (startingX / tilesize);
    //        int startY = (startingY / tilesize);
    //        targetX = (targetX / tilesize);
    //        targetY = (targetY / tilesize);

    //        Out.WriteLine("Found target at tile: [" + targetX + "," + targetY + "]");
    //        DebugProperty["Target Tile"] = "[" + targetX + "," + targetY + "]";
    //        //Not sure why I need this:
    //        //Resettings stuff
    //        if (onClosedList > 1000000)
    //        {
    //            for (int x = 0; x < mapWidth; x++)
    //            {
    //                for (int y = 0; y < mapHeight; y++)
    //                    whichList[x, y] = 0;
    //            }
    //            onClosedList = 10;
    //        }
    //        onClosedList += 2;
    //        onOpenList = onClosedList - 1;
    //        pathLength = 0;//i.e, = not started
    //        pathLocation = 0;//i.e, = not started
    //        Gcost[startX, startY] = 0; //reset starting square's G value to 0
    //        //Add start location to the open list
    //        numberOfOpenListItems = 1;
    //        openList[1] = 1;//assign it as the top (and currently only) item in the open list, which is maintained as a binary heap (explained below)
    //        openX[1] = startX; openY[1] = startY;
    //        do
    //        {
    //            if (numberOfOpenListItems != 0)
    //            {
    //                parentXval = openX[openList[1]];
    //                parentYval = openY[openList[1]];
    //                whichList[parentXval, parentYval] = onClosedList;

    //                numberOfOpenListItems -= 1;

    //                openList[1] = openList[numberOfOpenListItems + 1];
    //                int v = 1;
    //                int u;

    //                do
    //                {
    //                    u = v;
    //                    if (2 * u + 1 <= numberOfOpenListItems)
    //                    {
    //                        if (Fcost[openList[u]] >= Fcost[openList[2 * u]])
    //                        {
    //                            v = 2 * u;
    //                        }
    //                        if (Fcost[openList[v]] >= Fcost[openList[2 * u + 1]])
    //                        {
    //                            v = 2 * u + 1;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        if (2 * u <= numberOfOpenListItems)
    //                        {
    //                            if (Fcost[openList[u]] >= Fcost[openList[2 * u]])
    //                            {
    //                                v = 2 * u;
    //                            }
    //                        }
    //                    }
    //                    if (u != v)
    //                    {
    //                        temp = openList[u];
    //                        openList[u] = openList[v];
    //                        openList[v] = temp;
    //                    }
    //                } while (u != v);

    //                for (int b = parentYval - 1; b <= parentYval + 1; b++)
    //                {
    //                    for (int a = parentXval - 1; a <= parentXval + 1; b++)
    //                    {
    //                        if (a >= 0 && b >= 0 && a <= mapWidth && b <= mapHeight)
    //                        {
    //                            if (whichList[a, b] != onClosedList)
    //                            {
    //                                if (collisionMap[a, b] != unwalkable)
    //                                {
    //                                    corner = walkable;
    //                                    if (a == parentYval - 1)
    //                                    {
    //                                        if (b == parentYval - 1)
    //                                        {
    //                                            if (collisionMap[parentXval - 1, parentYval] == unwalkable || collisionMap[parentXval, parentYval - 1] == unwalkable)
    //                                            {
    //                                                corner = unwalkable;
    //                                            }
    //                                        }
    //                                        else if (b == parentYval + 1)
    //                                        {
    //                                            if (collisionMap[parentXval, parentYval + 1] == unwalkable || collisionMap[parentXval - 1, parentYval] == unwalkable)
    //                                            {
    //                                                corner = unwalkable;
    //                                            }
    //                                        }
    //                                    }
    //                                    else if (a == parentXval + 1)
    //                                    {
    //                                        if (b == parentYval - 1)
    //                                        {
    //                                            if (collisionMap[parentXval, parentYval - 1] == unwalkable || collisionMap[parentXval + 1, parentYval] == unwalkable)
    //                                                corner = unwalkable;
    //                                        }
    //                                        else if (b == parentYval + 1)
    //                                        {
    //                                            if (collisionMap[parentXval + 1, parentYval] == unwalkable || collisionMap[parentXval, parentYval + 1] == unwalkable)
    //                                                corner = unwalkable;
    //                                        }
    //                                    }
    //                                    if (corner == walkable)
    //                                    {
    //                                        int m;
    //                                        if (whichList[a, b] != onOpenList)
    //                                        {
    //                                            newOpenListItemID += 1;
    //                                            m = numberOfOpenListItems + 1;
    //                                            openList[m] = newOpenListItemID;
    //                                            openX[newOpenListItemID] = a;
    //                                            openY[newOpenListItemID] = b;
    //                                            if (Math.Abs(a - parentXval) == 1 && Math.Abs(b - parentYval) == 1)
    //                                            {
    //                                                addedGCost = 14;
    //                                            }
    //                                            else
    //                                            {
    //                                                addedGCost = 10;
    //                                            }
    //                                            Gcost[a, b] = Gcost[parentXval, parentYval] + addedGCost; 
    //                                            Hcost[openList[m]] = 10 * (Math.Abs(a - targetX) + Math.Abs(b - targetY));
    //                                            Fcost[openList[m]] = Gcost[a, b] + Hcost[openList[m]];
    //                                            parentX[a, b] = parentXval;
    //                                            parentY[a, b] = parentYval;
    //                                            while (m != 1)
    //                                            {
    //                                                if (Fcost[openList[m]] <= Fcost[openList[m / 2]])
    //                                                {
    //                                                    temp = openList[m / 2];
    //                                                    openList[m / 2] = openList[m];
    //                                                    openList[m] = temp;
    //                                                    m /= 2;
    //                                                }
    //                                                else
    //                                                {
    //                                                    break;
    //                                                }
    //                                            }
    //                                            numberOfOpenListItems += 1;

    //                                            whichList[a, b] = onOpenList;
    //                                        }
    //                                        else
    //                                        {
    //                                            if (Math.Abs(a - parentXval) == 1 && Math.Abs(b - parentYval) == 1)
    //                                            {
    //                                                addedGCost = 14;
    //                                            }
    //                                            else
    //                                            {
    //                                                addedGCost = 10;
    //                                            }
    //                                            int tempGcost = Gcost[parentXval, parentYval] + addedGCost;

    //                                            if (tempGcost < Gcost[a, b])
    //                                            {
    //                                                parentX[a, b] = parentXval;
    //                                                parentY[a, b] = parentYval;
    //                                                Gcost[a, b] = tempGcost;
    //                                                for (int x = 1; x <= numberOfOpenListItems; x++)
    //                                                {
    //                                                    if (openX[openList[x]] == a && openY[openList[x]] == b)
    //                                                    {
    //                                                        Fcost[openList[x]] = Gcost[a, b] + Hcost[openList[x]];

    //                                                        m = x;
    //                                                        while (m != 1)
    //                                                        {
    //                                                            if (Fcost[openList[m]] < Fcost[openList[m / 2]])
    //                                                            {
    //                                                                temp = openList[m / 2];
    //                                                                openList[m / 2] = openList[m];
    //                                                                openList[m] = temp;
    //                                                                m /= 2;
    //                                                            }
    //                                                            else
    //                                                            {
    //                                                                break;
    //                                                            }
    //                                                        }
    //                                                        break;
    //                                                    }
    //                                                }
    //                                            }
    //                                        }
    //                                    }
    //                                }
    //                            }
    //                        }
    //                        else
    //                        {
    //                        }
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                path = nonexistent;
    //                break;
    //            }
    //            if (whichList[targetX, targetY] == onOpenList)
    //            {
    //                path = found;
    //                break;
    //            }
    //        } while (true);
    //        if (path == found)
    //        {
    //            pathX = targetX;
    //            pathY = targetY;

    //            //This shit is all backwards...

    //                do
    //                {
    //                    int tempx = parentX[pathX, pathY];
    //                    pathY = parentY[pathX, pathY];
    //                    pathX = tempx;

    //                    pathLength += 1;
    //                } while (pathX != startX || pathY != startY);

    //                pathX = targetX;
    //                pathY = targetY;
    //                cellPosition = pathLength * 2;

    //                //So is this...

    //                do
    //                {
    //                    pathBank.Enqueue(pathX);
    //                    pathBank.Enqueue(pathY);

    //                    int tempx = parentX[pathX, pathY];
    //                    pathY = parentY[pathX, pathY];
    //                    pathX = tempx;
    //                } while (pathX != startX || pathY != startY);

    //                ReadPath(startingX, startingY);
    //            }
    //        return path;
    //    }

        public bool FindPath(int startX, int startY, int targetX, int targetY)
        {
            int i = 0, n = 0;
            //Set every Node to not visited, i.e 0.
            for (int j = 0; j < checkedNodes.GetLength(0); j++)
            {
                for (int k = 0; k < checkedNodes.GetLength(1); k++)
                {
                    checkedNodes[j, k] = 0;
                    Out.WriteLine("J:" + (j+1) + " K:" + (k+1));
                    
                }
            }
           
            //DEBUG
            targetX /= tilesize;
            targetY /= tilesize;
            startX /= tilesize;
            startY /= tilesize;

            Out.WriteLine("Start:[" + (startX+1) +"," + (startY+1) + "]");
            Out.WriteLine("Target:[" + (targetX+1) + "," + (targetY+1) + "]");

            checkedNodes[startY, startX] = 1;
            checkedNodes[targetY, targetX] = 1;

            for (int x = 0; x < checkedNodes.GetLength(1); x++)
            {
                Out.Write("[");
                for (int y = 0; y < checkedNodes.GetLength(0); y++)
                {
                    Out.Write(checkedNodes[x, y] + ",");
                }
                Out.WriteLine("]");
            }
            //uʍop ǝpısdn pǝddıןɟ sı ʇıɥs sıɥʇ


            queuedNodes.Enqueue(startX);
            queuedNodes.Enqueue(startY);

            
            #region PSEUDO
            /*
             function A*(start,goal)
                closedset := the empty set    // The set of nodes already evaluated.
                openset := {start}    // The set of tentative nodes to be evaluated, initially containing the start node
                came_from := the empty map    // The map of navigated nodes.
 
                g_score[start] := 0    // Cost from start along best known path.
                // Estimated total cost from start to goal through y.
                f_score[start] := g_score[start] + heuristic_cost_estimate(start, goal)
 
                while openset is not empty
                    current := the node in openset having the lowest f_score[] value
                    if current = goal
                        return reconstruct_path(came_from, goal)
 
                    remove current from openset
                    add current to closedset
                    for each neighbor in neighbor_nodes(current)
                        if neighbor in closedset
                            continue
                        tentative_g_score := g_score[current] + dist_between(current,neighbor)
 
                        if neighbor not in openset or tentative_g_score < g_score[neighbor] 
                            came_from[neighbor] := current
                            g_score[neighbor] := tentative_g_score
                            f_score[neighbor] := g_score[neighbor] + heuristic_cost_estimate(neighbor, goal)
                            if neighbor not in openset
                                add neighbor to openset
                return failure
             */
            #endregion PSEUDO
            #region PG4400 Pseudo
        //Pseudo-kode for A*:
        //o Oppstart: MARK noder ”NOT visited”, etc.
        //o Initialiser målnode, startnode, m.m.
        //o ADD startnode til køen (traveled = 0.0).
        //o MARK startnode ”visited”.
        //o WHILE (queued list ikke tom)
        //o SET current = første fra køen, DELETE.
        //o IF (current == mål)
        //o TERMINATE with SUCCESS.
        //o ADD current til visitedList.
        //o FOR (hver node tilkoplet current ("neighbour"))
        //o Do the A* distance comparisons,
        //add (sorted by distance) if not seen before
        //or if shorter path.
        //o TERMINATE with FAILURE.
            #endregion
            return false;
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
