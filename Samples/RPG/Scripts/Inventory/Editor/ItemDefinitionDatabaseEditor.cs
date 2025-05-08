using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEditor;

namespace ArcaneOnyx.RPGSample
{
    [CustomEditor(typeof(ItemDefinitionDatabase))]
    public class ItemDefinitionDatabaseEditor : ScriptableDatabaseEditor<ItemDefinitionDatabaseEditorWindow, ItemDefinitionDatabase, ItemDefinition>
    {
        
    }
}