using UnityEditor;

namespace ArcaneOnyx.ScriptableObjectDatabase
{
    public abstract class ScriptableItemDefaultEditor<ItemEditorWindow, Database, Entry> : ScriptableItemInspector<ItemEditorWindow, Database, Entry> 
        where ItemEditorWindow : DatabaseEditorWindow<Database, Entry> 
        where Database : ScriptableDatabase<Entry> 
        where Entry : ScriptableItem
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            ScriptableItem item = target as ScriptableItem;
            EditorGUILayout.LabelField($"Item ID: {item.Id}");
        }
    }
}