using UnityEngine;

namespace ScriptableObjectDatabase.Sample
{
    public class ShopDatabaseExample : MonoBehaviour
    { 
        [SerializeField] private ShopItemsDatabase shopItemsDB;
        [SerializeField] private uint m_id;

        private void Start()
        {
            var item = shopItemsDB.GetItem(m_id);
            Debug.Log($"Item Name: {item.name}");
        }
    }
}