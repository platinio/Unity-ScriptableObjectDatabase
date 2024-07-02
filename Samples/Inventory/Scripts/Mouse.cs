using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Platinio.InventorySample
{
    public static class Mouse
    {
        private static int UILayer => LayerMask.NameToLayer("UI");
        private static int TerrainLayer => 1 << LayerMask.NameToLayer("Default");
        private static Camera camera = null;
        
        public static bool Raycast(LayerMask layerMask, out RaycastHit raycastHit)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out raycastHit, 1000.0f, layerMask);
        }
        
        public static RaycastHit[] RaycastAll()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.RaycastAll(ray, 1000.0f);
        }

        public static bool IsOverUIElement() => IsPointerOverUIElement(GetEventSystemRaycastResults());

        public static bool RaycastTerrain(out RaycastHit raycastHit) => Raycast(TerrainLayer, out raycastHit);
        
        private static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaycasterResults)
        {
            if (eventSystemRaycasterResults == null) return false;
            
            for (int index = 0; index < eventSystemRaycasterResults.Count; index++)
            {
                RaycastResult currentRaycasterResult = eventSystemRaycasterResults[index];
                if (currentRaycasterResult.gameObject.layer == UILayer) return true;
            }
            return false;
        }
        
        private static List<RaycastResult> GetEventSystemRaycastResults()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> raycastResults = new List<RaycastResult>();

            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                Debug.LogError("Event System is null, please create one");
                return null;
            }

            EventSystem.current.RaycastAll(eventData, raycastResults);
            return raycastResults;
        }
    }
}