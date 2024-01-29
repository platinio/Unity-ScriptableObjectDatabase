using UnityEngine;

namespace Platinio.ScriptableObjectDatabase.Sample
{
    public class ShopDatabaseExample : MonoBehaviour
    { 
        [SerializeField] private ShopItemsDatabase shopItemsDB;
        [SerializeField] private uint id;

        private void Start()
        {
            var item = shopItemsDB.GetItem(id);
            Debug.Log($"Item Name: {item.name}");
        }
    }
}