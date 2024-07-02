using Platinio.ScriptableObjectDatabase;

namespace Platinio.InventorySample
{
    public class ItemDefinitionDatabaseEditorWindow : DatabaseEditorWindow<ItemDefinitionDatabase, ItemDefinition>
    {
        public override string GetWindowTitle() => "Item Definition Editor";
    }
}