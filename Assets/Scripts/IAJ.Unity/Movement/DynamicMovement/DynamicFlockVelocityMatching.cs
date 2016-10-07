using System;
using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Util;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
	public class DynamicFlockVelocityMatching : DynamicVelocityMatch
	{
		public DynamicFlockVelocityMatching ()
		{
		}

		public override string Name
		{
			get { return "Flock Velocity Matching"; }
		}


		public List<KinematicData> flock { get; set; }

		public float radius { get; set; }

		public float fanAngle { get; set; }

		public float shortestAngleDifference( float source, float target )
		{
			var delta = target - source;
			if (delta > MathConstants.MATH_PI)
				delta -= 360;
			else {
				if (delta < -MathConstants.MATH_PI)
					delta += 360;
			}
			return delta;
		}

		public override MovementOutput GetMovement ()
		{
			var averageVelocity = new Vector3 ();
			var closeBoids = 0;

			foreach (KinematicData boid in flock) {
				if (this.Character != boid) {
					var direction = boid.position - this.Character.position;
					if (direction.magnitude <= radius) {
						var angle = MathHelper.ConvertVectorToOrientation (direction);
						var angleDifference = shortestAngleDifference (this.Character.orientation, angle);

						if (Math.Abs (angleDifference) <= fanAngle) {
							averageVelocity += boid.velocity;
							closeBoids++;
						}
					}
				}
			}

			if (closeBoids == 0)
				return new MovementOutput ();

			averageVelocity /= closeBoids;
			this.Target.velocity = averageVelocity;

			return base.GetMovement ();
		}
	}
}

