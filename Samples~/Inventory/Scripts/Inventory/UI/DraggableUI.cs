using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcaneOnyx.RPGSample
{
    public class DraggableUI : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        private Vector2 dragPositionOffset;
        
        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position - dragPositionOffset;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Vector2 p = new Vector2(transform.position.x, transform.position.y);
            dragPositionOffset = eventData.position - p;
        }
    }

}

