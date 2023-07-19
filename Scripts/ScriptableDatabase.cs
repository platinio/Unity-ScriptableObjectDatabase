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
        [SerializeField] private List<T> m_items;
        [SerializeField] private bool m_isEnabled = true;
        [SerializeField, HideInInspector] private List<T> m_itemsCopy;
        [SerializeField, HideInInspector] private uint m_id = 0;

        public int Count => m_items.Count;
        public IEnumerable<T> Items => m_items;

        private uint GetUniqueId() => m_id++;

        public T GetItem(uint id) => m_items.Find(x => x.Id == id);

        public bool IsEnabled() => m_isEnabled;
        
        public void AddItem(T item)
        {
            if (m_items.Contains(item)) return;

            item.Id = GetUniqueId();
            m_items.Add(item);
            
            item.name = $"Item {item.Id}";
            item.Name = item.name;

#if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(item, this);
            EditorUtility.SetDirty(this);
#endif
        }

        public bool Contains(T item) => m_items.Contains(item);

#if UNITY_EDITOR
        public void Clean()
        {
            if (m_items == null || m_items.Count == 0) return;
            
            m_items = m_items.Distinct().ToList();
            if (m_items.Count >= m_itemsCopy.Count) return;

            foreach (var item in m_itemsCopy)
            {
                if (!m_items.Contains(item)) DestroyImmediate(item, true);
            }
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif

        public void UpdateItemsCopy()
        {
            if (m_items == null || m_items.Count == 0) return;
            m_itemsCopy = m_items.ToList();
        }

        /// <summary>
        /// returns a collection containing the name and the id of each item in the database
        /// </summary>
        public IEnumerable<ScriptableItemNameId> GetItemsNameAndId()
        {
            ScriptableItemNameId[] items = new ScriptableItemNameId[m_items.Count];
            for (uint i = 0; i < m_items.Count; i++)
            {
                var item = m_items[(int)i];
                if (item == null) continue;
                items[i] = new ScriptableItemNameId(item.Id, m_items[(int)i]);
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