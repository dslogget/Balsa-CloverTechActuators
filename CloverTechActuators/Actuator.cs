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
using Construction;

namespace CloverTech
{
    abstract public class Actuator : PartModule, IInputAxisReceiver, IConstructionEvents
    {
        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        bool initialPositionsSet = false;
        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        protected Vector3 initialPositionToParent;
        [SerializeField]
        [CfgField(CfgContext.Config, null, false, null)]
        protected Quaternion initialRotationToParent;

        abstract public void OnReceiveAxisState(float axis);
        abstract public string GetActionName();

        abstract public void ActuatorFixedUpdate();

        abstract public void OnPartBeginMoveCallback();
        abstract public void OnPartMovingCallback();
        abstract public void OnPartEndMoveCallback();

        protected void OnEnable() => PartOps.Add((IConstructionEvents)this);
        protected void OnDisable() => PartOps.Remove((IConstructionEvents)this);


        protected bool moving = false;
        protected bool finishedMoving = true;
        protected bool needUpdate = false;

        public override void OnModuleSpawn()
        {

        }

        public void FixedUpdate()
        {
            if (!this.part.spawned || this.Rb == null || (Object)this.vehicle == (Object)null || !PartModuleUtil.CheckCanApplyForces((PartModule)this) 
                || !this.vehicle.IsAuthorityOrBot || part.Parent == null || part.PtrSelectable.Selected)
                return;

            needUpdate = false;

            if (finishedMoving)
            {
                initialPositionsSet = true;
                initialPositionToParent = transform.localPosition;
                initialRotationToParent = transform.localRotation;
            }

            if (initialPositionsSet)
            {
                ActuatorFixedUpdate();
            }

            finishedMoving = false;
        }

        public void LateUpdate()
        {
            if (needUpdate)
            {
                EditorLogic.VAsys.NeedUpdate();
            }
        }

        public static void RelativeRotateAroundRecursive(Part targetPart, Vector3 worldPoint, Vector3 worldAxis, float angle)
        {
            targetPart.transform.RotateAround(worldPoint, worldAxis, angle);
            foreach (Part link in targetPart.Children)
            {
                RelativeRotateAroundRecursive(link, worldPoint, worldAxis, angle);
            }
        }

        public static void TranslateByRecursive(Part targetPart, Vector3 worldTranslate)
        {
            targetPart.transform.position += worldTranslate;
            foreach (Part link in targetPart.Children)
            {
                TranslateByRecursive(link, worldTranslate);
            }
        }

        public static void TranslateRotateRecursive(Part targetPart, Vector3 trans, Quaternion rot)
        {
            targetPart.transform.position += trans;
            targetPart.transform.rotation = rot * targetPart.transform.rotation;
            foreach (Part link in targetPart.Children)
            {
                TranslateRotateRecursive(link, trans, rot);
            }
        }


        public void RotateAround(Vector3 rotPoint, Vector3 rotAxis, float angle)
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            transform.localPosition = initialPositionToParent;
            transform.localRotation = initialRotationToParent;
            transform.RotateAround(rotPoint, rotAxis, angle);
            Vector3 endPos = transform.position;
            Quaternion endRot = transform.rotation;
            transform.rotation = startRot;
            transform.position = startPos;
            part.SetPosRotRecursive(endPos, endRot);
        }

        public void ResetToInitial()
        {
            if (initialPositionToParent == null)
            {
                return;
            }

            Vector3 diffPos = initialPositionToParent - (transform.position - part.Parent.transform.position);
            Quaternion diffRot = (part.Parent.transform.rotation.Inverse() * transform.rotation).Inverse() * initialRotationToParent;
            TranslateRotateRecursive(part, diffPos, diffRot);
        }

        public void AbsoluteRotateAround(Vector3 worldPoint, Vector3 worldAxis, float angle)
        {
            if (!initialPositionsSet)
            {
                return;
            }

            transform.localPosition = initialPositionToParent;
            transform.localRotation = initialRotationToParent;
            Vector3 prePos = transform.position;
            Quaternion preRot = transform.rotation;
            transform.RotateAround(worldPoint, worldAxis, angle);

            Vector3 diffPos = transform.position - prePos;
            Quaternion diffRot = preRot.Inverse() * transform.rotation;
            foreach (Part link in part.Children)
            {
                TranslateRotateRecursive(link, diffPos, diffRot);
            }
        }

        public void OnPartBeginMove(Part part)
        {
            moving = true;
            OnPartBeginMoveCallback();
        }

        public void OnPartMoving(Part part, ref Vector3 wpos, ref Quaternion wrot)
        {
            //initialPositionsSet = true;
            //initialPositionToParent = transform.localPosition;
            //initialRotationToParent = transform.localRotation;
            OnPartMovingCallback();
        }

        public void OnPartEndMove(Part part)
        {
            //initialPositionsSet = true;
            //initialPositionToParent = transform.localPosition;
            //initialRotationToParent = transform.localRotation;
            OnPartEndMoveCallback();
            finishedMoving = true;
            moving = false;
        }
    }
}
