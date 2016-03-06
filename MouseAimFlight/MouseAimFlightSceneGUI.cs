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
using UnityEngine;

namespace MouseAimFlight
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class MouseAimFlightSceneGUI : MonoBehaviour
    {
        static MouseAimFlightSceneGUI instance;
        public static MouseAimFlightSceneGUI Instance { get { return instance; } }

        static ApplicationLauncherButton mAFButton = null;
        static bool showGUI = false;
        static Rect guiRect;

        static Texture2D vesselForwardReticle;
        static Texture2D mouseCursorReticle;

        static Texture2D vesselForwardCross;
        static Texture2D vesselForwardDot;
        static Texture2D vesselForwardBlank;

        void Start()
        {
            instance = this;
            if (mouseCursorReticle == null)
            {
                mouseCursorReticle = GameDatabase.Instance.GetTexture("MouseAimFlight/Assets/circle", false);
                mouseCursorReticle.filterMode = FilterMode.Trilinear;
            }
            if (vesselForwardCross == null)
            {
                vesselForwardCross = GameDatabase.Instance.GetTexture("MouseAimFlight/Assets/cross", false);
                vesselForwardCross.filterMode = FilterMode.Trilinear;
            }
            if (vesselForwardDot == null)
            {
                vesselForwardDot = GameDatabase.Instance.GetTexture("MouseAimFlight/Assets/dot", false);
                vesselForwardDot.filterMode = FilterMode.Trilinear;
            }
            if (vesselForwardBlank == null)
            {
                vesselForwardBlank = GameDatabase.Instance.GetTexture("MouseAimFlight/Assets/blank", false);
                vesselForwardBlank.filterMode = FilterMode.Trilinear;
            }
            UpdateCursor(MouseAimSettings.Cursor);
            OnGUIAppLauncherReady();
        }

        public static void DisplayMouseAimReticles(Vector3 mouseAimScreenLocation, Vector3 vesselForwardScreenLocation)
        {
            float size = Screen.width / 32;
            if (mouseAimScreenLocation.z > 0)
            {
                Rect aimRect = new Rect(mouseAimScreenLocation.x - (0.5f * size), (Screen.height - mouseAimScreenLocation.y) - (0.5f * size), size, size);

                GUI.DrawTexture(aimRect, mouseCursorReticle);
            }

            if (vesselForwardScreenLocation.z > 0)
            {
                Rect directionRect = new Rect(vesselForwardScreenLocation.x - (0.5f * size), (Screen.height - vesselForwardScreenLocation.y) - (0.5f * size), size, size);

                GUI.DrawTexture(directionRect, vesselForwardReticle);
            }
        }

        void OnGUI()
        {
            if (showGUI)
            {
                GUI.skin = HighLogic.Skin;

                guiRect = GUILayout.Window(this.GetHashCode(), guiRect, GUIWindow, "MouseAim Settings");
            }
        }

        void GUIWindow(int windowID)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal(GUILayout.Width(170));
            GUILayout.Label("Toggle MouseAim Key: ");
            MouseAimSettings.ToggleKeyString = GUILayout.TextField(MouseAimSettings.ToggleKeyString, 1, GUILayout.Width(25));
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal(GUILayout.Width(170));
            if(GUILayout.Button("Change Cursor: ", GUILayout.Width(100)))
                CycleCursor();
            GUI.DrawTexture(new Rect(120, 65, 35, 35), vesselForwardReticle);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.Label("Mouse Sensitivity: " + MouseAimSettings.MouseSensitivity);
            MouseAimSettings.MouseSensitivity = GUILayout.HorizontalSlider(MouseAimSettings.MouseSensitivity, 25, 500);
            MouseAimSettings.InvertXAxis = GUILayout.Toggle(MouseAimSettings.InvertXAxis, "Invert X Axis");
            MouseAimSettings.InvertYAxis = GUILayout.Toggle(MouseAimSettings.InvertYAxis, "Invert Y Axis");
            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        void CycleCursor()
        {
            if (MouseAimSettings.Cursor == MouseAimSettings.CursorStyle.FULL)
                MouseAimSettings.Cursor = MouseAimSettings.CursorStyle.DOT;
            else if (MouseAimSettings.Cursor == MouseAimSettings.CursorStyle.DOT)
                MouseAimSettings.Cursor = MouseAimSettings.CursorStyle.NONE;
            else
                MouseAimSettings.Cursor = MouseAimSettings.CursorStyle.FULL;

            UpdateCursor(MouseAimSettings.Cursor);
        }
        
        void UpdateCursor(MouseAimSettings.CursorStyle cursor)
        {
            if(cursor == MouseAimSettings.CursorStyle.FULL)
                vesselForwardReticle = vesselForwardCross;
            else if (cursor == MouseAimSettings.CursorStyle.DOT)
                vesselForwardReticle = vesselForwardDot;
            else
                vesselForwardReticle = vesselForwardBlank;
        }

       #region AppLauncher
        public void OnGUIAppLauncherReady()
        {
            if (ApplicationLauncher.Ready && mAFButton == null)
            {
                mAFButton = ApplicationLauncher.Instance.AddModApplication(
                    onAppLaunchToggleOn,
                    onAppLaunchToggleOff,
                    null,
                    null,
                    null,
                    null,
                    ApplicationLauncher.AppScenes.FLIGHT,
                    (Texture)GameDatabase.Instance.GetTexture("MouseAimFlight/Textures/icon_button_stock", false));
            }
        }

        void onAppLaunchToggleOn()
        {
            showGUI = true;
        }

        void onAppLaunchToggleOff()
        {
            showGUI = false;
        }
        #endregion
    }
}
