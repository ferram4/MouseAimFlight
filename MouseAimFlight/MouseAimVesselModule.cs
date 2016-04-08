using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MouseAimFlight
{
    public class MouseAimVesselModule : VesselModule
    {
        Vessel vessel;
        Transform vesselTransform;

        float upWeighting = 0; //Upweighting not working, updating it on the GUI doesn't work either.

        string upWeightingStr;

        AdaptivePID pilot;

        static Vessel prevActiveVessel = null;
        bool mouseAimActive = false;
        static bool freeLook = false;
        static bool prevFreeLook = false;
        static bool forceCursorResetNextFrame = false;
        static bool pitchYawOverrideMouseAim = false;
        static FieldInfo freeLookKSPCameraField = null;
        
        Vector3 upDirection;
        Vector3 targetPosition;
        Vector3 mouseAimScreenLocation;
        Vector3 vesselForwardScreenLocation;

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

            upWeightingStr = upWeighting.ToString();

            pilot = new AdaptivePID();

            vesselTransform = vessel.ReferenceTransform;
            targetPosition = vesselTransform.up * 5000;     //if it's activated, set it to the baseline

            FieldInfo[] cameraMouseLookStaticFields = typeof(CameraMouseLook).GetFields(BindingFlags.NonPublic | BindingFlags.Static);
            freeLookKSPCameraField = cameraMouseLookStaticFields[0];
            
        }

        //Commented out old GUI
        void OnGUI()
        {
            if (vessel == FlightGlobals.ActiveVessel && mouseAimActive && !MapView.MapIsEnabled)
            {
                MouseAimFlightSceneGUI.DisplayMouseAimReticles(mouseAimScreenLocation, vesselForwardScreenLocation);
            }
        }

        void Update()
        {
            if (PauseMenu.isOpen)
            {
                mouseAimActive = false;
                //forceCursorResetNextFrame = true;
                return;
            } 
            
            if (vessel == FlightGlobals.ActiveVessel && vessel != prevActiveVessel)
            {
                prevActiveVessel = vessel;
                if (mouseAimActive)
                {
                    Screen.lockCursor = true;
                    Screen.showCursor = false;
                }
                else
                {
                    Screen.lockCursor = false;
                    Screen.showCursor = true;
                }
            }
            else if (Input.GetKeyDown(MouseAimSettings.ToggleKeyCode))
            {
                mouseAimActive = !mouseAimActive;
                if (mouseAimActive)
                {
                    Screen.lockCursor = true;
                    Screen.showCursor = false;
                }
                else
                {
                    Screen.lockCursor = false;
                    Screen.showCursor = true;
                }
                targetPosition = vesselTransform.up * 5000f;     //if it's activated, set it to the baseline
                UpdateCursorScreenLocation();
            }

            if (vessel != FlightGlobals.ActiveVessel || !mouseAimActive)
                return;

            UpdateMouseCursorForCameraRotation();
            UpdateVesselScreenLocation();
            UpdateCursorScreenLocation();
        }

        void LateUpdate()
        {
            if (vessel == FlightGlobals.ActiveVessel)
                CheckResetCursor();
        }

        void MouseAimPilot(FlightCtrlState s)
        {
            if (vessel != FlightGlobals.ActiveVessel || !mouseAimActive || PauseMenu.isOpen)
                return;

            vesselTransform = vessel.ReferenceTransform;

            if (s.pitch != s.pitchTrim || s.yaw != s.yawTrim)
            {
                pitchYawOverrideMouseAim = true;
                return;
            }
            else
                pitchYawOverrideMouseAim = false;

            upDirection = VectorUtils.GetUpDirection(vesselTransform.position);

            FlyToPosition(s, targetPosition + vessel.CoM);
        }

        void UpdateMouseCursorForCameraRotation()
        {
            if (pitchYawOverrideMouseAim)
            {
                targetPosition = vesselTransform.up * 5000f;
            }
            else
            {
                Vector3 mouseDelta;

                if (freeLook)
                    mouseDelta = Vector3.zero;
                else
                    mouseDelta = new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * MouseAimSettings.MouseSensitivity;

                if (MouseAimSettings.InvertXAxis)
                    mouseDelta.x *= -1;
                if (MouseAimSettings.InvertYAxis)
                    mouseDelta.y *= -1;

                Transform cameraTransform = FlightCamera.fetch.mainCamera.transform;

                Vector3 localTarget = cameraTransform.InverseTransformDirection(targetPosition);
                localTarget += mouseDelta;
                localTarget.Normalize();
                localTarget *= 5000f;

                targetPosition = cameraTransform.TransformDirection(localTarget);
            }
        }

        void UpdateCursorScreenLocation()
        {
            mouseAimScreenLocation = FlightCamera.fetch.mainCamera.WorldToScreenPoint(targetPosition + vessel.CoM);
        }

        void UpdateVesselScreenLocation()
        {
            vesselForwardScreenLocation = vesselTransform.up * 5000f;
            vesselForwardScreenLocation = FlightCamera.fetch.mainCamera.WorldToScreenPoint(vesselForwardScreenLocation + vessel.CoM);
        }

        void CheckResetCursor()
        {
            if (MapView.MapIsEnabled || PauseMenu.isOpen)
                return;

            prevFreeLook = freeLook;

            if (Mouse.Right.GetButton())
                freeLook = true;
            else if (freeLook)
                freeLook = false;


            freeLook |= (bool)freeLookKSPCameraField.GetValue(null);

            if ((freeLook != prevFreeLook || forceCursorResetNextFrame) && mouseAimActive)
            {
                Screen.lockCursor = true;
                Screen.showCursor = false;

                forceCursorResetNextFrame = false;
            }
        }

        void FlyToPosition(FlightCtrlState s, Vector3 targetPosition)
        {
            Vector3d srfVel = vessel.srf_velocity;
            if (srfVel != Vector3d.zero)
            {
                velocityTransform.rotation = Quaternion.LookRotation(srfVel, -vesselTransform.forward);
            }
            velocityTransform.rotation = Quaternion.AngleAxis(90, velocityTransform.right) * velocityTransform.rotation;
            Vector3 localAngVel = vessel.angularVelocity * Mathf.Rad2Deg;

            Vector3d targetDirection;
            Vector3d targetDirectionYaw;
            float yawError;
            float pitchError;
            float rollError;

            float terrainAltitude;
            float dynPressure;
            float velocity;

            //Setup
            targetDirection = vesselTransform.InverseTransformDirection(targetPosition - velocityTransform.position).normalized;
            targetDirectionYaw = targetDirection;

            terrainAltitude = GetRadarAltitude();
            dynPressure = (float)vessel.dynamicPressurekPa;
            velocity = (float)vessel.srfSpeed;

            pitchError = (float)Math.Asin(Vector3d.Dot(Vector3d.back, VectorUtils.Vector3dProjectOnPlane(targetDirection, Vector3d.right))) * Mathf.Rad2Deg;
            yawError = (float)Math.Asin(Vector3d.Dot(Vector3d.right, VectorUtils.Vector3dProjectOnPlane(targetDirectionYaw, Vector3d.forward))) * Mathf.Rad2Deg;

            //roll
            Vector3 currentRoll = -vesselTransform.forward;
            Vector3 rollTarget;

            upWeighting = pilot.UpWeighting(terrainAltitude, dynPressure, velocity);

            rollTarget = (targetPosition + Mathf.Clamp(upWeighting * (100f - Math.Abs(yawError * 1.6f) - (pitchError * 2.8f)), 0, float.PositiveInfinity) * upDirection) - vessel.CoM;

            rollTarget = Vector3.ProjectOnPlane(rollTarget, vesselTransform.up);

            rollError = VectorUtils.SignedAngle(currentRoll, rollTarget, vesselTransform.right);

            Steer steer = pilot.Simulate(pitchError, rollError, yawError, localAngVel, terrainAltitude, TimeWarp.fixedDeltaTime, dynPressure, velocity);

            s.pitch = Mathf.Clamp(steer.pitch, -1, 1);
            if (s.roll == s.rollTrim)
                s.roll = Mathf.Clamp(steer.roll, -1, 1);
            s.yaw = Mathf.Clamp(steer.yaw, -1, 1);
        }

        float GetRadarAltitude()
        {
            float radarAlt = Mathf.Clamp((float)(vessel.mainBody.GetAltitude(vessel.findWorldCenterOfMass()) - vessel.terrainAltitude), 0, (float)vessel.altitude);
            return radarAlt;
        }

        void OnDestroy()
        {
            if(vessel)
                vessel.OnAutopilotUpdate -= MouseAimPilot;
        }


    }
}
