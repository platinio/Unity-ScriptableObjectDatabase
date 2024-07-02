using System;
using Platinio.ScriptableObjectDatabase;
using UnityEngine;

namespace Platinio.InventorySample
{
    [Serializable]
    public class InventoryItem
    {
        [SerializeField, ScriptableItemDatabaseSelector(typeof(ItemDefinitionDatabase))] private ItemDefinition item;
        [SerializeField] private int currentAmount;

        public ItemDefinition Item => item;
        public int CurrentAmount => currentAmount;

        private int? preferredSlotIndex;

        public InventoryItem(ItemDefinition item, int currentAmount)
        {
            this.item = item;
            this.currentAmount = currentAmount;
        }

        public void Add(int addAmount, out int surplus)
        {
            int oldAmount = currentAmount;
            int newAmount = Mathf.Min(item.StackableAmount, addAmount + currentAmount);
            surplus = addAmount - (newAmount - oldAmount);

            currentAmount = newAmount;
        }

        public void UpdateAmount(int amount)
        {
            currentAmount = amount;
        }

        public void Remove(int amount)
        {
            currentAmount = Mathf.Max(0, currentAmount - amount);
        }

        public bool TryGetPreferredSlotIndex(out int slotIndex)
        {
            slotIndex = 0;
            if (preferredSlotIndex == null) return false;
            
            slotIndex = preferredSlotIndex.Value;
            return true;
        }

        public void UpdatePreferredSlotIndex(int slotIndex)
        {
            preferredSlotIndex = slotIndex;
        }

        public void Use(GameObject owner)
        {
            item.Use(owner);
        }
    }
}