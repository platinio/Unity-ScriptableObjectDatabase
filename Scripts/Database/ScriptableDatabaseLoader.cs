using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace ScriptableObjectDatabase
{
    public static class ScriptableDatabaseLoader
    {
        public static object LoadDatabase(Type databaseType, string name = null)
        {
            #if UNITY_EDITOR
            var guids = AssetDatabase.FindAssets($"t:{databaseType.Name}");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var database = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject)) as ScriptableObject;
                if (database == null) continue;

                if (database.GetType() == databaseType && (name == null || database.name == name))
                {
                    var obj = Convert.ChangeType(database, databaseType);
                    
                    var methodIndo = database.GetType().GetMethod("IsEnabled");
                    var result = methodIndo.Invoke(obj, null);

                    if (result is bool isEnable && isEnable) return obj;
                }
            }

            Debug.LogError($"Can't find a database of type {databaseType} do you have one created and enabled?");
            #endif
            return null;
        }
    }
}