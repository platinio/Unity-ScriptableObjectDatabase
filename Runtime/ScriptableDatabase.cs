using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace ArcaneOnyx.ScriptableObjectDatabase
{
    /// <summary>
    /// class to hold a collection of scriptable items
    /// </summary>
    public class ScriptableDatabase<T> : ScriptableObject where T : ScriptableItem
    {
        [SerializeField] protected List<T> items = new();
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private bool isMainDatabase = true;
        [SerializeField, HideInInspector] private int id = 0;

        public int Count => items.Count;
        public IReadOnlyList<T> Items => items;

        private int GetUniqueId() => id++;

        public T GetItem(string itemId) => items.Find(x => x.Id == itemId);

        public T GetItemByName(string itemName)
        {
            return items.Find(x => x.name == itemName);
        }

        public bool IsEnabled() => isEnabled;
        public bool IsMainDatabse() => isEnabled && isMainDatabase;

        public void OnSave()
        {
            foreach (var item in items)
            {
                item.OnSave();
            }
        }

        public void AddItem(T item)
        {
            if (items.Contains(item)) return;


            string id = GetId(GetUniqueId());
            item.SetId(id);
            items.Add(item);
            
            item.name = "New Item";
            item.Name = item.name;

            #if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(item, this);
            EditorUtility.SetDirty(this);
            #endif
        }
        
        private string GetId(int idx)
        {
#if UNITY_EDITOR
            string assetPath = AssetDatabase.GetAssetPath(this);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            return $"{guid}_{idx}";
#else
            return string.Empty;
#endif
        }

        public string GetGuid()
        {
#if UNITY_EDITOR
            string assetPath = AssetDatabase.GetAssetPath(this);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            return guid;
#else
            return string.Empty;
#endif
        }

        public void MigrateIds()
        {
            foreach (var item in items)
            {
                if (int.TryParse(item.Id, out int idx))
                {
                    item.SetId(GetId(idx));
                }
            }
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void RemoveItem(T item)
        {
            if (!Contains(item)) return;
            items.Remove(item);
            #if UNITY_EDITOR
            DestroyImmediate(item, true);
            EditorUtility.SetDirty(this);
            #endif
        }

        public void SwapItems(T a, T b)
        {
            int aIndex = items.IndexOf(a);
            int bIndex = items.IndexOf(b);

            items[aIndex] = b;
            items[bIndex] = a;
        }

        public int GetItemIndex(T item) => items.IndexOf(item);

        public bool Contains(T item) => items.Contains(item);

        public IEnumerable<T> GetItems() => items;

    }
}