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
using UnityEngine;

namespace MouseAimFlight
{
    class MouseAimSettings
    {
        static MouseAimSettings instance;
        public static MouseAimSettings Instance
        {
            get
            {
                if (instance == null)
                    instance = new MouseAimSettings();

                return instance;
            }
        }

        static KeyCode toggleKeyCode = KeyCode.P;
        public static KeyCode ToggleKeyCode
        {
            get { return toggleKeyCode; }
        }
        static string toggleKeyString = "P";
        public static string ToggleKeyString
        {
            get { return toggleKeyString; }
            set
            {
                string tmp = value.ToUpperInvariant();
                if(tmp != toggleKeyString && tmp.Length == 1)
                {
                    toggleKeyString = tmp;
                    toggleKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), toggleKeyString);
                    Instance.SaveSettings();
                }
            }
        }

        public enum CursorStyle
        {
            FULL,
            DOT,
            NONE
        }
        static CursorStyle cursor = CursorStyle.FULL;
        public static CursorStyle Cursor
        {
            get { return cursor; }
            set
            {
                if (value != cursor)
                {
                    cursor = value;
                    Instance.SaveSettings();
                }
            }
        }

        static float mouseSensitivity = 100;
        public static float MouseSensitivity
        {
            get { return mouseSensitivity; }
            set
            {
                if (value != mouseSensitivity)
                {
                    mouseSensitivity = value;
                    Instance.SaveSettings();
                }
            }
        }

        static bool invertY = false;
        static bool invertX = false;
        public static bool InvertYAxis
        {
            get { return invertY; }
            set
            {
                if (value != invertY)
                {
                    invertY = value;
                    Instance.SaveSettings();
                }
            }
        }

        public static bool InvertXAxis
        {
            get { return invertX; }
            set
            {
                if (value != invertX)
                {
                    invertX = value;
                    Instance.SaveSettings();
                }
            }
        }

        MouseAimSettings()
        {
            LoadSettings();
        }

        void LoadSettings()
        {
            ConfigNode node = GameDatabase.Instance.GetConfigNode("MAFSettings");
            if((object)node != null)
            {
                if (node.HasValue("toggleKey"))
                {
                    toggleKeyString = ((string)node.GetValue("toggleKey")).ToUpperInvariant();
                    toggleKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), toggleKeyString);
                }
                if(node.HasValue("cursorStyle"))
                {
                    Enum.TryParse<CursorStyle>((string)node.GetValue("cursorStyle"), out cursor);
                }
                if (node.HasValue("mouseSensitivity"))
                {
                    float.TryParse(node.GetValue("mouseSensitivity"), out mouseSensitivity);
                }
                if(node.HasValue("invertX"))
                {
                    bool.TryParse(node.GetValue("invertX"), out invertX);
                }
                if (node.HasValue("invertY"))
                {
                    bool.TryParse(node.GetValue("invertY"), out invertX);
                }
            }
        }

        public void SaveSettings()
        {
            ConfigNode node = new ConfigNode("MAFSettings");
            node.AddValue("name", "default");
            node.AddValue("toggleKey", toggleKeyCode.ToString());
            node.AddValue("cursorStyle", cursor.ToString());
            node.AddValue("mouseSensitivity", mouseSensitivity.ToString());
            node.AddValue("invertX", invertX.ToString());
            node.AddValue("invertY", invertY.ToString());

            ConfigNode saveNode = new ConfigNode();
            saveNode.AddNode(node);
            saveNode.Save(KSPUtil.ApplicationRootPath.Replace("\\", "/") + "GameData/MouseAimFlight/MAFSettings.cfg");
        }
    }
}
