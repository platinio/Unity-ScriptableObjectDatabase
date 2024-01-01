using System;
using ScriptableObjectDatabase.Sample;
using UnityEditor;

namespace ScriptableObjectDatabase
{
    [CustomEditor(typeof(ShopItem))]
    public class ShopItemEditor : ScriptableItemEditor
    {
        public override Type DatabaseType => typeof(ShopItemsDatabase);
    }
}