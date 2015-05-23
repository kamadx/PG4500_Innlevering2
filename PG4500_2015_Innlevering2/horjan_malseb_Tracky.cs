using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robocode;
using Robocode.Util;


namespace PG4500_2015_Innlevering2
{
	public class horjan_malseb_Tracky : AdvancedRobot
	{
		/*
		 TODO: Find target
		 TODO: DEBUG: Just go target, then wait for new target. Repeat.
		 TODO: A* Algorithm
		 TODO: Draw path
		 TODO: Traverse path
		 TODO: Win
		 */

		//Point to go to.
		public double nodeX, nodeY;

		private RobotStatus robotStatus;

		public override void Run()
		{
			//Startup - Go to (25, 25) and wait.
			nodeX = 25;
			nodeY = 25;
			GoToPoint(nodeX, nodeY);
			Out.Write("#{0}\t{1}\n", Time, "Heading to (25, 25).");
			WaitFor(new MoveCompleteCondition(this));
			Out.Write("#{0}\t{1}\n", Time, "Arrived at (" + X.ToString("F") + "," + Y.ToString("F") + ").");
			//SetTurnRight(180);

			SetTurnRadarRightRadians(Double.PositiveInfinity);
			//Main Loop
			while (true)
			{
				
				if (Velocity == 0)
				{
					GoToPoint(nodeX, nodeY);
					WaitFor(new MoveCompleteCondition(this));
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
			double angle = Utils.NormalRelativeAngle(Math.Atan2(pointX, pointY) - HeadingRadians);

			double turnAngle = Math.Atan(Math.Tan(angle));
			SetTurnRightRadians(turnAngle);
			WaitFor(new TurnCompleteCondition(this));
			SetAhead(distance * (angle == turnAngle ? 1 : -1));
			Execute();
		}

		public override void OnStatus(StatusEvent e)
		{
			robotStatus = e.Status;
		}

		public override void OnScannedRobot(ScannedRobotEvent e)
		{
			double radarTurn = HeadingRadians + e.BearingRadians - RadarHeadingRadians;
			SetTurnRadarRightRadians(Utils.NormalRelativeAngle(radarTurn));

			if (e.Velocity == 0 && Velocity == 0)
			{
				FindEnemyCoords(e);
			}
		}

		public void FindEnemyCoords(ScannedRobotEvent e)
		{
			double angleToEnemy = e.Bearing;

			// Calculate the angle to the scanned robot
			double angle = ToRad(robotStatus.Heading + angleToEnemy % 360);

			// Calculate the coordinates of the robot
			nodeX = (robotStatus.X + Math.Sin(angle) * e.Distance);
			nodeY = (robotStatus.Y + Math.Cos(angle) * e.Distance);
		}

		public double ToRad(double angle)
		{
			return (Math.PI / 180) * angle;
		}


	}
}
