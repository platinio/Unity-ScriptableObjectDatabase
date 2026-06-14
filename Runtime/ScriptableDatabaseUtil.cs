using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
#if UNITY_EDITOR
            string databaseKey = string.Format(PrefsDatabaseKey, typeof(Database).Name);
            EditorPrefs.SetString(databaseKey, database.GetGuid());
#endif
        }

        public static Database GetActiveDatabase<Item, Database>()
            where Item : ScriptableItem 
            where Database : ScriptableDatabase<Item>
        {
#if UNITY_EDITOR
            string databaseKey = string.Format(PrefsDatabaseKey, typeof(Database).Name);
            if (EditorPrefs.HasKey(databaseKey))
            {
                string guid = EditorPrefs.GetString(databaseKey);
                var activeDb = GetDatabase<Database>(guid);

                if (activeDb != null)
                {
                    return activeDb;
                }
            }
            
            var db = GetDatabases<Database>().FirstOrDefault();

            if (db == null)
            {
                Debug.LogError($"There arent active databases of type {typeof(Database)}");
                return null;
            }

            EditorPrefs.SetString(databaseKey, db.GetGuid());

            return db;
#else
            return null;
#endif
        }
        
        public static Database GetDatabaseWhichContainsItem<Item, Database>(Item item)
            where Item : ScriptableItem 
            where Database : ScriptableDatabase<Item>
        {
#if UNITY_EDITOR
            var databases = GetDatabases<Database>();

            foreach (var database in databases)
            {
                if (database.Contains(item))
                {
                    return database;
                }
            }
#endif

            return null;
        }

        public static Database GetDatabase<Database>(string databaseGuid) where Database : ScriptableObject
        {
#if UNITY_EDITOR
           var guids = AssetDatabase.FindAssets($"t:{typeof(Database).Name}");

            foreach (var guid in guids)
            {
                if (!guid.Equals(databaseGuid)) continue;
                
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var database = AssetDatabase.LoadAssetAtPath(path, typeof(Database)) as Database;
                if (database == null) continue;

                return database;
            }
#endif

            return null;
        }
        
        public static List<Database> GetDatabases<Database>() where Database : ScriptableObject
        {
#if UNITY_EDITOR
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
#else
            return null;
#endif
        }


        public static List<Item> GetAllItems<Item, Database>()
            where Item : ScriptableItem 
            where Database : ScriptableDatabase<Item>
        {
#if UNITY_EDITOR
            List<Item> result = new();
            var databases = GetDatabases<Database>();

            foreach (var database in databases)
            {
                result.AddRange(database.Items);
            }

            return result;
#else
            return null;
#endif
        }
        
        public static Item GetItemByName<Item, Database>(string itemName)
            where Item : ScriptableItem 
            where Database : ScriptableDatabase<Item>
        {
#if UNITY_EDITOR
            var dbs = GetDatabases<Database>();

            foreach (var db in dbs)
            {
                var item = db.GetItemByName(itemName);
                if (item != null)
                {
                    return item;
                }
            }
#endif

            return null;
        }

        public static List<ScriptableItem> GetDropdownOptions(Type databaseType)
        {
#if UNITY_EDITOR
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
#else
            return null;
#endif
        }
        

        public static void SaveDatabase<Item, Database>() 
            where Item : ScriptableItem 
            where Database : ScriptableDatabase<Item>
        {
#if UNITY_EDITOR
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
#endif
        }
    }
}