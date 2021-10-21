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
        protected bool initialPositionsSet = false;
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
        abstract public void OnLateUpdateCallback();

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
                GetPosRotRelativeToParent(out initialPositionToParent, out initialRotationToParent);
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
                EditorLogic.VAsys?.NeedUpdate();
            }
            OnLateUpdateCallback();
        }

        public void RotateAround(Vector3 rotPoint, Vector3 rotAxis, float angle)
        {
            Quaternion rotAngle = Quaternion.AngleAxis(angle, rotAxis);
            Vector3 endPos = ((ParentTransform.position + initialRotationToParent * initialPositionToParent - rotPoint)) + rotPoint;
            Quaternion endRot = rotAngle * ParentTransform.rotation * initialRotationToParent;
            part.SetPosRotRecursive(endPos, endRot);
        }

        public void ResetToInitial()
        {
            if (initialPositionsSet && ParentTransform != null)
            {
                part.SetPosRotRecursive(initialRotationToParent * initialPositionToParent + ParentTransform.position, ParentTransform.rotation * initialRotationToParent);
            }
        }

        public void OnPartBeginMove(Part part)
        {
            moving = true;
            OnPartBeginMoveCallback();
        }

        public void OnPartMoving(Part part, ref Vector3 wpos, ref Quaternion wrot)
        {
            OnPartMovingCallback();
        }

        public void OnPartEndMove(Part part)
        {
            OnPartEndMoveCallback();
            finishedMoving = true;
            moving = false;
        }

        public Transform ParentTransform
        {
            get { return part.Parent?.transform; }
        }

        // currentRot = parentRot * rotToParent
        // parentRot.Inverse() * currentRot = rotToParent
        public void GetPosRotRelativeToParent(out Vector3 positionToParent, out Quaternion rotationToParent)
        {
            positionToParent = transform.position - ParentTransform.position;
            rotationToParent = ParentTransform.rotation.Inverse() * transform.rotation;
        }

    }
}
