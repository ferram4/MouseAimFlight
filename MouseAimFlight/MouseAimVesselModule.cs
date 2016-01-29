using System;
using System.Collections.Generic;
using UnityEngine;

namespace MouseAimFlight
{
    public class MouseAimVesselModule : VesselModule
    {
        Vessel vessel;
        Transform vesselTransform;

        float pitchP = 0.2f, pitchI = 0.01f, pitchD = 0.08f;
        float yawP = 0.035f, yawI = 0.004f, yawD = 0.04f;
        float rollP = 0.01f, rollI = 0.01f, rollD = 0.005f;
        float upWeighting = 3f;

        string pitchPstr, pitchIstr, pitchDstr;
        string yawPstr, yawIstr, yawDstr;
        string rollPstr, rollIstr, rollDstr;
        string upWeightingStr;

        AdaptivePID pitchPID;
        AdaptivePID yawPID;
        AdaptivePID rollPID;

        //float pitchIntegrator;
        //float yawIntegrator;
        //float rollIntegrator;

        bool mouseAimActive = false;
        bool prevFreeLook = false;
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

            pitchDstr = pitchD.ToString();
            pitchIstr = pitchI.ToString();
            pitchPstr = pitchP.ToString();

            yawDstr = yawD.ToString();
            yawIstr = yawI.ToString();
            yawPstr = yawP.ToString();

            rollDstr = rollD.ToString();
            rollIstr = rollI.ToString();
            rollPstr = rollP.ToString();

            upWeightingStr = upWeighting.ToString();

            pitchPID = new AdaptivePID(pitchP, pitchI, pitchD);
            yawPID = new AdaptivePID(yawP, yawI, yawD);
            rollPID = new AdaptivePID(rollP, rollI, rollD);

            vesselTransform = vessel.ReferenceTransform;
            targetPosition = vesselTransform.up * 5000;     //if it's activated, set it to the baseline
        }

        void OnGUI()
        {
            if (vessel == FlightGlobals.ActiveVessel && mouseAimActive)
            {
                float size = Screen.width / 16;
                Rect aimRect = new Rect(mouseAimScreenLocation.x - (0.5f * size), (Screen.height - mouseAimScreenLocation.y) - (0.5f * size), size, size);

                GUI.DrawTexture(aimRect, mouseCursorReticle);

                size *= 0.5f;
                Rect directionRect = new Rect(vesselForwardScreenLocation.x - (0.5f * size), (Screen.height - vesselForwardScreenLocation.y) - (0.5f * size), size, size);

                GUI.DrawTexture(directionRect, vesselForwardReticle);

                GUI.Label(new Rect(200, 200, 1200, 800), debugLabel);
                
            }
            else if(vessel == FlightGlobals.ActiveVessel)
                debugRect = GUILayout.Window(this.GetHashCode(), debugRect, DebugPIDGUI, "");
        }

