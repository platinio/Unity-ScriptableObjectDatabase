using UnityEditor;
using UnityEngine;

namespace ArcaneOnyx.ScriptableObjectDatabase
{
    public class ScriptableDatabaseEditor<DatabaseEditorWindowType, Database, Entry> : Editor where DatabaseEditorWindowType : DatabaseEditorWindow<Database, Entry>
        where Database : ScriptableDatabase<Entry> 
        where Entry : ScriptableItem
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor Window"))
            {
                DatabaseEditorWindowType wnd = EditorWindow.GetWindow<DatabaseEditorWindowType>();
                wnd.titleContent = new GUIContent(wnd.GetWindowTitle());
            }
        }
    }
}