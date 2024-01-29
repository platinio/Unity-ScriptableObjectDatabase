using System;
using UnityEditor;

namespace Platinio.ScriptableObjectDatabase.Sample
{
    [CustomEditor(typeof(ShopItem))]
    public class ShopItemEditor : ScriptableItemEditor
    {
        public override Type DatabaseType => typeof(ShopItemsDatabase);
    }
}