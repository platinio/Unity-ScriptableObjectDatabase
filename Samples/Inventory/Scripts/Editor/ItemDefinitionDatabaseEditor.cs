using Platinio.ScriptableObjectDatabase;
using UnityEditor;

namespace Platinio.InventorySample
{
    [CustomEditor(typeof(ItemDefinitionDatabase))]
    public class ItemDefinitionDatabaseEditor : ScriptableDatabaseEditor<ItemDefinitionDatabaseEditorWindow, ItemDefinitionDatabase, ItemDefinition>
    {
        
    }
}