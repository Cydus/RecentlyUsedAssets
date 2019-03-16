using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RUA
{
    /// <summary>
    /// Impliments a Queue like datastructure for recent asset storage and 
    /// pushes changes to editor window. MVC-esque.
    /// </summary>
    public class RecentlyUsedAssetsQueue
    {
        private static readonly int list_capacity = 50;
        private readonly string favouriteSuffix = "favs";
        private readonly string normalSuffix = "normal";

        private static RecentlyUsedAssetsQueue _instance;
        public static RecentlyUsedAssetsQueue Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RecentlyUsedAssetsQueue();
                    _instance.Load();
                }
                return _instance;
            }
        }

        [InitializeOnLoadMethod]
        static void OnProjectLoadedInEditor()
        {
            _instance = new RecentlyUsedAssetsQueue();
            _instance.Load();
        }

        private List<RecentAsset> recentAssets = new List<RecentAsset>(list_capacity);
        private List<RecentAsset> favourites = new List<RecentAsset>(list_capacity);

        public void Add(string guid, bool save)
        {

            if (IsInFavs(guid))
                AddToList(favourites, guid, save);
            else
                AddToList(recentAssets, guid, save);
        }

        private bool isReimportedAndSelectedGUID(string guid)
        {
            bool returnValue = false;
            if (AssetImportDetector.priorSelectedGUIDs.Contains<string>(guid))
                returnValue = true;
            return returnValue;
        }


        public void AddToList(List<RecentAsset> assetList, string guid, bool save)
        {
            //Debug.Log("adding");
            
            RecentAsset assetToAdd = new RecentAsset(guid);

            if (assetList.Count >= assetList.Capacity)
                assetList.RemoveAt(0);

            if (isReimportedAndSelectedGUID(guid))
                assetToAdd._selected = true;

            if (IsSelected(guid))
                assetToAdd._selected = true;

            RemoveAssetsWithGUID(assetList, guid);

            assetList.Add(assetToAdd);
                Save();
        }

        private bool IsInFavs(string GUID)
        {
            return favourites.Exists(x => x._GUID == GUID);
        }

        public void SetFavourite(RecentAsset ra, bool isFavourite)
        {
            if (isFavourite)
            {
                recentAssets.Remove(ra);
                favourites.Add(ra);
            }
            else
            {
                favourites.Remove(ra);
                AddToList(recentAssets, ra._GUID, true);
            }
            Save();
        }

        private bool IsSelected(string guid)
        {
            bool selected = false;
            if (Selection.assetGUIDs.Contains(guid))
                selected = true;
            return selected;
        }

        public void RemoveAssetsWithGUID(string guid, bool pushToGUI = false)
        {
            RemoveAssetsWithGUID(favourites, guid, pushToGUI);
            RemoveAssetsWithGUID(recentAssets, guid, pushToGUI);
        }

        private void RemoveAssetsWithGUID(List<RecentAsset> assetList, string guid, bool pushToGUI = false)
        {
            assetList.RemoveAll(x => x._GUID == guid);
            if (pushToGUI)
                Save();
        }

        private void Load()
        {
            string[] normalGUIDS = GUIDStore.Instance.LoadGUIDs(normalSuffix);
            string[] favGUIDS = GUIDStore.Instance.LoadGUIDs(favouriteSuffix);

            //Debug.Log("loaded up " + normalGUIDS.Length +"-" + favGUIDS.Length);

            foreach (string guid in normalGUIDS)
                AddToList(recentAssets, guid, false);

            foreach (string guid in favGUIDS)
                AddToList(favourites, guid, false);
        }

        public void Clear()
        {
            favourites.Clear();
            recentAssets.Clear();

            Save();
            Load();
        }

        private void Save()
        {
            //Debug.Log("saving");
            //recentAssets.Clear();
            RecentlyUsedWindow.Favourites = favourites;
            RecentlyUsedWindow.RecentAssets = recentAssets;

            GUIDStore.Instance.SaveGUIDs(recentAssets, normalSuffix);
            GUIDStore.Instance.SaveGUIDs(favourites, favouriteSuffix);
        }
    }
}
