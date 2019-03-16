using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RUA
{
    /// <summary>
    /// Recent Asset Class. Used to store references to the GUIDs 
    /// between sessions with the help of GUIDStore
    /// </summary>
    public class RecentAsset
    {
        public RecentAsset(string guid)
        {
            _GUID = guid;
            _selected = false;
        }

        public string _GUID { get; set; }
        //public bool _favourited { get; private set; } = false;

        public bool _selected { get; set; }

        public string Name
        {
            get
            {
                string path = AssetDatabase.GUIDToAssetPath(_GUID);
                string name = Path.GetFileName(path);
                return (name);
            }
        }
    }
}
