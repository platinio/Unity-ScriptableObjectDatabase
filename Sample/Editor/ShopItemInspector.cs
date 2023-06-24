using System;
using ScriptableObjectDatabase.Sample;
using UnityEditor;

namespace ScriptableObjectDatabase
{
    [CustomEditor(typeof(ShopItem))]
    public class ShopItemInspector : ScriptableItemInspector
    {
        public override Type DatabaseType => typeof(ShopItemsDatabase);
    }
}