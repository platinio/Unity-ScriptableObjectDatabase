using UnityEngine;

namespace ScriptableObjectDatabase.Sample
{
    [CreateAssetMenu(menuName = "Database/Test Database")]
    public class ShopItemsDatabase : ScriptableDatabase<ShopItem> { }
}