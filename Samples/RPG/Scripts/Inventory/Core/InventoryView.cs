using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ArcaneOnyx.RPGSample
{
    public class InventoryView : MonoBehaviour
    {
        [Header("References")]
        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject visualRoot;
        [SerializeField] private Transform itemsParent;
        [SerializeField] private InventoryOwner inventoryModel;
        
        [Header("Config")]
        [SerializeField] private int size;
        
        [Header("Prefabs")]
        [SerializeField] private InventorySlot inventorySlot;

        [Header("Events")] 
        [SerializeField] private UnityEvent onOpen;
        [SerializeField] private UnityEvent onClose;

        private List<InventorySlot> slots = new();

        public bool IsOpen => visualRoot.activeInHierarchy;

        
        private InventoryController inventoryController;
      
        private void Awake()
        {
            CreateUI();
            Close();

            if (closeButton)
            {
                closeButton.onClick.AddListener(Close);
            }
        }
     
        public void Setup(InventoryOwner model, InventoryController controller)
        {
            inventoryModel = model;
            inventoryController = controller;
            inventoryModel.OnContentChanged = null;
            inventoryModel.OnContentChanged += Open;
        }

        public void Close()
        {
            SetVisibleValue(false);
            onClose?.Invoke();
        }
        
        private void SetVisibleValue(bool value)
        {
            if (!visualRoot) return;
            visualRoot.SetActive(value);
        }

        private void CreateUI()
        {
            for (int i = 0; i < size; i++)
            {
                var slot = Instantiate(inventorySlot, itemsParent);
                slot.OnAddItem += OnAddItem;
                slot.OnRemoveItem += OnRemoveItem;
                slot.OnUse += OnUseItem;
                slot.OnDropItemInSlot += OnDropItemInSlot;
                slot.OnDropItemOnGround += OnDropItemOnGround;
                slot.SlotIndex = i;
                
                slots.Add(slot);
            }
        }

        private void OnDropItemOnGround(InventorySlot slot, InventoryItem itemToDrop)
        {
            inventoryController?.RemoveItem(itemToDrop);
        }

        private void OnDropItemInSlot(InventorySlot from, InventorySlot to)
        {
            var fromItem = from.CurrentInventoryItem;
            if (fromItem == null) return;

            var toItem = to.CurrentInventoryItem;

            if (!inventoryModel.HasItem(fromItem))
            {
                int fromItemAmount = fromItem.CurrentAmount;
                from.OnRemoveItem?.Invoke(fromItem);
                
                if (!to.IsEmpty)
                {
                    int toItemAmount = toItem.CurrentAmount;
                    to.OnRemoveItem?.Invoke(to.CurrentInventoryItem);
                    
                    toItem.UpdateAmount(toItemAmount);
                    toItem.UpdatePreferredSlotIndex(from.SlotIndex);
                    from.OnAddItem?.Invoke(toItem);
                }

                inventoryController?.AddItem(fromItem.Item, fromItemAmount, to.SlotIndex);
            }
            else
            {
                to.UpdateContent(fromItem);
                from.UpdateContent(toItem);
            }

            SaveSlotCurrentPositions();
        }

        private void OnUseItem(InventorySlot slot, InventoryItem item)
        {
            if (!slot || item == null) return;
            inventoryController?.UseItem(item);
        }

        private void OnAddItem(InventoryItem item)
        {
            if (item == null) return;

            SaveSlotCurrentPositions();
            inventoryController?.AddItem(item);
        }

        private void OnRemoveItem(InventoryItem item)
        {
            if (item == null) return;
            
            SaveSlotCurrentPositions();
            inventoryController?.RemoveItem(item);
        }

        public void UpdateSlot(InventoryItem inventoryItem)
        {
            if (!inventoryItem.TryGetPreferredSlotIndex(out int slotIndex)) return;

            var slot = slots[slotIndex];
            
            if (inventoryItem.CurrentAmount <= 0)
            {
                slot.ClearContent();
            }
            else
            {
                slot.UpdateContent(inventoryItem);
            }
        }

        private void SaveSlotCurrentPositions()
        {
            foreach (var slot in slots)
            {
                if (slot.IsEmpty) continue;

                int index = slot.SlotIndex;
                slot.CurrentInventoryItem.UpdatePreferredSlotIndex(index);
            }
        }

        public void Open()
        {
            Clear();
            FillInventory(inventoryModel.Items);
            SetVisibleValue(true);
            SaveSlotCurrentPositions();
            
            onOpen.Invoke();
        }

        private void FillInventory(IReadOnlyCollection<InventoryItem> inventoryItems)
        {
            foreach (var inventoryItem in inventoryItems)
            {
                int slotIndex = GetSlotIndexForItem(inventoryItem);
                slots[slotIndex].UpdateContent(inventoryItem);
            }
        }

        private int GetSlotIndexForItem(InventoryItem item)
        {
            if (item.TryGetPreferredSlotIndex(out int slotIndex)) return slotIndex;
            
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (!slot.IsEmpty) continue;

                return i;
            }

            return -1;
        }

        public void OnNewItemAdded(InventoryItem inventoryItem)
        {
            AddInventoryItem(inventoryItem);
            SaveSlotCurrentPositions();
        }

        public void OnItemRemoved(InventoryItem inventoryItem)
        {
            if (!inventoryItem.TryGetPreferredSlotIndex(out int index)) return;
            slots[index].ClearContent();
        }

        public void OnItemAmountChanged(InventoryItem inventoryItem)
        {
            if (!inventoryItem.TryGetPreferredSlotIndex(out int index)) return;
            slots[index].UpdateContent(inventoryItem);
        }

        private void AddInventoryItem(InventoryItem inventoryItem)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (!slot.IsEmpty) continue;
                    
                slot.UpdateContent(inventoryItem);
                break;
            }
        }

        public void SetSlot(InventoryItem item, int index)
        {
            slots[index].UpdateContent(item);
        }

        private void Clear()
        {
            foreach (var slot in slots)
            {
                slot.ClearContent();
            }
        }
    }
}

