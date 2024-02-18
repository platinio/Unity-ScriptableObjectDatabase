using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Platinio.ScriptableObjectDatabase
{
    /// <summary>
    /// class to hold a collection of scriptable items
    /// </summary>
    public class ScriptableDatabase<T> : ScriptableObject where T : ScriptableItem
    {
        [SerializeField] protected List<T> items;
        [SerializeField] private bool isEnabled = true;
        [SerializeField, HideInInspector] private uint id = 0;

        public int Count => items.Count;
        public IReadOnlyList<T> Items => items;

        private uint GetUniqueId() => id++;

        public T GetItem(uint itemId) => items.Find(x => x.Id == itemId);

        public T GetItemByName(string itemName)
        {
            return items.Find(x => x.name == itemName);
        }

        public bool IsEnabled() => isEnabled;

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
            
            item.SetId(GetUniqueId());
            items.Add(item);
            
            item.name = $"Item {item.Id}";
            item.Name = item.name;

            #if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(item, this);
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