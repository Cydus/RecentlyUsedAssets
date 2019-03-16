using System.Collections.Generic;
using UnityEditor;

namespace RUA
{
    class AssetImportDetector : AssetPostprocessor
    {
        // hold prior selected GUIDs as they are lost on asset import
        public static List<string> priorSelectedGUIDs = new List<string>();

        void OnPreprocessAsset()
        {
            // find those prior asset Ids
            foreach (string guid in Selection.assetGUIDs)
            {
                if (priorSelectedGUIDs.Contains(guid) == false)
                {
                    priorSelectedGUIDs.Add(guid);
                }
            }
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets)
            {
                //Debug.Log("Reimported Asset: " + str);
                string guid = AssetDatabase.AssetPathToGUID(str);
                RecentlyUsedAssetsQueue.Instance.Add(guid, true);

            }

            priorSelectedGUIDs.Clear();

            foreach (string str in deletedAssets)
            {
                //Debug.Log("Deleted Asset: " + str);
                string guid = AssetDatabase.AssetPathToGUID(str);
                RecentlyUsedAssetsQueue.Instance.RemoveAssetsWithGUID(guid, true);
            }

            //for (int i = 0; i < movedAssets.Length; i++)
            //{
            //    Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
            //}
        }
    }
}