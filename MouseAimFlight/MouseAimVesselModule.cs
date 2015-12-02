using System;
using System.Collections.Generic;
using UnityEngine;

namespace MouseAimFlight
{
    public class MouseAimVesselModule : VesselModule
    {
        Vessel vessel;
        Transform vesselTransform;

        float pitchP = 0.1f, pitchI = 0.001f, pitchD = 0;
        float yawP = 0.005f, yawI = 0.0005f, yawD = 0.017f;
        float rollP = 0.01f, rollI = 0.01f, rollD = 0.0f;

        string pitchPstr, pitchIstr, pitchDstr;
        string yawPstr, yawIstr, yawDstr;
        string rollPstr, rollIstr, rollDstr;

        float pitchIntegrator;
        float yawIntegrator;
        float rollIntegrator;

        Vector3 upDirection;
        Vector3 targetPosition;
        Vector3 mouseAimScreenLocation;
        Vector3 vesselForwardScreenLocation;

        Vector3 prevCameraVector;

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

                debugRect = GUILayout.Window(this.GetHashCode(), debugRect, DebugPIDGUI, "");
            }
        }

        void DebugPIDGUI(int windowID)
        {
            GUILayout.Label("Pitch:");

            TextEntry(ref pitchPstr, "P:");
            TextEntry(ref pitchIstr, "I:");
            TextEntry(ref pitchDstr, "D:");

            GUILayout.Label("Yaw:");

            TextEntry(ref yawPstr, "P:");
            TextEntry(ref yawIstr, "I:");
            TextEntry(ref yawDstr, "D:");

            GUILayout.Label("Roll:");

            TextEntry(ref rollPstr, "P:");
            TextEntry(ref rollIstr, "I:");
            TextEntry(ref rollDstr, "D:");

            if(GUILayout.Button("Update K and reset integration errors"))
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

                rollIntegrator = pitchIntegrator = yawIntegrator = 0;
            }
            if(GUILayout.Button("Reset integration errors"))
                rollIntegrator = pitchIntegrator = yawIntegrator = 0;

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


            float steerPitch = (pitchP * pitchError) + (pitchI * pitchIntegrator) - (pitchD * -localAngVel.x);
            float steerYaw = (yawP * yawError) + (yawI * yawIntegrator) - (yawD * -localAngVel.z);

            pitchIntegrator += pitchError;
            yawIntegrator += yawError;

            pitchIntegrator = Mathf.Clamp(pitchIntegrator, -1.5f / pitchI, 1.5f / pitchI);
            yawIntegrator = Mathf.Clamp(yawIntegrator, -1.5f / yawI, 1.5f / yawI);

            if (GetRadarAltitude() < 15)
            {
                pitchIntegrator = 0;
                yawIntegrator = 0;
                rollIntegrator = 0;
            }
            if (Math.Abs(pitchError) > 20)
                pitchIntegrator = 0;
            if (Math.Abs(yawError) > 20)
                yawIntegrator = 0;



            s.yaw = Mathf.Clamp(steerYaw, -1, 1);
            s.pitch = Mathf.Clamp(steerPitch, -1, 1);


            //roll
            Vector3 currentRoll = -vesselTransform.forward;
            Vector3 rollTarget;

            if (GetRadarAltitude() > 10)
                rollTarget = (targetPosition + (750f - yawError * 10f) * upDirection) - vesselTransform.position;
            else
                rollTarget = upDirection;

            rollTarget = Vector3.ProjectOnPlane(rollTarget, vesselTransform.up);

            float rollError = VectorUtils.SignedAngle(currentRoll, rollTarget, vesselTransform.right);
            if (Math.Abs(rollError) > 20)
                rollIntegrator = 0;
            
            //debugString += "\nRoll offset: " + rollError;
            float steerRoll = (rollP * rollError);
            steerRoll += (rollD * rollIntegrator);
            //debugString += "\nSteerRoll: " + steerRoll;
            float rollDamping = (rollD * -localAngVel.y);
            steerRoll -= rollDamping;
            //debugString += "\nRollDamping: " + rollDamping;

            rollIntegrator += rollError;
            rollIntegrator = Mathf.Clamp(rollIntegrator, -1.5f / rollI, 1.5f / rollI);

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
