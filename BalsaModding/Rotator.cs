using BalsaCore;
using BalsaCore.FX;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Modules;
using CfgFields;
using FSControl;

namespace CloverTech 
{ 

    public class Rotator : Actuator
    {
        private FSInputState inputState;
        private float diffAngle = 0;

        private Quaternion originalRotation;

        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        public Vector3 parentAxis;
        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        public float angleMultiplier = 4.0f;
        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        public Vector3 parentRotationPoint;

        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        public float maxAngle = 120;
        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        public float minAngle = -120;
        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        public float currentAngle = 0;

        public override void OnModuleSpawn()
        {
            
        }

        protected string GetModuleName() => "CloverTech.Rotator";
        public override string GetActionName() => "Rotator";

        public override void ActuatorFixedUpdate()
        {
            if (parentRotationPoint == null)
            {
                parentRotationPoint = part.Parent.transform.InverseTransformPoint(transform.TransformPoint(part.attachPoint));
            }
            if (parentAxis == null)
            {
                parentAxis = part.Parent.transform.InverseTransformDirection(transform.TransformDirection(0, 1, 0));
            }
            
            Vector3 worldAxis = part.Parent.transform.TransformDirection(parentAxis);
            Vector3 worldPoint = part.Parent.transform.TransformPoint(parentRotationPoint);
            AbsoluteRotateAround(worldPoint, worldAxis, currentAngle);

        }

        public override void OnReceiveAxisState(float axis)
        {
            currentAngle = Mathf.LerpAngle(minAngle, maxAngle, axis);
        }
    }

}
