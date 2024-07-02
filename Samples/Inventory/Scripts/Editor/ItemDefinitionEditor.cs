using Platinio.ScriptableObjectDatabase;
using UnityEditor;

namespace Platinio.InventorySample
{
    [CustomEditor(typeof(ItemDefinition))]
    public class ItemDefinitionEditor : ScriptableItemEditor<ItemDefinitionDatabaseEditorWindow, ItemDefinitionDatabase, ItemDefinition>
    {
        
    }
}