using UnityEngine;

namespace ArcaneOnyx.RPGSample
{
    public class Lootbox : MonoBehaviour, IGameInteractable
    {
        [SerializeField] private InventoryController inventoryController;
        
        public void Interact()
        {
            inventoryController.Open();
        }
    }
}

