using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RUA
{
    /// <summary>
    /// Window defintion and GUI for Recently Used Assets.
    /// </summary>
    public class RecentlyUsedWindow : EditorWindow
    {
        public static List<RecentAsset> Favourites = new List<RecentAsset>();
        static Texture2D fav_icon_enabled;
        Texture2D fav_icon_disabled;

        Texture2D SelectedTexture;

        static List<RecentAsset> _recentAssets = new List<RecentAsset>();
        public static List<RecentAsset> RecentAssets
        {
            set
            {
                _recentAssets = value;
            }
        }

        Vector2 scrollPos;
        bool dragable = true;

        bool doubleClicked = false;
        bool rightClicked = false;

        [MenuItem("Window/Asset Management/Recently Used Assets")]
        static void Init()
        {
            //Debug.Log("window init");
            RecentlyUsedWindow window = (RecentlyUsedWindow)EditorWindow.GetWindow(typeof(RecentlyUsedWindow), false, "Recently Used Assets");
            window.Show();
        }

        private void OnEnable()
        {
            SelectedTexture = new Texture2D(1, 1);
            SelectedTexture.SetPixel(0, 0, new Color(0.2431f, 0.49020f, 0.90588f));
            SelectedTexture.Apply();

            Selection.selectionChanged += HandleChangedEvent;

            fav_icon_enabled = Resources.Load<Texture2D>("icons/favourite_icon");
            fav_icon_disabled = Resources.Load<Texture2D>("icons/d_favourite_icon");
        }

        private void OnDisable()
        {
            //Debug.Log("disabled");
            Selection.selectionChanged -= HandleChangedEvent;
        }

        private void HandleChangedEvent()
        {
            //Debug.Log("--- selection changed detected --- ");
            List<string> selectedGuids = new List<string>();

            List<Object> ids = new List<Object>();
            ids = Selection.objects.ToList<Object>();

            // Do this instead of Selection.assetGUIDs due to issues with folder re-importing.
            foreach (Object obj in ids)
            {
                string path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
                selectedGuids.Add(AssetDatabase.AssetPathToGUID(path));
                //Debug.Log("path " + AssetDatabase.GetAssetPath(obj.GetInstanceID()));
            }

            List<RecentAsset> selected = _recentAssets.Where(a => selectedGuids.Any(x => x == a._GUID)).ToList();
            List<RecentAsset> selectedInFavs = Favourites.Where(a => selectedGuids.Any(x => x == a._GUID)).ToList();

            foreach (RecentAsset ra in _recentAssets)
                ra._selected = false;

            foreach (RecentAsset ra in selected)
            {
                ra._selected = true;
                //Debug.Log(AssetDatabase.GUIDToAssetPath(ra._GUID));
            }

            foreach (RecentAsset ra in Favourites)
                ra._selected = false;

            foreach (RecentAsset ra in selectedInFavs)
                ra._selected = true;

            Repaint();
        }

        GUIStyle GetBtnStyle(bool selected)
        {

            var s = new GUIStyle();

            if (selected)
            {
                s.normal.background = SelectedTexture;
                s.normal.textColor = Color.white;
            }

            s.padding = new RectOffset(4, 0, 0, 0);
            s.fixedHeight = 16;

            return s;
        }
        else
        { 
            if (EditorGUIUtility.isProSkin)
                s.normal.textColor = new Color(0.760f, 0.760f, 0.760f);
        }

        GUIStyle GetFavouriteStyle()
        {
            var s = new GUIStyle();
            s.padding = new RectOffset(0, 0, 1, 1);
            s.fixedHeight = 16;
            s.fixedWidth = 16;
            return s;
        }

        void OnGUI()
        {
            float windowWidth = position.width;

            float windowHeight = position.height;

            // space fix
            EditorGUILayout.BeginVertical();
            GUILayout.Space(1);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(windowWidth), GUILayout.Height(windowHeight));

            Event currentEvent = Event.current;

            if (currentEvent.isMouse && currentEvent.type == EventType.MouseDown && currentEvent.clickCount == 2)
                doubleClicked = true;

            if (currentEvent.isMouse && currentEvent.button == 1 && currentEvent.type == EventType.MouseUp)
                rightClicked = true;

            if (Favourites.Count > 0)
            {
                RenderAssetList(Favourites, windowWidth, currentEvent, true);
                GuiLine(1);
            }
            RenderAssetList(_recentAssets, windowWidth, currentEvent, false);


            if (Favourites.Count == 0 && _recentAssets.Count == 0)
            {
                EditorGUILayout.LabelField("Empty. Drag or save assets to add.");
            }

            if (Favourites.Count > 0 || _recentAssets.Count > 0)
            {
                if (GUILayout.Button("clear"))
                {

                    RecentlyUsedAssetsQueue.Instance.Clear();
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();

            DropAreaGUI();
        }

        private void DragGUI(Rect rect, Object obj)
        {
            Event evt = Event.current;
            if (evt.type == EventType.MouseDrag)
            {
                //Debug.Log("MouseDrag");
                if (rect.Contains(evt.mousePosition))
                {
                    //Debug.Log("contains");
                    Object[] objRefs = new Object[] { obj };
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = objRefs;
                    DragAndDrop.StartDrag("RUADrag");
                }
            }
        }

        private bool isDragTagetValid(Object obj)
        {
            bool valid = true;
            string path =   AssetDatabase.GetAssetOrScenePath(obj);
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (Favourites.Exists(x => x._GUID == guid) || _recentAssets.Exists(x => x._GUID == guid))
            {
                valid = false;
            }

            return valid;
        }

        private void DropAreaGUI()
        {
            Event evt = Event.current;

            if (evt.type == EventType.DragUpdated)
            {
                
                dragable = true;
 
                foreach (Object obj in DragAndDrop.objectReferences)
                {
                    if (AssetDatabase.GetAssetPath(obj) == "")
                    {
                        dragable = false;
                    }

                    if (!isDragTagetValid(obj)) {
                        dragable = false;
                    }

                    if (!dragable)
                        break;
                }
                if (dragable)
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            }

            if (evt.type == EventType.DragPerform)
            {
                if (!dragable)
                    return;

                DragAndDrop.AcceptDrag();

                foreach (Object obj in DragAndDrop.objectReferences)
                {
                    if (AssetDatabase.GetAssetPath(obj) == "")
                        continue;

                    string path = AssetDatabase.GetAssetPath(obj);
                    string guid = AssetDatabase.AssetPathToGUID(path);


                    RecentlyUsedAssetsQueue.Instance.Add(guid, true);
                }
            }
        }

        private void RenderAssetList(List<RecentAsset> assetList, float windowWidth, Event currentEvent, bool favsList)
        {
            for (int i = assetList.Count - 1; i >= 0; i--)
            {
                bool sel = assetList[i]._selected;

                string go = AssetDatabase.GUIDToAssetPath(assetList[i]._GUID);
                GUIContent iconContent = new GUIContent();

                iconContent.image = AssetDatabase.GetCachedIcon(go);
                iconContent.text = assetList[i].Name;

                GUIContent favIcon = new GUIContent();
                if (favsList)
                    favIcon.image = fav_icon_disabled;
                else
                    favIcon.image = fav_icon_enabled;

                Object theGameObjectIwantToSelect = AssetDatabase.LoadAssetAtPath(go, typeof(Object));

                Event e = Event.current;

                EditorGUILayout.BeginHorizontal(GetBtnStyle(sel));
                Rect buttonRect = GUILayoutUtility.GetRect(iconContent, GetBtnStyle(sel), GUILayout.Width(windowWidth - 44));

                if (e.isMouse && buttonRect.Contains(e.mousePosition) && e.type != EventType.MouseDrag)
                {
                    //Debug.Log("Mouse pressed: " + (e.type == EventType.MouseDown));

                    if (doubleClicked)
                    {
                        //Debug.Log(go);
                        doubleClicked = false;
                        AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(go));
                    }

                    Vector2 mousePos = currentEvent.mousePosition;
                    if (rightClicked)
                    {
                        EditorUtility.DisplayPopupMenu(new Rect(mousePos.x, mousePos.y, 0, 0), "Assets/", null);
                        rightClicked = false;
                    }
                }

                if (e.type == EventType.MouseUp && buttonRect.Contains(e.mousePosition)) {
                    //Selection.objects = new Object[] { theGameObjectIwantToSelect };
                    Selection.activeObject = theGameObjectIwantToSelect;
                }

                DragGUI(buttonRect, theGameObjectIwantToSelect);

                GUI.Label(buttonRect, iconContent, GetBtnStyle(sel));

                if (GUILayout.Button(favIcon, GetFavouriteStyle()))
                {
                    bool setting = favsList ? false : true;
                    //Debug.Log("favourted " + setting);
                    RecentlyUsedAssetsQueue.Instance.SetFavourite(assetList[i], setting);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        void GuiLine(int i_height = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);
            rect.height = i_height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }
    }


}
