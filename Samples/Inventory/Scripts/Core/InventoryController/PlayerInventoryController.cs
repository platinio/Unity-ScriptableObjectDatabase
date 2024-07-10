using UnityEngine;

namespace ArcaneOnyx.InventorySample
{
    public class PlayerInventoryController : InventoryController
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                Toggle();
            }
        }

        private void Toggle()
        {
            if (inventoryView.IsOpen) Close();
            else Open();
        }
    }
}

