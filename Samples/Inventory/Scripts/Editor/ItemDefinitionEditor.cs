using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEditor;

namespace ArcaneOnyx.InventorySample
{
    [CustomEditor(typeof(ItemDefinition))]
    public class ItemDefinitionEditor : ScriptableItemDefaultEditor<ItemDefinitionDatabaseEditorWindow, ItemDefinitionDatabase, ItemDefinition>
    {
        
    }
}