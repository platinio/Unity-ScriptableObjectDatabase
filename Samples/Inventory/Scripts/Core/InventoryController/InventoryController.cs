using UnityEngine;

namespace ArcaneOnyx.InventorySample
{
    /// <summary>
    /// Connect an inventoryView to an inventoryOwner
    /// </summary>
    public class InventoryController : MonoBehaviour
    {
        [SerializeField] protected InventoryView inventoryView;
        [SerializeField] private InventoryOwner inventoryOwner;

        public InventoryOwner OpenedInventoryOwner => inventoryOwner;
        
        public void UseItem(InventoryItem inventoryItem)
        {
            var item = inventoryItem.Item;
            item.Use(gameObject);

            if (item.DestroyAfterUse) RemoveItem(inventoryItem, 1);
        }

        public void AddItem(InventoryItem inventoryItem)
        {
            if (inventoryOwner == null) return;
            
            inventoryOwner.AddItem(inventoryItem);
            inventoryView.UpdateSlot(inventoryItem);
        }

        public void AddItem(ItemDefinition item, int amount, int preferredSlotIndex)
        {
            if (inventoryOwner == null) return;

            InventoryItem inventoryItem = new InventoryItem(item, amount);
            inventoryItem.UpdatePreferredSlotIndex(preferredSlotIndex);
            AddItem(inventoryItem);
        }

        public void RemoveItem(InventoryItem inventoryItem) => RemoveItem(inventoryItem, inventoryItem.CurrentAmount);
      
        public void RemoveItem(InventoryItem inventoryItem, int amount)
        {
            if (inventoryOwner == null) return;
            
            inventoryOwner.RemoveItem(inventoryItem, amount);
            inventoryView.UpdateSlot(inventoryItem);
        }

        public void Open()
        {
            if (!inventoryOwner) return;

            inventoryOwner.OnRemoveItem += inventoryView.OnItemRemoved;
            inventoryOwner.OnItemAmountChanged += inventoryView.OnItemAmountChanged;
            
            inventoryView.Setup(inventoryOwner, this);
            inventoryView.Open();
        }

        public void Close()
        {
            if (!inventoryOwner) return;
            
            inventoryOwner.OnRemoveItem -= inventoryView.OnItemRemoved;
            inventoryOwner.OnItemAmountChanged -= inventoryView.OnItemAmountChanged;
            
            inventoryView.Close();
        }
    }
}

