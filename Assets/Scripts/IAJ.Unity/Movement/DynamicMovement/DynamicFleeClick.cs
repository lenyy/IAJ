using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    class DynamicFleeClick : DynamicFlee
    {
        public override string Name
        {
            get { return "Flee Click"; }
        }

        public Vector3 LastClickPosition { get; set; }

        public override MovementOutput GetMovement()
        {
            this.Target.position = LastClickPosition;

            return base.GetMovement();
        }
    }
}
