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
        float steerInt = 0.5f;
        float steerDamping = 10;

        float pitchIntegrator;

        Vector3 upDirection;
        Vector3 targetPosition;
        Vector3 mouseAimScreenLocation;
        Vector3 vesselForwardScreenLocation;

        Vector3 prevCameraVector;

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

        static Texture2D vesselForwardReticle;
        static Texture2D mouseCursorReticle;

        void Start()
        {
            vessel = GetComponent<Vessel>();
            vessel.OnAutopilotUpdate += MouseAimPilot;

            if (mouseCursorReticle == null)
                mouseCursorReticle = GameDatabase.Instance.GetTexture("BDArmory/Textures/greenCircle3", false);
            if(vesselForwardReticle == null)
                vesselForwardReticle = GameDatabase.Instance.GetTexture("BDArmory/Textures/greenPointCircle", false);

        }

        void OnGUI()
        {
            if (vessel == FlightGlobals.ActiveVessel)
            {
                float size = Screen.width / 16;
                Rect aimRect = new Rect(mouseAimScreenLocation.x - (0.5f * size), (Screen.height - mouseAimScreenLocation.y) - (0.5f * size), size, size);

                GUI.DrawTexture(aimRect, mouseCursorReticle);

                size *= 0.5f;
                Rect directionRect = new Rect(vesselForwardScreenLocation.x - (0.5f * size), (Screen.height - vesselForwardScreenLocation.y) - (0.5f * size), size, size);

                GUI.DrawTexture(directionRect, vesselForwardReticle);
            }
        }

        void Update()
        {
            UpdateMouseCursorForCameraRotation();
            targetPosition = GetMouseCursorPosition();
            UpdateVesselForwardLocation();
        }

        void MouseAimPilot(FlightCtrlState s)
        {
            vesselTransform = vessel.ReferenceTransform;

            upDirection = VectorUtils.GetUpDirection(vesselTransform.position);

            FlyToPosition(s, targetPosition);
        }

        void UpdateMouseCursorForCameraRotation()
        {
            Vector3 mouseDelta = new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 25;

            Vector3 adjustedScreenCoords = FlightCamera.fetch.mainCamera.WorldToScreenPoint(prevCameraVector + FlightCamera.fetch.mainCamera.transform.position);

            mouseAimScreenLocation = adjustedScreenCoords + mouseDelta;
            mouseAimScreenLocation.x = Mathf.Clamp(mouseAimScreenLocation.x, 0, Screen.width);
            mouseAimScreenLocation.y = Mathf.Clamp(mouseAimScreenLocation.y, 0, Screen.height);
        }

        void UpdateVesselForwardLocation()
        {
            vesselForwardScreenLocation = vesselTransform.up * 5000;
            vesselForwardScreenLocation = FlightCamera.fetch.mainCamera.WorldToScreenPoint(vesselForwardScreenLocation + FlightCamera.fetch.mainCamera.transform.position);
        }

        Vector3 GetMouseCursorPosition()
        {
            Vector3 mouseAim = new Vector3(mouseAimScreenLocation.x / Screen.width, mouseAimScreenLocation.y / Screen.height, 5000);
            Vector3 target;
 /*           Ray ray = FlightCamera.fetch.mainCamera.ViewportPointToRay(mouseAim);

            prevCameraVector = ray.direction;
            target = (prevCameraVector * 5000) + FlightCamera.fetch.mainCamera.transform.position;*/

            target = FlightCamera.fetch.mainCamera.ViewportToWorldPoint(mouseAim);


            prevCameraVector = target - FlightCamera.fetch.mainCamera.transform.position;
            return target;
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

            Vector3 targetDirection;
            Vector3 targetDirectionYaw;
            float yawError;
            float pitchError;
            float postYawFactor;
            float postPitchFactor;
            //if (steerMode == SteerModes.NormalFlight)
            
                targetDirection = vesselTransform.InverseTransformDirection(targetPosition - velocityTransform.position).normalized;
                targetDirection = Vector3.RotateTowards(Vector3.up, targetDirection, 45 * Mathf.Deg2Rad, 0);

                //targetDirectionYaw = vesselTransform.InverseTransformDirection(srfVel).normalized;
                //targetDirectionYaw = Vector3.RotateTowards(Vector3.up, targetDirectionYaw, 45 * Mathf.Deg2Rad, 0);
                targetDirectionYaw = targetDirection;


                postYawFactor = 1;
                postPitchFactor = 1;
            
            //else//(steerMode == SteerModes.Aiming)
            /*{
                targetDirection = vesselTransform.InverseTransformDirection(targetPosition - vesselTransform.position).normalized;
                targetDirection = Vector3.RotateTowards(Vector3.up, targetDirection, 45 * Mathf.Deg2Rad, 0);
                targetDirectionYaw = targetDirection;
            }*/

            pitchError = VectorUtils.SignedAngle(Vector3.up, Vector3.ProjectOnPlane(targetDirection, Vector3.right), Vector3.back);
            yawError = VectorUtils.SignedAngle(Vector3.up, Vector3.ProjectOnPlane(targetDirectionYaw, Vector3.forward), Vector3.right);


            float steerPitch = (postPitchFactor * 0.015f * steerMult * pitchError) + (postPitchFactor * 0.015f * steerInt * pitchIntegrator) - (postPitchFactor * steerDamping * -localAngVel.x);
            float steerYaw = (postYawFactor * 0.008f * steerMult * yawError) - (postYawFactor * steerDamping * -localAngVel.z);

            pitchIntegrator += pitchError;

            if (GetRadarAltitude() < 15 || Math.Abs(pitchError) > 20)
                pitchIntegrator = 0;


            s.yaw = Mathf.Clamp(steerYaw, -1, 1);
            s.pitch = Mathf.Clamp(steerPitch, -1, 1);


            //roll
            Vector3 currentRoll = -vesselTransform.forward;
            Vector3 rollTarget;

            if (GetRadarAltitude() > 10)
                rollTarget = (targetPosition + 750f * upDirection) - vesselTransform.position;
            else
                rollTarget = upDirection;

            rollTarget = Vector3.ProjectOnPlane(rollTarget, vesselTransform.up);

            float rollError = VectorUtils.SignedAngle(currentRoll, rollTarget, vesselTransform.right);
            //debugString += "\nRoll offset: " + rollError;
            float steerRoll = (steerMult * 0.015f * rollError);
            //debugString += "\nSteerRoll: " + steerRoll;
            float rollDamping = (.10f * steerDamping * -localAngVel.y);
            steerRoll -= rollDamping;
            //debugString += "\nRollDamping: " + rollDamping;


            float roll = Mathf.Clamp(steerRoll, -1, 1);
            if(s.roll == s.rollTrim)
                s.roll = roll;
            
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
