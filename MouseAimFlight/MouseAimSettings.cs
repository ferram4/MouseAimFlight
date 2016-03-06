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
