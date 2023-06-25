using UnityEngine;

namespace ScriptableObjectDatabase.Sample
{
    public class ShopDatabaseExample : MonoBehaviour
    {
        [SerializeField] private ShopItemsDatabase m_shopItemdsDB;
        [SerializeField] private uint m_id;

        private void Start()
        {
            var item = m_shopItemdsDB.GetItem(m_id);
            Debug.Log($"Item Name: {item.name}");
        }
    }
}