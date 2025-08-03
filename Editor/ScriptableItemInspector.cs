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

            EditorApplication.delayCall += () =>
            {
                var database = ScriptableDatabaseUtil.GetDatabaseWhichContainsItem<Entry, Database>(target as Entry);
                wnd.ChangeDatabaseSelection(database);
                wnd.ChangeSelection(target as Entry);
            };
        }
    }
}