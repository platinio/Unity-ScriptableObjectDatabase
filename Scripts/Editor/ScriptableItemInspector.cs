using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace ScriptableObjectDatabase
{
    public abstract class ScriptableItemInspector : Editor
    {
        public abstract Type DatabaseType { get; }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            ScriptableItem item = target as ScriptableItem;
            EditorGUILayout.LabelField($"Item ID: {item.Id}");
        }

        public void OnDisable()
        {
            ScriptableItem item = target as ScriptableItem;
            if (item == null) return;

            if (item.name != item.Name)
            {
                item.name = item.Name;

                var db = ScriptableDatabaseLoader.LoadDatabase(DatabaseType) as Object;
                EditorUtility.SetDirty(db);
                AssetDatabase.SaveAssets();
            }
        }
    }
}