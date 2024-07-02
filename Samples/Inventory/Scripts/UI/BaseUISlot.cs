using UnityEngine;
using UnityEngine.EventSystems;

namespace Platinio.InventorySample
{
    public abstract class BaseSlotUI : MonoBehaviour, IPointerClickHandler
    {
        private float lastClickTime = float.MinValue;
        private const float doubleClickDelta = 0.35f;
        
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            float clickTimeDelta = Time.time - lastClickTime;
            if (clickTimeDelta < doubleClickDelta)
            {
                OnPointerDoubleClick(eventData);
            }
            else
            {
                lastClickTime = Time.time;
            }
        }

        protected abstract void OnPointerDoubleClick(PointerEventData eventData);
    }
}