using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Platinio.ScriptableObjectDatabase.Sample
{
    public class ShopItemEditorWindow : DatabaseEditorWindow<ShopItemsDatabase, ShopItem>
    {
        [MenuItem("Window/Shop Item Editor")]
        public static void OpenShopItemEditorWindow()
        {
            ShopItemEditorWindow wnd = GetWindow<ShopItemEditorWindow>();
            wnd.titleContent = new GUIContent(wnd.GetWindowTitle());
        }

        public override string GetWindowTitle() => "Shop Item Editor";

        protected override IEnumerable<ShopItem> FilterEntries(IEnumerable<ShopItem> entries) => entries;
    }
}

