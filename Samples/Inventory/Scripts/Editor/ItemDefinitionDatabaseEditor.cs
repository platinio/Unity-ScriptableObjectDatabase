using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEditor;

namespace ArcaneOnyx.InventorySample
{
    [CustomEditor(typeof(ItemDefinitionDatabase))]
    public class ItemDefinitionDatabaseEditor : ScriptableDatabaseEditor<ItemDefinitionDatabaseEditorWindow, ItemDefinitionDatabase, ItemDefinition>
    {
        
    }
}