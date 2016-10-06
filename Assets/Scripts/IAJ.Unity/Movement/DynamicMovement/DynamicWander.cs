using Assets.Scripts.IAJ.Unity.Util;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
	public class DynamicWander : DynamicSeek
	{
		public DynamicWander()
		{
			this.Target = new KinematicData();
			TurnAngle = MathConstants.MATH_PI / 8;
			WanderOffset = 5.0f;
			WanderRadius = 3.0f;
			WanderOrientation = 0;
		}
		public override string Name
		{
			get { return "Wander"; }
		}
		public float TurnAngle { get; private set; }
		public float WanderOffset { get; private set; }
		public float WanderRadius { get; private set; }
		protected float WanderOrientation { get; private set; }
		public override MovementOutput GetMovement()
		{

			WanderOrientation += RandomHelper.RandomBinomial() * TurnAngle;
			var newOrientation = WanderOrientation + this.Character.orientation;
			var circleCenter = this.Character.position + WanderOffset * this.Character.GetOrientationAsVector();
			this.Target.position = circleCenter + WanderRadius * MathHelper.ConvertOrientationToVector(newOrientation);
			return base.GetMovement();
		}
	}
}
