using UnityEngine;

namespace Platinio.InventorySample
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

