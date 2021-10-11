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

        public override void OnModuleSpawn()
        {
            
        }

        protected string GetModuleName() => "CloverTech.Rotator";
        public override string GetActionName() => "Rotator";

        public override void ActuatorFixedUpdate()
        {
            if (!parentRotationPointSet)
            {
                parentRotationPointSet = true;
                parentRotationPoint = part.Parent.transform.InverseTransformPoint(transform.TransformPoint(part.attachPoint));
            }
            if (!parentAxisSet)
            {
                parentAxisSet = true;
                parentAxis = part.Parent.transform.InverseTransformDirection(transform.TransformDirection(new Vector3(1.0f, 0.0f, 0.0f).normalized));
            }

            Vector3 worldAxis = part.Parent.transform.TransformDirection(parentAxis);
            Vector3 worldPoint = part.Parent.transform.TransformPoint(parentRotationPoint);
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            transform.localPosition = initialPositionToParent;
            transform.localRotation = initialRotationToParent;
            transform.RotateAround(worldPoint, worldAxis, currentAngle);
            Vector3 endPos = transform.position;
            Quaternion endRot = transform.rotation;
            transform.rotation = startRot;
            transform.position = startPos;

            Quaternion otherRot = part.Parent.transform.rotation * initialRotationToParent * Quaternion.Euler(0, currentAngle, 0);
            //Debug.LogError($"{transCopy} and {otherRot}");
            part.SetPosRotRecursive(endPos, endRot);

            //AbsoluteRotateAround(worldPoint, worldAxis, currentAngle);

            //transform.localRotation = Quaternion.Euler(0, currentAngle, 0);

        }

        public override void OnReceiveAxisState(float axis)
        {
            axis = (axis + 1.0f)*0.5f;
            currentAngle = Mathf.Lerp(minAngle, maxAngle, axis);
        }
    }

}
