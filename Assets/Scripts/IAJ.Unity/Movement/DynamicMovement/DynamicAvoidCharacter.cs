using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    class DynamicAvoidCharacter : DynamicMovement
    {
        public override string Name
        {
            get { return "AvoidCharacter"; }
        }

        public DynamicAvoidCharacter(KinematicData t)
        {
            this.Target = t;
        }
        public override KinematicData Target { get; set; }

        public float AvoidMargin { get; set; }

        public float MaxTimeLookAhead { get; set; }

        public override MovementOutput GetMovement()
        {
            var output = new MovementOutput();

            var deltaPos = Target.position - Character.position;
            var deltaVel = Target.velocity - Character.velocity;
            var deltaSpeed = deltaVel.magnitude;

            if (deltaSpeed == 0) return output;

            var timeToClosest = -(Vector3.Dot(deltaPos, deltaVel)) / (deltaSpeed * deltaSpeed);

            if (timeToClosest > MaxTimeLookAhead) return output;

            var futureDeltaPos = deltaPos + deltaVel * timeToClosest;
            var futureDistance = futureDeltaPos.magnitude;
            if (futureDistance > 2 * AvoidMargin) return output;

            if (futureDistance <= 0 || deltaPos.magnitude < 2 * AvoidMargin)
                output.linear = Character.position - Target.position;
            else
                output.linear = futureDeltaPos * -1;

            output.linear = output.linear.normalized * MaxAcceleration;
            Debug.DrawRay(this.Character.position, output.linear.normalized * 3.0f, MovementDebugColor);
            return output;
        }
    }
}
