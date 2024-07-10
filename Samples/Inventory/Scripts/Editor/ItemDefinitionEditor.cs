using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEditor;

namespace ArcaneOnyx.InventorySample
{
    [CustomEditor(typeof(ItemDefinition))]
    public class ItemDefinitionEditor : ScriptableItemEditor<ItemDefinitionDatabaseEditorWindow, ItemDefinitionDatabase, ItemDefinition>
    {
        
    }
}