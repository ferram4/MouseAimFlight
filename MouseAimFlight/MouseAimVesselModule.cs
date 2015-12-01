using System;
using System.Collections.Generic;
using UnityEngine;

namespace MouseAimFlight
{
    public class MouseAimVesselModule : VesselModule
    {
        Vessel vessel;
        Transform vesselTransform;

        float steerMult = 4;
        float steerInt = 0.1f;
        float steerDamping = 4;

        float pitchIntegrator;
        float yawIntegrator;

        Vector3 upDirection;

        GameObject vobj;
        Transform velocityTransform
        {
            get
            {
                if (!vobj)
                {
                    vobj = new GameObject("velObject");
                    vobj.transform.position = vessel.ReferenceTransform.position;
                    vobj.transform.parent = vessel.ReferenceTransform;
                }

                return vobj.transform;
            }
        }

        void Start()
        {
            vessel = GetComponent<Vessel>();
            vessel.OnAutopilotUpdate += MouseAimPilot;
        }

        void MouseAimPilot(FlightCtrlState s)
        {
            vesselTransform = vessel.ReferenceTransform;

            upDirection = VectorUtils.GetUpDirection(vessel.transform.position);
            Vector3 targetPosition = GetMouseCursorPosition();
            FlyToPosition(s, targetPosition);
        }

        Vector3 GetMouseCursorPosition()
        {
            Vector3 mouseAim = new Vector3(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height, 0);
            Ray ray = FlightCamera.fetch.mainCamera.ViewportPointToRay(mouseAim);
            return (ray.direction * Mathf.Max(15f, (float)vessel.srfSpeed)) + FlightCamera.fetch.mainCamera.transform.position;
        }

        void FlyToPosition(FlightCtrlState s, Vector3 targetPosition)
        {
            Vector3d srfVel = vessel.srf_velocity;
            if (srfVel != Vector3d.zero)
            {
                velocityTransform.rotation = Quaternion.LookRotation(srfVel, -vesselTransform.forward);
            }
            velocityTransform.rotation = Quaternion.AngleAxis(90, velocityTransform.right) * velocityTransform.rotation;
            Vector3 localAngVel = vessel.angularVelocity;

            float angleToTarget = Vector3.Angle(targetPosition - vesselTransform.position, vesselTransform.up);

            //slow down for tighter turns
            float velAngleToTarget = Vector3.Angle(targetPosition - vesselTransform.position, vessel.srf_velocity);
            float normVelAngleToTarget = Mathf.Clamp(velAngleToTarget, 0, 90) / 90;

            Vector3 targetDirection;
            Vector3 targetDirectionYaw;
            float yawError;
            float pitchError;
            float postYawFactor;
            float postPitchFactor;
            //if (steerMode == SteerModes.NormalFlight)
            {
                targetDirection = velocityTransform.InverseTransformDirection(targetPosition - velocityTransform.position).normalized;
                targetDirection = Vector3.RotateTowards(Vector3.up, targetDirection, 45 * Mathf.Deg2Rad, 0);

                targetDirectionYaw = vesselTransform.InverseTransformDirection(vessel.srf_velocity).normalized;
                targetDirectionYaw = Vector3.RotateTowards(Vector3.up, targetDirectionYaw, 45 * Mathf.Deg2Rad, 0);


                postYawFactor = 1;
                postPitchFactor = 1;
            }
            //else//(steerMode == SteerModes.Aiming)
            /*{
                targetDirection = vesselTransform.InverseTransformDirection(targetPosition - vesselTransform.position).normalized;
                targetDirection = Vector3.RotateTowards(Vector3.up, targetDirection, 45 * Mathf.Deg2Rad, 0);
                targetDirectionYaw = targetDirection;
            }*/

            pitchError = VectorUtils.SignedAngle(Vector3.up, Vector3.ProjectOnPlane(targetDirection, Vector3.right), Vector3.back);
            yawError = VectorUtils.SignedAngle(Vector3.up, Vector3.ProjectOnPlane(targetDirectionYaw, Vector3.forward), Vector3.right);


            float steerPitch = (postPitchFactor * 0.015f * steerMult * pitchError) + (postPitchFactor * 0.0015f * steerInt * pitchIntegrator) - (postPitchFactor * steerDamping * -localAngVel.x);
            float steerYaw = (postYawFactor * 0.022f * steerMult * yawError) + (postYawFactor * 0.0022f * steerInt * yawIntegrator) - (postYawFactor * steerDamping * -localAngVel.z);

            pitchIntegrator += pitchError;
            yawIntegrator += yawError;

            if (GetRadarAltitude() < 15)
            {
                pitchIntegrator = 0;
                yawIntegrator = 0;
            }

            s.yaw = Mathf.Clamp(steerYaw, -1, 1);
            s.pitch = Mathf.Clamp(steerPitch, -1, 1);


            //roll
            Vector3 currentRoll = -vesselTransform.forward;
            Vector3 rollTarget;

            //if(steerMode == SteerModes.Aiming || angleToTarget > 2)
            //{
            rollTarget = (targetPosition + 30f * upDirection) - vesselTransform.position;
            //}
            //else
            //{
            //	rollTarget = upDirection;
            //}

            rollTarget = Vector3.ProjectOnPlane(rollTarget, vesselTransform.up);

            float rollError = VectorUtils.SignedAngle(currentRoll, rollTarget, vesselTransform.right);
            //debugString += "\nRoll offset: " + rollError;
            float steerRoll = (steerMult * 0.0015f * rollError);
            //debugString += "\nSteerRoll: " + steerRoll;
            float rollDamping = (.10f * steerDamping * -localAngVel.y);
            steerRoll -= rollDamping;
            //debugString += "\nRollDamping: " + rollDamping;


            float roll = Mathf.Clamp(steerRoll, -1, 1);
            s.roll = roll;
            //
        }

        float GetRadarAltitude()
        {
            float radarAlt = Mathf.Clamp((float)(vessel.mainBody.GetAltitude(vessel.findWorldCenterOfMass()) - vessel.terrainAltitude), 0, (float)vessel.altitude);
            return radarAlt;
        }

        void OnDestroy()
        {
            if (vobj)
                GameObject.Destroy(vobj);

            vessel.OnAutopilotUpdate -= MouseAimPilot;
        }
    }
}
