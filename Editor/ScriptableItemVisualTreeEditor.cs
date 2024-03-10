using UnityEditor;
using UnityEngine;

namespace Platinio.ScriptableObjectDatabase
{
    public class ScriptableItemVisualTreeEditor<ItemEditorWindow, Database, Item> : Editor where ItemEditorWindow : DatabaseEditorWindow<Database, Item> where Database : ScriptableDatabase<Item> where Item : ScriptableItem
    {
        public void OpenInEditorWindow()
        {
            ItemEditorWindow wnd = EditorWindow.GetWindow<ItemEditorWindow>();
            wnd.titleContent = new GUIContent(wnd.GetWindowTitle());
                
            wnd.ChangeSelection(target as Item);
        }
    }
}

