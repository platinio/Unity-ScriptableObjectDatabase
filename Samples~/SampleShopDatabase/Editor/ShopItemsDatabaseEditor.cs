using UnityEditor;

namespace Platinio.ScriptableObjectDatabase.Sample
{
    [CustomEditor(typeof(ShopItemsDatabase))]
    public class ShopItemsDatabaseEditor : ScriptableDatabaseEditor<ShopItemEditorWindow, ShopItemsDatabase, ShopItem> { }
}