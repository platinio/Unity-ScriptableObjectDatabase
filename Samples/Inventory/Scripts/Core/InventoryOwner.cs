using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneOnyx.InventorySample
{
    public class InventoryOwner : MonoBehaviour
    {
        [SerializeField] private List<InventoryItem> items = new();

        public IReadOnlyCollection<InventoryItem> Items => items;
        
        //called when something from the outside update the content
        public Action OnContentChanged { get; set; }

        //events
        public event Action<InventoryItem> OnRemoveItem;
        public event Action<InventoryItem> OnItemAmountChanged;

        public void AddItem(InventoryItem inventoryItem)
        {
            items.Add(inventoryItem);
        }

        public void RemoveItem(InventoryItem item, int amount)
        {
            item.Remove(amount);
            if (item.CurrentAmount <= 0)
            {
                items.Remove(item);
                OnRemoveItem?.Invoke(item);
            }
            else
            {
                OnItemAmountChanged?.Invoke(item);
            }
        }

        public void RemoveItem(InventoryItem item)
        {
            items.Remove(item);
            OnRemoveItem?.Invoke(item);
        }

        public bool HasItem(InventoryItem item) => items.Contains(item);
    }
}

