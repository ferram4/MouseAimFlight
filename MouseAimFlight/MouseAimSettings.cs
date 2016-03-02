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
                    cursor = (CursorStyle)Enum.Parse(typeof(CursorStyle), (string)node.GetValue("cursorStyle"));
                }
            }
        }

        public void SaveSettings()
        {
            ConfigNode node = new ConfigNode("MAFSettings");
            node.AddValue("name", "default");
            node.AddValue("toggleKey", toggleKeyCode.ToString());
            node.AddValue("cursorStyle", cursor.ToString());

            ConfigNode saveNode = new ConfigNode();
            saveNode.AddNode(node);
            saveNode.Save(KSPUtil.ApplicationRootPath.Replace("\\", "/") + "GameData/MouseAimFlight/MAFSettings.cfg");
        }
    }
}
