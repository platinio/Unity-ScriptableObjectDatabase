using UnityEngine;

namespace ArcaneOnyx.RPGSample
{
    public class PlayerInventoryController : InventoryController
    {
        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current.iKey.wasPressedThisFrame)
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

