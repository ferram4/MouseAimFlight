/*
Copyright (c) 2016, BahamutoD, ferram4, tetryds
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
 
* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
