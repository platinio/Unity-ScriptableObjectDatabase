using ArcaneOnyx.ScriptableObjectDatabase;

namespace ArcaneOnyx.InventorySample
{
    public class ItemDefinitionDatabaseEditorWindow : DatabaseEditorWindow<ItemDefinitionDatabase, ItemDefinition>
    {
        public override string GetWindowTitle() => "Item Definition Editor";
    }
}