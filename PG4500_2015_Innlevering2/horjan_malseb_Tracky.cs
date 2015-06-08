﻿using Robocode;
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
		private bool enemyStopped;


        private int[] Fcost = new int[mapWidth * mapHeight + 2];
        private int[,] Gcost = new int[mapWidth + 1, mapHeight + 1];
        private int[] Hcost = new int[mapWidth * mapHeight + 2];

        private RobotStatus robotStatus;

		public override void Run()
		{
			enemyStopped = false;
			//Startup - Go to (25, 25) and wait.
			nodeX = 25;
			nodeY = 25;
			DebugProperty["Headed to coord"] = "(" + nodeX.ToString() + "," + nodeY.ToString() + ")";
            DebugProperty["Headed to tile"] = "(" + nodeX / tilesize + "," + nodeY / tilesize + ")";
			GoToPoint(nodeX, nodeY);
			WaitFor(new MoveCompleteCondition(this));
			Out.Write("#{0}\t{1}\n", Time, "Arrived at (" + X.ToString() + "," + Y.ToString() + ").");
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
                        DebugProperty["Headed to tile"] = "(" + nodeX / tilesize + "," + nodeY / tilesize + ")";
						GoToPoint(nodeX, nodeY);
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

        public void FindPath(int startingX, int startingY, int targetX, int targetY)
        {
            int onOpenList = 0, parentXval = 0, parentYval = 0, a = 0, b = 0, m = 0, u = 0, v = 0, temp = 0, corner = 0, numberOfOpenListItems = 0,
    addedGCost = 0, tempGcost = 0, path = 0,
    tempx, pathX, pathY, cellPosition,
    newOpenListItemID = 0;

            int startX = startingX / tilesize;
            int startY = startingY / tilesize;
            targetX = targetX / tilesize;
            targetY = targetY / tilesize;

            

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
