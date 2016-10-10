using System;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.Util;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicAvoidObstacle : DynamicSeek
    {
        public DynamicAvoidObstacle(GameObject obstacle)
        {
            CollisionDetector = obstacle.GetComponent<Collider>();
        }

        public Collider CollisionDetector { get; set; }

        public float AvoidMargin { get; set; }

        public float MaxLookAhead { get; set; }

        public float WhiskersLength { get; set; }

        public float WhiskersSpan { get; set; }

        public override MovementOutput GetMovement()
        {
            var centralRay = this.Character.velocity.normalized;
            Debug.DrawRay(this.Character.position, centralRay * MaxLookAhead, MovementDebugColor);
            var leftWhisker = this.Character.velocity.normalized;
            leftWhisker = Quaternion.Euler(0, WhiskersSpan / 2 * -1, 0) * leftWhisker;
            Debug.DrawRay(this.Character.position, leftWhisker * WhiskersLength, MovementDebugColor);
            var rightWhisker = this.Character.velocity.normalized;
            rightWhisker = Quaternion.Euler(0, WhiskersSpan / 2, 0) * rightWhisker;
            Debug.DrawRay(this.Character.position, rightWhisker * WhiskersLength, MovementDebugColor);

            Ray RayCenter;
            bool CollisionCenter;
            RaycastHit HitCenter;

            Ray RayLeft;
            bool CollisionLeft;
            RaycastHit HitLeft;

            Ray RayRight;
            bool CollisionRight;
            RaycastHit HitRight;

            if (centralRay.magnitude != 0)
            {
                RayCenter = new Ray(this.Character.position, centralRay.normalized);
                CollisionCenter = CollisionDetector.Raycast(RayCenter, out HitCenter, MaxLookAhead);

                if (CollisionCenter)
                {
                    this.Target = new KinematicData();
                    this.Target.position = HitCenter.point + HitCenter.normal * AvoidMargin;
                    return base.GetMovement();
                }
            }
            if (leftWhisker.magnitude != 0)
            {
                RayLeft = new Ray(this.Character.position, leftWhisker.normalized);
                CollisionLeft = CollisionDetector.Raycast(RayLeft, out HitLeft, WhiskersLength);

                if (CollisionLeft)
                {
                    this.Target = new KinematicData();
                    this.Target.position = HitLeft.point + HitLeft.normal * AvoidMargin;
                    return base.GetMovement();
                }
            }
            if (rightWhisker.magnitude != 0)
            {
                RayRight = new Ray(this.Character.position, rightWhisker.normalized);
                CollisionRight = CollisionDetector.Raycast(RayRight, out HitRight, WhiskersLength);

                if (CollisionRight)
                {
                    this.Target = new KinematicData();
                    this.Target.position = HitRight.point + HitRight.normal * AvoidMargin;
                    return base.GetMovement();
                }
            }
            return new MovementOutput();
        }
    }
}

