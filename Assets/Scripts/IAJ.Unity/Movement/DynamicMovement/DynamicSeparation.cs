using System;
using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Util;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
	public class DynamicSeparation : DynamicMovement
	{
		public DynamicSeparation ()
		{
		}

		public override string Name
		{
			get { return "Separation"; }
		}

		public override KinematicData Target { get; set; }

		public List<KinematicData> flock { get; set; }

		public float separationFactor { get; set; }

		public float radius { get; set; }

		public float maxAcceleration { get; set; }


		public override MovementOutput GetMovement()
		{
			var output = new MovementOutput ();

			foreach(KinematicData boid in flock)
			{
				if (boid != this.Character) 
				{
					var direction = this.Character.position - boid.position;
					var distance = MathHelper.ConvertVectorToOrientation (direction);
					if (direction.magnitude < radius) {
						var separationStrength = Math.Min (separationFactor / distance * distance, maxAcceleration);
						direction.Normalize ();
						output.linear += direction * separationStrength;
					}
				}
			}

			if (MathHelper.ConvertVectorToOrientation(output.linear) > maxAcceleration) {
				output.linear.Normalize ();
				output.linear *= maxAcceleration;
			}

			return output;
		}


	}
}

