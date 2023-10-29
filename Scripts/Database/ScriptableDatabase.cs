using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace ScriptableObjectDatabase
{
    /// <summary>
    /// class to hold a collection of scriptable items
    /// </summary>
    public class ScriptableDatabase<T> : ScriptableObject where T : ScriptableItem
    {
        [SerializeField] protected List<T> items;
        [SerializeField] private bool isEnabled = true;
        [SerializeField, HideInInspector] private List<T> itemsCopy;
        [SerializeField, HideInInspector] private uint id = 0;

        public int Count => items.Count;
        public IEnumerable<T> Items => items;

        private uint GetUniqueId() => id++;

        public T GetItem(uint id) => items.Find(x => x.Id == id);

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

            item.Id = GetUniqueId();
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
            UpdateItemsCopy();
            
            #if UNITY_EDITOR
            DestroyImmediate(item, true);
            EditorUtility.SetDirty(this);
            #endif
        }

        public bool Contains(T item) => items.Contains(item);

#if UNITY_EDITOR
        public void Clean()
        {
            if (items == null || items.Count == 0) return;
            
            items = items.Distinct().ToList();
            if (items.Count >= itemsCopy.Count) return;

            foreach (var item in itemsCopy)
            {
                if (!items.Contains(item)) DestroyImmediate(item, true);
            }
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif

        public void UpdateItemsCopy()
        {
            if (items == null || items.Count == 0) return;
            itemsCopy = items.ToList();
        }

        /// <summary>
        /// returns a collection containing the name and the id of each item in the database
        /// </summary>
        public IEnumerable<ScriptableItemNameId> GetItemsNameAndId()
        {
            ScriptableItemNameId[] items = new ScriptableItemNameId[this.items.Count];
            for (uint i = 0; i < this.items.Count; i++)
            {
                var item = this.items[(int)i];
                if (item == null) continue;
                items[i] = new ScriptableItemNameId(item.Id, this.items[(int)i]);
            }

            return items;
        }
    }

    public class ScriptableItemNameId
    {
        public uint Id;
        public string Name;

        public ScriptableItemNameId(uint id, ScriptableItem scriptableItem)
        {
            Id = id;
            Name = scriptableItem.name;
        }
    }
}