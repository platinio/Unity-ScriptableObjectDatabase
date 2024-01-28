using ScriptableObjectDatabase.Sample;
using UnityEditor;

namespace ScriptableObjectDatabase
{
    [CustomEditor(typeof(ShopItemsDatabase))]
    public class ShopItemsDatabaseEditor : ScriptableDatabaseEditor<ShopItem> { }
}