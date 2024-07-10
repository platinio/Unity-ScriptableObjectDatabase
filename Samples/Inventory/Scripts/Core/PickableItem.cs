using System.Collections;
using UnityEngine;

namespace ArcaneOnyx.InventorySample
{
    public class PickableItem : MonoBehaviour, IGameInteractable
    {
        [SerializeField] private float travelTime = 1.0f;

        private bool hasAlreadyBeenPicked = false;
        
        public void Interact()
        {
            if (hasAlreadyBeenPicked) return;

            hasAlreadyBeenPicked = true;
            var player =  GameObject.FindWithTag("Player");
            var inventoryOwner = player.GetComponent<InventoryOwner>();
            var itemGameRepresentation = GetComponent<ItemGameRepresentation>();
            
            inventoryOwner.AddItem(new InventoryItem(itemGameRepresentation.ItemDefinition, 1));

            StartCoroutine(MoveToPlayer(player));
        }

        private IEnumerator MoveToPlayer(GameObject player)
        {

            float normalizeDuration = 0.0f;
            float duration = 0.0f;
            Vector3 from = transform.position;
            
            while (normalizeDuration < 1.0f)
            {
                duration += Time.deltaTime;
                normalizeDuration = duration / travelTime;
                transform.position = Vector3.Lerp(from, player.transform.position, normalizeDuration);

                yield return null;
            }
            
            Destroy(gameObject);
        }
    }
}

