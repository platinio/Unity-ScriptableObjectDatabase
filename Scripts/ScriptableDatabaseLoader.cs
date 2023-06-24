using System;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjectDatabase
{
    public static class ScriptableDatabaseLoader
    {
        public static object LoadDatabase(Type databaseType)
        {
            var guids = AssetDatabase.FindAssets($"t:{databaseType.Name}");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var database = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject)) as ScriptableObject;
                if (database == null) continue;

                if (database.GetType() == databaseType) return Convert.ChangeType(database, databaseType);
            }

            return null;
        }
    }
}