        void DebugPIDGUI(int windowID)
        {
            GUILayout.Label("Pitch:");

            GUILayout.BeginHorizontal();
            TextEntry(ref pitchPstr, "P:");
            GUILayout.Label(pitchPID.kp.ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            TextEntry(ref pitchIstr, "I:");
            GUILayout.Label(pitchPID.ki.ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            TextEntry(ref pitchDstr, "D:");
            GUILayout.Label(pitchPID.kd.ToString());
            GUILayout.EndHorizontal();

            GUILayout.Label("Yaw:");

            GUILayout.BeginHorizontal();
            TextEntry(ref yawPstr, "P:");
            GUILayout.Label(yawPID.kp.ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            TextEntry(ref yawIstr, "I:");
            GUILayout.Label(yawPID.ki.ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            TextEntry(ref yawDstr, "D:");
            GUILayout.Label(yawPID.kd.ToString());
            GUILayout.EndHorizontal();

            GUILayout.Label("Roll:");

            GUILayout.BeginHorizontal();
            TextEntry(ref rollPstr, "P:");
            GUILayout.Label(rollPID.kp.ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            TextEntry(ref rollIstr, "I:");
            GUILayout.Label(rollPID.ki.ToString());
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            TextEntry(ref rollDstr, "D:");
            GUILayout.Label(rollPID.kd.ToString());
            GUILayout.EndHorizontal();

            TextEntry(ref upWeightingStr, "Roll-up Weight");
            
            if (GUILayout.Button("Update K and reset integration errors"))
            {
                pitchD = float.Parse(pitchDstr);
                pitchI = float.Parse(pitchIstr);
                pitchP = float.Parse(pitchPstr);

                yawD = float.Parse(yawDstr);
                yawI = float.Parse(yawIstr);
                yawP = float.Parse(yawPstr);

                rollD = float.Parse(rollDstr);
                rollI = float.Parse(rollIstr);
                rollP = float.Parse(rollPstr);

                upWeighting = float.Parse(upWeightingStr);

                pitchPID = new AdaptivePID(pitchP, pitchI, pitchD);
                yawPID = new AdaptivePID(yawP, yawI, yawD);
                rollPID = new AdaptivePID(rollP, rollI, rollD);
            }
            if (GUILayout.Button("Reset integration errors"))
            {
                pitchPID.ZeroIntegral();
                yawPID.ZeroIntegral();
                rollPID.ZeroIntegral();
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
            if (Input.GetKeyDown(KeyCode.P))
            {
                mouseAimActive = !mouseAimActive;
                if(mouseAimActive)
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
            CheckResetCursor();
        }

        void MouseAimPilot(FlightCtrlState s)
        {
            if (vessel != FlightGlobals.ActiveVessel || !mouseAimActive)
                return;

            vesselTransform = vessel.ReferenceTransform;
            //if(!freeLook)
            //    UpdateMouseCursorForCameraRotation();
            debugLabel = "";
            if (s.roll != s.rollTrim || s.pitch != s.pitchTrim || s.yaw != s.yawTrim)
                return;

            upDirection = VectorUtils.GetUpDirection(vesselTransform.position);

            FlyToPosition(s, targetPosition + vessel.CoM);
            pitchPID.DebugString(ref debugLabel, "pitch");
            debugLabel += "\n\n";
            yawPID.DebugString(ref debugLabel, "yaw");
            debugLabel += "\n\n";
            rollPID.DebugString(ref debugLabel, "roll");
        }

        void UpdateMouseCursorForCameraRotation()
        {
            Vector3 mouseDelta;

            if (Mouse.Right.GetButton())
                mouseDelta = Vector3.zero;
            else
                mouseDelta = new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 100;

            /*if ((mouseAimScreenLocation.x < 0 && mouseDelta.x < 0) || (mouseAimScreenLocation.x > Screen.width && mouseDelta.x > 0))
                mouseDelta.x = 0;
            if ((mouseAimScreenLocation.y < 0 && mouseDelta.y < 0) || (mouseAimScreenLocation.y > Screen.height && mouseDelta.y > 0))
                mouseDelta.y = 0;*/

            Transform cameraTransform = FlightCamera.fetch.mainCamera.transform;

            Vector3 localTarget = cameraTransform.InverseTransformDirection(targetPosition);
            localTarget += mouseDelta;
            localTarget.Normalize();
            localTarget *= 5000f;

            targetPosition = cameraTransform.TransformDirection(localTarget);
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
            if(!Mouse.Right.GetButton() && prevFreeLook)
            {
                prevFreeLook = false;
                /*if ((mouseAimScreenLocation.z < 0 || mouseAimScreenLocation.x < 0 || mouseAimScreenLocation.x > Screen.width || mouseAimScreenLocation.y < 0 || mouseAimScreenLocation.y > Screen.height))
                {
                    targetPosition = FlightCamera.fetch.mainCamera.transform.forward * 5000f;
                    mouseAimScreenLocation = FlightCamera.fetch.mainCamera.WorldToScreenPoint(targetPosition + vessel.CoM);
                }*/
            }
            if (Mouse.Right.GetButton())
                prevFreeLook = true;
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

            Vector3 targetDirection;
            Vector3 targetDirectionYaw;
            float yawError;
            float pitchError;
            //if (steerMode == SteerModes.NormalFlight)
            
                targetDirection = vesselTransform.InverseTransformDirection(targetPosition - velocityTransform.position).normalized;
                targetDirection = Vector3.RotateTowards(Vector3.up, targetDirection, 45 * Mathf.Deg2Rad, 0);

                //targetDirectionYaw = vesselTransform.InverseTransformDirection(srfVel).normalized;
                //targetDirectionYaw = Vector3.RotateTowards(Vector3.up, targetDirectionYaw, 45 * Mathf.Deg2Rad, 0);
                targetDirectionYaw = targetDirection;

            
            //else//(steerMode == SteerModes.Aiming)
            /*{
                targetDirection = vesselTransform.InverseTransformDirection(targetPosition - vesselTransform.position).normalized;
                targetDirection = Vector3.RotateTowards(Vector3.up, targetDirection, 45 * Mathf.Deg2Rad, 0);
                targetDirectionYaw = targetDirection;
            }*/

            pitchError = VectorUtils.SignedAngle(Vector3.up, Vector3.ProjectOnPlane(targetDirection, Vector3.right), Vector3.back);
            yawError = VectorUtils.SignedAngle(Vector3.up, Vector3.ProjectOnPlane(targetDirectionYaw, Vector3.forward), Vector3.right);

            bool nearGround = GetRadarAltitude() < 10;

            float steerPitch = pitchPID.Simulate(pitchError, localAngVel.x, TimeWarp.fixedDeltaTime, !nearGround);
            float steerYaw = yawPID.Simulate(yawError, localAngVel.z, TimeWarp.fixedDeltaTime, !nearGround);

            if (Math.Abs(pitchError) > 20)
                pitchPID.ZeroIntegral();
            if (Math.Abs(yawError) > 20)
                yawPID.ZeroIntegral();



            s.yaw = Mathf.Clamp(steerYaw, -1, 1);
            s.pitch = Mathf.Clamp(steerPitch, -1, 1);


            //roll
            Vector3 currentRoll = -vesselTransform.forward;
            Vector3 rollTarget;

            if (!nearGround)
                rollTarget = (targetPosition + upWeighting * (75f - yawError * 1f) * upDirection) - vessel.CoM;
            else
                rollTarget = upDirection;

            rollTarget = Vector3.ProjectOnPlane(rollTarget, vesselTransform.up);

            float rollError = VectorUtils.SignedAngle(currentRoll, rollTarget, vesselTransform.right);

            /*float rollFactor = Vector3.Dot(currentRoll, rollTarget);
            if(rollFactor < 0 && rollFactor > -200 && Vector3.Dot(rollTarget.normalized, Vector3.ProjectOnPlane(upDirection, vesselTransform.up).normalized) < 0.95f)
            {
                if (rollError < -120)
                    rollError += 180f;
                else if (rollError > 120)
                    rollError -= 180f;
            }*/

            if (Math.Abs(rollError) > 20)
                rollPID.ZeroIntegral();
            
            //debugString += "\nRoll offset: " + rollError;
            float steerRoll = rollPID.Simulate(rollError, localAngVel.y, TimeWarp.fixedDeltaTime, nearGround);
            //debugString += "\nSteerRoll: " + steerRoll;
            //float rollDamping = (rollD * -localAngVel.y);
            //steerRoll -= rollDamping;
            //debugString += "\nRollDamping: " + rollDamping;

            float roll = Mathf.Clamp(steerRoll, -1, 1);
            s.roll = roll;
        }

        float GetRadarAltitude()
        {
            float radarAlt = Mathf.Clamp((float)(vessel.mainBody.GetAltitude(vessel.findWorldCenterOfMass()) - vessel.terrainAltitude), 0, (float)vessel.altitude);
            return radarAlt;
        }

        void OnDestroy()
        {
            //if (vobj)
            //    GameObject.Destroy(vobj);
            if(vessel)
                vessel.OnAutopilotUpdate -= MouseAimPilot;
        }


    }
}
