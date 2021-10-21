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
            rotAxisMarker?.gameObject?.DestroyGameObject();
            rotAxisMarker = null;
        }

        public Construction.UI.Markers.Marker_Ray rotAxisMarker;

        public override void ActuatorFixedUpdate()
        {


            if (moving)
            {
                return;
            }
            //Debug.LogError("test");

            if (finishedMoving)
            {
                parentRotationPoint = ParentTransform.InverseTransformPoint(transform.TransformPoint(pivotPoint));
                parentAxis = ParentTransform.InverseTransformDirection(transform.TransformDirection(axis.normalized));
            }

            if (lastAngle != currentAngle)
            {
                Vector3 worldAxis = ParentTransform.TransformDirection(parentAxis);
                Vector3 worldPoint = ParentTransform.TransformPoint(parentRotationPoint);
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
            ResetToInitial();
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


        public override void OnLateUpdateCallback()
        {
            if (needUpdate && initialPositionsSet) {
                if (rotAxisMarker == null)
                {
                    rotAxisMarker = UnityEngine.Object.Instantiate<Construction.UI.Markers.Marker_Ray>(PrefabBase.OverlayRay_CoL);
                    rotAxisMarker.transform.NestToParent(transform);
                    rotAxisMarker.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    rotAxisMarker.gameObject.SetActive(true);
                }
                if (ParentTransform != null) {
                    rotAxisMarker?.SetMarker(ParentTransform.TransformPoint(parentRotationPoint), ParentTransform.TransformDirection(parentAxis));
                }
            }
        }

    }

}
