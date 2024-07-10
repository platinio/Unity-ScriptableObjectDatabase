using UnityEngine;

namespace ArcaneOnyx.InventorySample
{
    public class MouseInteraction : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryInteract();
            }
        }

        private void TryInteract()
        {
            if (Mouse.IsOverUIElement()) return;

            var raycastResult = Mouse.RaycastAll();
            if (raycastResult.Length == 0) return;

            foreach (var raycastHit in raycastResult)
            {
                var gameInteractable = raycastHit.collider.GetComponent<IGameInteractable>();
                if (gameInteractable == null) continue;
            
                gameInteractable.Interact();
            }
        }
    }
}

