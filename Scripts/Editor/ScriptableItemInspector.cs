using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace ScriptableObjectDatabase
{
    public abstract class ScriptableItemInspector : Editor
    {
        public abstract Type DatabaseType { get; }
      

        public void OnDisable()
        {
            ScriptableItem item = target as ScriptableItem;

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