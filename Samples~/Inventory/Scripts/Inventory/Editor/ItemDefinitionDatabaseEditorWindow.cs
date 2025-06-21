using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEditor;
using UnityEngine;

namespace ArcaneOnyx.RPGSample
{
    public class ItemDefinitionDatabaseEditorWindow : DatabaseEditorWindow<ItemDefinitionDatabase, ItemDefinition>
    {
        [MenuItem("Window/Sample/Items Editor")]
        public static void OpenEditor()
        {
            ItemDefinitionDatabaseEditorWindow wnd = GetWindow<ItemDefinitionDatabaseEditorWindow>();
            wnd.titleContent = new GUIContent(wnd.GetWindowTitle());
        }
        
        public override string GetWindowTitle() => "Item Definition Editor";
    }
}