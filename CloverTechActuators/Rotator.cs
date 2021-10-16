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
using GameEventsData;
using UnityEngine.Events;

namespace CloverTech 
{ 

    public class Rotator : Actuator
    {
        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        public Vector3 axis = new Vector3(0,0,1);
        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        public Vector3 pivotPoint = new Vector3(0, 0, 0);
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

        public void Awake()
        {
            GameEvents.Game.OnSceneTransitionStart.AddListener(new UnityAction<FromToAction<GameScenes>>(this.OnSceneLoadBegin));
            GameEvents.Game.OnSceneTransitionComplete.AddListener(new UnityAction<FromToAction<GameScenes>>(this.OnSceneLoadEnd));
        }

        public void OnDestroy()
        {
            GameEvents.Game.OnSceneTransitionStart.RemoveListener(new UnityAction<FromToAction<GameScenes>>(this.OnSceneLoadBegin));
            GameEvents.Game.OnSceneTransitionComplete.RemoveListener(new UnityAction<FromToAction<GameScenes>>(this.OnSceneLoadEnd));
        }

        public override void ActuatorFixedUpdate()
        {
            if (moving)
            {
                return;
            }

            if (finishedMoving)
            {
                parentRotationPoint = part.Parent.transform.InverseTransformPoint(transform.TransformPoint(pivotPoint));
                parentAxis = part.Parent.transform.InverseTransformDirection(transform.TransformDirection(axis.normalized));
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

        private void OnSceneLoadBegin(FromToAction<GameScenes> evt)
        {
            moving = true;
            OnPartBeginMoveCallback();
        }

        private void OnSceneLoadEnd(FromToAction<GameScenes> evt)
        {
            OnPartBeginMoveCallback();
            moving = false;
        }

    }

}
