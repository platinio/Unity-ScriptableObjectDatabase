using System.Collections.Generic;
using UnityEngine;

namespace Platinio.ScriptableObjectDatabase.Sample
{
    public class Shop : MonoBehaviour
    {
        [SerializeField, ScriptableItemDatabaseSelector(typeof(ShopItemsDatabase))]
        private List<ShopItem> shopItems;

        [SerializeField] private ItemUI itemUIPrefab;
        [SerializeField] private Transform itemsParent;
        private void Start()
        {
            foreach (var item in shopItems)
            {
                var itemUI = Instantiate(itemUIPrefab, itemsParent);
                itemUI.UpdateContent(item);
            }
        }
    }
}