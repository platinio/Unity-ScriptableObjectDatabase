using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace ScriptableObjectDatabase
{
    /// <summary>
    /// base class to hold a collection of data items
    /// </summary>
    public class ScriptableDatabase<T> : ScriptableObject where T : ScriptableItem
    {
        [SerializeField] private List<T> m_items;
        [SerializeField, HideInInspector] private List<T> m_itemsCopy;
        [SerializeField, HideInInspector] private uint m_id = 0;

        public int Count => m_items.Count;

        private uint GetUniqueId() => m_id++;

        public T GetItem(uint id)
        {
            if (m_items.Count <= id) return default;
            return m_items[(int)id];
        }

        public void AddItem(T item)
        {
            if (m_items.Contains(item)) return;

            item.ID = GetUniqueId();
            m_items.Add(item);
            
#if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(item, this);
            EditorUtility.SetDirty(this);
#endif
        }

        public bool Contains(T item) => m_items.Contains(item);

#if UNITY_EDITOR
        public void Clean()
        {
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
            m_itemsCopy = m_items.ToList();
        }


        public IEnumerable<ScriptableItemNameId> GetItemsNameAndId()
        {
            ScriptableItemNameId[] items = new ScriptableItemNameId[m_items.Count];
            for (uint i = 0; i < m_items.Count; i++)
            {
                if (m_items[(int)i] == null) continue;
                items[i] = new ScriptableItemNameId(i, m_items[(int)i]);
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