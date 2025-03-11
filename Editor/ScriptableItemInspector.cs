using UnityEditor;
using UnityEngine;

namespace ArcaneOnyx.ScriptableObjectDatabase
{
    public abstract class ScriptableItemInspector<ItemEditorWindow, Database, Entry> : Editor 
        where ItemEditorWindow : DatabaseEditorWindow<Database, Entry> 
        where Database : ScriptableDatabase<Entry> 
        where Entry : ScriptableItem
    {
        public void OpenInEditorWindow()
        {
            ItemEditorWindow wnd = EditorWindow.GetWindow<ItemEditorWindow>();
            wnd.titleContent = new GUIContent(wnd.GetWindowTitle());
                
            wnd.ChangeSelection(target as Entry);
        }

        public void OnDisable()
        {
            ScriptableItem item = target as ScriptableItem;
            if (item == null) return;

            if (item.name != item.Name)
            {
                item.name = item.Name;

                var db = ScriptableDatabaseLoader.LoadDatabase(typeof(Database)) as Object;
                EditorUtility.SetDirty(db);
                AssetDatabase.SaveAssets();
            }
        }
    }
}