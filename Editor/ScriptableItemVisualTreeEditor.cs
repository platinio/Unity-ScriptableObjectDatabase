using UnityEditor;
using UnityEngine;

namespace ArcaneOnyx.ScriptableObjectDatabase
{
    public class ScriptableItemVisualTreeEditor<ItemEditorWindow, Database, Item> : Editor where ItemEditorWindow : DatabaseEditorWindow<Database, Item> where Database : ScriptableDatabase<Item> where Item : ScriptableItem
    {
        public void OpenInEditorWindow()
        {
            ItemEditorWindow wnd = EditorWindow.GetWindow<ItemEditorWindow>();
            wnd.titleContent = new GUIContent(wnd.GetWindowTitle());
                
            var database = ScriptableDatabaseUtil.GetDatabaseWhichContainsItem<Item, Database>(target as Item);
            wnd.ChangeDatabaseSelection(database);
            wnd.ChangeSelection(target as Item);
        }
    }
}

