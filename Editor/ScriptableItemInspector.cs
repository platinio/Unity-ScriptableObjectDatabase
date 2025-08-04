using UnityEditor;
using UnityEngine;

namespace ArcaneOnyx.ScriptableObjectDatabase
{
    public abstract class ScriptableItemInspector<ItemEditorWindow, Database, Entry> : Editor 
        where ItemEditorWindow : DatabaseEditorWindow<Database, Entry> 
        where Database : ScriptableDatabase<Entry> 
        where Entry : ScriptableItem
    {
        private const string PrefsDatabaseKey = "{0}_SelectedDatabase";
        
        public void OpenInEditorWindow()
        {
            ItemEditorWindow wnd = EditorWindow.GetWindow<ItemEditorWindow>();
            wnd.titleContent = new GUIContent(wnd.GetWindowTitle());

            EditorApplication.delayCall += () =>
            {
                var database = ScriptableDatabaseUtil.GetDatabaseWhichContainsItem<Entry, Database>(target as Entry);
                wnd.ChangeDatabaseSelection(database);
                wnd.ChangeSelection(target as Entry);
            };
        }
        
        public void OnDisable()
        {
            ScriptableItem item = target as ScriptableItem;
            if (item == null) return;

            if (item.name != item.Name)
            {
                item.name = item.Name;

                var activeDatabase = GetActiveDatabase();
                if (activeDatabase == null) return;
               
                EditorUtility.SetDirty(activeDatabase);
                AssetDatabase.SaveAssets();
            }
        }

        private Database GetActiveDatabase()
        {
            string databaseKey = string.Format(PrefsDatabaseKey, typeof(Database).Name);
            if (EditorPrefs.HasKey(databaseKey))
            {
                string guid = EditorPrefs.GetString(databaseKey);
                return ScriptableDatabaseUtil.GetDatabase<Database>(guid);
            }

            return null;
        }
    }
}