/*
 * Copyright (c) 2016 BahamutoD, ferram4, tetryds
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to
 * deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 * THE SOFTWARE.
*/

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

        //GET RID OF THIS AS SOON AS THE GUI IS REMOVED
        float pitchP = 0.2f, pitchI = 0.1f, pitchD = 0.08f;
        float rollP = 0.01f, rollI = 0.001f, rollD = 0.005f;
        float yawP = 0.035f, yawI = 0.1f, yawD = 0.04f;
        //------------------

        //DEBUG
        float dynPressDebug;
        float speedFactorDebug;
        float invSpeedFactorDebug;
        //------------------

        float upWeighting = 0; //Upweighting not working, updating it on the GUI doesn't work either.

        string pitchPstr, pitchIstr, pitchDstr;
        string rollPstr, rollIstr, rollDstr;
        string yawPstr, yawIstr, yawDstr;
        string upWeightingStr;

        AdaptivePID pilot;

        static Vessel prevActiveVessel = null;
        bool mouseAimActive = false;
        static bool freeLook = false;
        static bool prevFreeLook = false;
        static bool forceCursorResetNextFrame = false;
        static bool pitchYawOverrideMouseAim = false;
        static FieldInfo freeLookKSPCameraField = null;
        string debugLabel;
        
        Vector3 upDirection;
        Vector3 targetPosition;
        Vector3 mouseAimScreenLocation;
        Vector3 vesselForwardScreenLocation;

        Rect debugRect;

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

            pitchDstr = pitchD.ToString();
            pitchIstr = pitchI.ToString();
            pitchPstr = pitchP.ToString();

            rollDstr = rollD.ToString();
            rollIstr = rollI.ToString();
            rollPstr = rollP.ToString();

            yawDstr = yawD.ToString();
            yawIstr = yawI.ToString();
            yawPstr = yawP.ToString();

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
            /*    GUI.contentColor = Color.black;
                GUI.Label(new Rect(200, 200, 1200, 800), debugLabel);

            }
            else if (vessel == FlightGlobals.ActiveVessel)
                debugRect = GUILayout.Window(this.GetHashCode(), debugRect, DebugPIDGUI, "");*/
        }

        void DebugPIDGUI(int windowID)
        {
            GUILayout.Label("Pitch:");

            GUILayout.BeginHorizontal();
            TextEntry(ref pitchPstr, "P:");
            GUILayout.Label(pilot.pitchPID.kp.ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            TextEntry(ref pitchIstr, "I:");
            GUILayout.Label(pilot.pitchPID.ki.ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            TextEntry(ref pitchDstr, "D:");
            GUILayout.Label(pilot.pitchPID.kd.ToString());
            GUILayout.EndHorizontal();

            GUILayout.Label("Roll:");

            GUILayout.BeginHorizontal();
            TextEntry(ref rollPstr, "P:");
            GUILayout.Label(pilot.rollPID.kp.ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            TextEntry(ref rollIstr, "I:");
            GUILayout.Label(pilot.rollPID.ki.ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            TextEntry(ref rollDstr, "D:");
            GUILayout.Label(pilot.rollPID.kd.ToString());
            GUILayout.EndHorizontal();

            GUILayout.Label("Yaw:");

            GUILayout.BeginHorizontal();
            TextEntry(ref yawPstr, "P:");
            GUILayout.Label(pilot.yawPID.kp.ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            TextEntry(ref yawIstr, "I:");
            GUILayout.Label(pilot.yawPID.ki.ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            TextEntry(ref yawDstr, "D:");
            GUILayout.Label(pilot.yawPID.kd.ToString());
            GUILayout.EndHorizontal();
            
            TextEntry(ref upWeightingStr, "Roll-up Weight");

            if (GUILayout.Button("Update K and reset integration errors"))
            {
                pitchD = float.Parse(pitchDstr);
                pitchI = float.Parse(pitchIstr);
                pitchP = float.Parse(pitchPstr);

                rollD = float.Parse(rollDstr);
                rollI = float.Parse(rollIstr);
                rollP = float.Parse(rollPstr);

                yawD = float.Parse(yawDstr);
                yawI = float.Parse(yawIstr);
                yawP = float.Parse(yawPstr);

                upWeighting = float.Parse(upWeightingStr);

                pilot = new AdaptivePID(pitchP, pitchI, pitchD, rollP, rollI, rollD, yawP, yawI, yawD);
            }

            if (GUILayout.Button("Reset integration errors"))
            {
                pilot.pitchPID.ZeroIntegral();
                pilot.rollPID.ZeroIntegral();
                pilot.yawPID.ZeroIntegral();
            }

            GUI.DragWindow();
        }

        void TextEntry(ref string field, string label)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            field = GUILayout.TextField(field);
            GUILayout.EndHorizontal();
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

            debugLabel = "";
            if (s.pitch != s.pitchTrim || s.yaw != s.yawTrim)
            {
                pitchYawOverrideMouseAim = true;
                return;
            }
            else
                pitchYawOverrideMouseAim = false;

            upDirection = VectorUtils.GetUpDirection(vesselTransform.position);

            FlyToPosition(s, targetPosition + vessel.CoM);
            pilot.pitchPID.DebugString(ref debugLabel, "pitch");
            debugLabel += "\n\n";
            pilot.rollPID.DebugString(ref debugLabel, "roll");
            debugLabel += "\n\n";
            pilot.yawPID.DebugString(ref debugLabel, "yaw");
            debugLabel += "\n\n";
            debugLabel += "Dynpress: " + dynPressDebug.ToString("N7");
            debugLabel += "\n\n";
            debugLabel += "Speed Factor: " + speedFactorDebug.ToString("N7");
            debugLabel += "\n\n";
            debugLabel += "Inverse Speed Factor: " + invSpeedFactorDebug.ToString("N7");
            debugLabel += "\n\n";
            debugLabel += "freelook: " + freeLook;
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

            //Debug
            dynPressDebug = dynPressure;
            speedFactorDebug = dynPressure * 16 / velocity;
            invSpeedFactorDebug = 1 / (speedFactorDebug + Single.Epsilon);
            //------------------
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
