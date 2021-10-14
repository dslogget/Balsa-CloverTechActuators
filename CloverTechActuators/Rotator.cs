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
        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        public Vector3 parentAxis;
        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        bool parentAxisSet = false;
        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        public Vector3 parentRotationPoint;
        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        bool parentRotationPointSet = false;

        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        public float angleMultiplier = 4.0f;


        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        public float maxAngle = 120;
        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        public float minAngle = -120;
        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        public float currentAngle = 0;

        private float lastAngle = 0;



        protected string GetModuleName() => "CloverTech.Rotator";
        public override string GetActionName() => "Rotator";

        public override void ActuatorFixedUpdate()
        {
            if (moving)
            {
                return;
            }

            if (finishedMoving && !parentRotationPointSet)
            {
                parentRotationPointSet = true;
                parentRotationPoint = part.Parent.transform.InverseTransformPoint(transform.TransformPoint(part.attachPoint));
            }
            if (finishedMoving && !parentAxisSet)
            {
                parentAxisSet = true;
                parentAxis = part.Parent.transform.InverseTransformDirection(transform.TransformDirection(new Vector3(1.0f, 0.0f, 0.0f).normalized));
            }

            if (lastAngle != currentAngle)
            {
                Vector3 worldAxis = part.Parent.transform.TransformDirection(parentAxis);
                Vector3 worldPoint = part.Parent.transform.TransformPoint(parentRotationPoint);
                RotateAround(worldPoint, worldAxis, currentAngle);
                needUpdate = true;
            }
        }

        public override void OnReceiveAxisState(float axis)
        {
            axis = (axis + 1.0f)*0.5f;
            currentAngle = Mathf.Lerp(minAngle, maxAngle, axis);
        }

        public override void OnPartBeginMoveCallback()
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            transform.localPosition = initialPositionToParent;
            transform.localRotation = initialRotationToParent;
            Vector3 endPos = transform.position;
            Quaternion endRot = transform.rotation;
            transform.rotation = startRot;
            transform.position = startPos;
            part.SetPosRotRecursive(endPos, endRot);

            return;
        }

        public override void OnPartMovingCallback()
        {
            return;
        }

        public override void OnPartEndMoveCallback()
        {
            return;
        }
    }

}
