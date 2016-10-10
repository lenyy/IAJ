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

            RaycastHit hit;
            Ray ray = new Ray(this.Character.position, centralRay.normalized);
            var collision = CollisionDetector.Raycast(ray, out hit, MaxLookAhead);

            if (!collision)
            {
                ray = new Ray(this.Character.position, leftWhisker.normalized);
                collision = CollisionDetector.Raycast(ray, out hit, WhiskersLength);
                if (!collision)
                {
                    ray = new Ray(this.Character.position, rightWhisker.normalized);
                    collision = CollisionDetector.Raycast(ray, out hit, WhiskersLength);
                    if (!collision)
                    {
                        return new MovementOutput();
                    }
                }
            }

            this.Target = new KinematicData();
            this.Target.position = hit.point + hit.normal * AvoidMargin;
            return base.GetMovement();
        }
    }
}

