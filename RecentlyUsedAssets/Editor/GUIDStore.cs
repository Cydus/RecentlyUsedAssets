using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RUA
{
    /// <summary>
    /// Stringifies Recent Assets GUIDs for GUI persistance between editor sessions.
    /// </summary>
    [InitializeOnLoad]
    public class GUIDStore
    {
        public static GUIDStore _instance;
        public static GUIDStore Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GUIDStore();
                return _instance;
            }
        }

        string key = PlayerSettings.companyName + "." + PlayerSettings.productName + "." + "RUA";
        char[] splitChars = new char[] { ',' };

        public string [] LoadGUIDs(string listSuffix)
        {
            string guids = EditorPrefs.GetString(key + listSuffix, null);
            return guids.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
        }

        public void SaveGUIDs(List<RecentAsset> guids, string listSuffix)
        {
            string value = "";
            for (int i = 0; i < guids.Count; i++)
            {
                value += guids[i]._GUID;
                if (i < guids.Count -1)
                    value += ",";
            }

            EditorPrefs.SetString(key + listSuffix, value);
        }
    }
}