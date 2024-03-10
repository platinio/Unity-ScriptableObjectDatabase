using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Platinio.ScriptableObjectDatabase
{
    public abstract class ScriptableItemEditor<ItemEditorWindow, Database, Entry> : Editor where ItemEditorWindow : DatabaseEditorWindow<Database, Entry> where Database : ScriptableDatabase<Entry> where Entry : ScriptableItem
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            ScriptableItem item = target as ScriptableItem;
            EditorGUILayout.LabelField($"Item ID: {item.Id}");
        }

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