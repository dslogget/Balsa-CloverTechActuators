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
    abstract public class Actuator : PartModule, IInputAxisReceiver
    {
        private FSInputState inputState;
        private float angle = 0;
        public Vector3 axis = new Vector3(1, 0, 0);
        private Quaternion originalRotation;
        public float angleMultiplier = 4.0f;

        protected Vector3 initialPositionToParent;
        protected Quaternion initialRotationToParent;

        abstract public void OnReceiveAxisState(float axis);
        abstract public string GetActionName();

        abstract public void ActuatorFixedUpdate();

        public void FixedUpdate()
        {
            if (!this.part.spawned || this.Rb == null || (Object)this.vehicle == (Object)null || !PartModuleUtil.CheckCanApplyForces((PartModule)this) || !this.vehicle.IsAuthorityOrBot || part.Parent == null)
                return;

            if (initialPositionToParent == null)
            {
                initialPositionToParent = transform.position - part.Parent.transform.position;
                initialRotationToParent = part.Parent.transform.rotation.Inverse() * transform.rotation;
            }

            ActuatorFixedUpdate();
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
            targetPart.transform.Translate(worldTranslate);
            foreach (Part link in targetPart.Children)
            {
                TranslateByRecursive(link, worldTranslate);
            }
        }

        public static void TranslateRotateRecursive(Part targetPart, Vector3 trans, Quaternion rot)
        {
            targetPart.transform.Translate(trans);
            targetPart.transform.rotation = rot * targetPart.transform.rotation;
            foreach (Part link in targetPart.Children)
            {
                TranslateRotateRecursive(link, trans, rot);
            }
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
            if (initialPositionToParent == null)
            {
                return;
            }

            Vector3 diffPos = initialPositionToParent - (transform.position - part.Parent.transform.position);
            Quaternion diffRot = (part.Parent.transform.rotation.Inverse() * transform.rotation).Inverse() * initialRotationToParent;
            transform.Translate(diffPos);
            transform.rotation = diffRot * transform.rotation;
            Vector3 prePos = transform.position;
            Quaternion preRot = transform.rotation;
            transform.RotateAround(worldPoint, worldAxis, angle);
            diffPos = transform.position - prePos;
            diffRot = preRot.Inverse() * transform.rotation;
            foreach (Part link in part.Children)
            {
                TranslateRotateRecursive(link, diffPos, diffRot);
            }
        }

    }
}
