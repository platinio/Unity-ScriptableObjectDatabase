using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace ArcaneOnyx.ScriptableObjectDatabase
{
    public static class ScriptableDatabaseUtil
    {
        private const string PrefsDatabaseKey = "{0}_SelectedDatabase";

        public static void UpdateActiveDatabase<Item, Database>(Database database)
            where Item : ScriptableItem 
            where Database : ScriptableDatabase<Item>
        {
            string databaseKey = string.Format(PrefsDatabaseKey, typeof(Database).Name);
            EditorPrefs.SetString(databaseKey, database.GetGuid());
        }

        public static Database GetActiveDatabase<Item, Database>()
            where Item : ScriptableItem 
            where Database : ScriptableDatabase<Item>
        {
            string databaseKey = string.Format(PrefsDatabaseKey, typeof(Database).Name);
            if (EditorPrefs.HasKey(databaseKey))
            {
                string guid = EditorPrefs.GetString(databaseKey);
                return GetDatabase<Database>(guid);
            }
            
            var db = GetDatabases<Database>().FirstOrDefault();
            EditorPrefs.SetString(databaseKey, db.GetGuid());

            return db;
        }
        
        public static Database GetDatabaseWhichContainsItem<Item, Database>(Item item)
            where Item : ScriptableItem 
            where Database : ScriptableDatabase<Item>
        {
            var databases = GetDatabases<Database>();

            foreach (var database in databases)
            {
                if (database.Contains(item))
                {
                    return database;
                }
            }

            return null;
        }

        public static Database GetDatabase<Database>(string databaseGuid) where Database : ScriptableObject
        {
           var guids = AssetDatabase.FindAssets($"t:{typeof(Database).Name}");

            foreach (var guid in guids)
            {
                if (!guid.Equals(databaseGuid)) continue;
                
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var database = AssetDatabase.LoadAssetAtPath(path, typeof(Database)) as Database;
                if (database == null) continue;

                return database;
            }

            return null;
        }
        
        public static List<Database> GetDatabases<Database>() where Database : ScriptableObject
        {
            List<Database> databases = new();
            var guids = AssetDatabase.FindAssets($"t:{typeof(Database).Name}");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var database = AssetDatabase.LoadAssetAtPath(path, typeof(Database)) as Database;
                if (database == null) continue;

                databases.Add(database);
            }

            return databases;
        }


        public static List<Item> GetAllItems<Item, Database>()
            where Item : ScriptableItem 
            where Database : ScriptableDatabase<Item>
        {
            List<Item> result = new();
            var databases = GetDatabases<Database>();

            foreach (var database in databases)
            {
                result.AddRange(database.Items);
            }

            return result;
        }

        public static List<ScriptableItem> GetDropdownOptions(Type databaseType)
        {
            List<ScriptableItem> allItems = new();
            var guids = AssetDatabase.FindAssets($"t:{databaseType.Name}");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var database = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject)) as ScriptableObject;
                if (database == null) continue;

                if (database.GetType() == databaseType)
                {
                    var obj = Convert.ChangeType(database, databaseType);
                   
                    var methodIndo = database.GetType().GetMethod("GetItems");
                    var result = methodIndo.Invoke(obj, null);
                    allItems.AddRange(result as IEnumerable<ScriptableItem>);
                }
            }

            return allItems;
        }
        

        public static void SaveDatabase<Item, Database>() 
            where Item : ScriptableItem 
            where Database : ScriptableDatabase<Item>
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(Database).Name}");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var database = AssetDatabase.LoadAssetAtPath(path, typeof(Database)) as Database;
                if (database == null) continue;

                foreach (var item in database.Items)
                {
                    item.name = item.Name;
                }
                
                AssetDatabase.SaveAssetIfDirty(database);
            }
        }
    }
}