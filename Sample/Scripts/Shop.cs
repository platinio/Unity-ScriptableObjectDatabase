using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjectDatabase.Sample
{
    public class Shop : MonoBehaviour
    {
        [SerializeField, ScriptableItemDatabaseSelector(typeof(ShopItemsDatabase))]
        private List<ShopItem> shopItems;

        private void Start()
        {
            foreach (var shopItem in shopItems)
            {
                Debug.Log($"Item ID: {shopItem.Id}");
                Debug.Log($"Item Name: {shopItem.Name}");
                Debug.Log($"Item Cost: {shopItem.Cost}");
            }
        }
    }
}