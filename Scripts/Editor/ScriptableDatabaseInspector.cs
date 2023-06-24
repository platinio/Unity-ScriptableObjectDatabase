using UnityEditor;
using UnityEngine;

namespace ScriptableObjectDatabase
{
    public class ScriptableDatabaseInspector<T> : Editor where T : ScriptableItem
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ScriptableDatabase<T> db = target as ScriptableDatabase<T>;
            
            if (GUILayout.Button("Create Item"))
            {
                var item = CreateInstance(typeof(T)) as T;
                item.name = $"Item {db.Count}";

                db.AddItem(item);
                
                EditorUtility.SetDirty(db);
                AssetDatabase.SaveAssets();
            }
            
            db.Clean();
            db.UpdateItemsCopy();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}