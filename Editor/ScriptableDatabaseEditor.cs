using UnityEditor;
using UnityEngine;

namespace ArcaneOnyx.ScriptableObjectDatabase
{
    public class ScriptableDatabaseEditor<DatabaseEditorWindowType, Database, Entry> : Editor where DatabaseEditorWindowType : DatabaseEditorWindow<Database, Entry>
        where Database : ScriptableDatabase<Entry> 
        where Entry : ScriptableItem
    {
        private SerializedProperty isEnabledProperty = null;

        private void OnEnable()
        {
            isEnabledProperty = serializedObject.FindProperty("isEnabled");
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor Window"))
            {
                DatabaseEditorWindowType wnd = EditorWindow.GetWindow<DatabaseEditorWindowType>();
                wnd.titleContent = new GUIContent(wnd.GetWindowTitle());
            }
            
            EditorGUILayout.PropertyField(isEnabledProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}