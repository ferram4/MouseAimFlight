/*
Copyright (c) 2016, ferram4, tetryds
All rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MouseAimFlight
{
    class FlightBehavior
    {
        public FlightBehavior()
        {
            //Behavior setup, GUI interaction
        }
        public ErrorData normalFlight(Transform vesselTransform, Vector3d targetDirection, Vector3d targetDirectionYaw, Vector3 targetPosition, Vector3 upDirection, float upWeighting, Vessel vessel)
        {
            float pitchError;
            float rollError;
            float yawError;

            float sideslip;

            sideslip = (float)Math.Asin(Vector3.Dot(vesselTransform.right, vessel.srf_velocity.normalized)) * Mathf.Rad2Deg;

            pitchError = (float)Math.Asin(Vector3d.Dot(Vector3d.back, VectorUtils.Vector3dProjectOnPlane(targetDirection, Vector3d.right))) * Mathf.Rad2Deg;
            yawError = (float)Math.Asin(Vector3d.Dot(Vector3d.right, VectorUtils.Vector3dProjectOnPlane(targetDirectionYaw, Vector3d.forward))) * Mathf.Rad2Deg;

            //roll
            Vector3 currentRoll = -vesselTransform.forward;
            Vector3 rollTarget;

            rollTarget = (targetPosition + Mathf.Clamp(upWeighting * (100f - Math.Abs(yawError * 1.6f) - (pitchError * 2.8f)), 0, float.PositiveInfinity) * upDirection) - vessel.CoM;
            
            rollTarget = Vector3.ProjectOnPlane(rollTarget, vesselTransform.up);

            rollError = VectorUtils.SignedAngle(currentRoll, rollTarget, vesselTransform.right) - sideslip * (float)Math.Sqrt(vessel.srf_velocity.magnitude) / 5;

            ErrorData behavior = new ErrorData(pitchError, rollError, yawError);
            
            return behavior;
        }

    }
    public struct ErrorData
    {
        public float pitchError;
        public float rollError;
        public float yawError;

        public ErrorData(float p, float r, float y)
        {
            pitchError = p;
            rollError = r;
            yawError = y;
        }
    }
}
