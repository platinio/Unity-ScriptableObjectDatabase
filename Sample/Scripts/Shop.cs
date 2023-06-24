using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjectDatabase.Sample
{
    public class Shop : MonoBehaviour
    {
        [SerializeField, ScriptableItemDatabaseSelector(DatabaseType = typeof(ShopItemsDatabase))]
        private List<ShopItem> m_shopItems;

        private void Start()
        {
            foreach (var shopItem in m_shopItems)
            {
                Debug.Log($"Item ID: {shopItem.ID}");
                Debug.Log($"Item Name: {shopItem.Name}");
                Debug.Log($"Item Cost: {shopItem.Cost}");
            }
        }
    }
}