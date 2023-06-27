using System;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjectDatabase
{
    public static class ScriptableDatabaseLoader
    {
        public static object LoadDatabase(Type databaseType, string name = null)
        {
            var guids = AssetDatabase.FindAssets($"t:{databaseType.Name}");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var database = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject)) as ScriptableObject;
                if (database == null) continue;

                if (database.GetType() == databaseType && (name == null || database.name == name))
                {
                    return Convert.ChangeType(database, databaseType);
                }
            }

            return null;
        }
    }
}