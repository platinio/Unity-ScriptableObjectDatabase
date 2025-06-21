using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcaneOnyx.RPGSample
{
    public class InventorySlot : BaseSlotUI, IDragHandler, IBeginDragHandler, IEndDragHandler, IDropHandler, IPointerUpHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text amountLabel;
        [SerializeField] private GameObject amountContainer;
        [SerializeField] private Canvas dragCanvas;

        private InventoryItem currentInventoryItem;
        private Vector3 startPosition;
        private ItemGameRepresentation itemGameRepresentation;

        public bool IsEmpty => currentInventoryItem == null;
        public InventoryItem CurrentInventoryItem => currentInventoryItem;

        public Action<InventoryItem> OnAddItem;
        public Action<InventoryItem> OnRemoveItem;
        public event Action<InventorySlot, InventoryItem> OnUse;
        public event Action<InventorySlot, InventoryItem> OnDropItemOnGround;
        public event Action<InventorySlot, InventorySlot> OnDropItemInSlot;

        public int SlotIndex { get; set; }
        
        public void UpdateContent(InventoryItem inventoryItem)
        {
            if (inventoryItem == null)
            {
                ClearContent();
                return;
            }

            currentInventoryItem = inventoryItem;

            icon.enabled = true;
            icon.sprite = inventoryItem.Item.Icon;
            icon.color = Color.white;

            amountContainer.SetActive(inventoryItem.Item.StackableAmount > 1);
            amountLabel.text = inventoryItem.CurrentAmount.ToString();
        }

        public void ClearContent()
        {
            currentInventoryItem = null;
            icon.enabled = false;
            amountContainer.SetActive(false);
            dragCanvas.overrideSorting = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (IsEmpty) return;
            
            bool isOverUIElement = Mouse.IsOverUIElement();
            
            float alpha = isOverUIElement? 1 : 0;
            var c = icon.color;
            c.a = alpha;
            icon.color = c;

            icon.transform.position = eventData.position;
            
            if (!isOverUIElement && Mouse.RaycastTerrain(out var raycastHit))
            {
                var itemRepresentation = GetItemGameRepresentation();
                if (itemRepresentation) itemRepresentation.transform.position = raycastHit.point;
            }

            if (itemGameRepresentation != null)
            {
                itemGameRepresentation.gameObject.SetActive(!isOverUIElement);
            }
        }

        private ItemGameRepresentation GetItemGameRepresentation()
        {
            //if we have a representation to use but it is the incorrect one
            if (itemGameRepresentation != null && itemGameRepresentation.ItemDefinition != currentInventoryItem.Item)
            {
                Destroy(itemGameRepresentation.gameObject);
            }

            if (IsEmpty) return null;
            
            //create one if we dont have
            if (itemGameRepresentation == null)
            {
                itemGameRepresentation = currentInventoryItem.Item.CreateItemRepresentationInstance();
            }

            return itemGameRepresentation;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            startPosition = icon.transform.position;
            icon.raycastTarget = false;
            dragCanvas.overrideSorting = true;
        }

        public void OnEndDrag(PointerEventData eventData) => EndDrag();

        public void EndDrag()
        {
            if (!icon) return;
            
            icon.raycastTarget = true;
            icon.transform.position = startPosition;
            dragCanvas.overrideSorting = false;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!eventData.pointerDrag) return;

            var inventorySlot = eventData.pointerDrag.GetComponent<InventorySlot>();
            if (!inventorySlot) return;

            inventorySlot.EndDrag();
            OnDropItemInSlot?.Invoke(inventorySlot, this);
        }

        private void OnDestroy()
        {
            if (itemGameRepresentation) Destroy(itemGameRepresentation.gameObject);
            
            OnAddItem = null;
            OnRemoveItem = null;
            OnDropItemInSlot = null;
            OnDropItemOnGround = null;
        }

        protected override void OnPointerDoubleClick(PointerEventData eventData)
        {
            if (currentInventoryItem == null) return;
            OnUse?.Invoke(this, currentInventoryItem);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Mouse.IsOverUIElement() || !Mouse.RaycastTerrain(out var raycastHit)) return;

            itemGameRepresentation = null;
            OnDropItemOnGround?.Invoke(this, currentInventoryItem);
            ClearContent();
        }
    }
}

