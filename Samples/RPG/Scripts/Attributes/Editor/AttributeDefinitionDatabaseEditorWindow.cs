using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEditor;
using UnityEngine;

namespace ArcaneOnyx.RPGSample.Editor
{
    public class AttributeDefinitionDatabaseEditorWindow : DatabaseEditorWindow<AttributeDefinitionDatabase, AttributeDefinition>
    {
        [MenuItem("Window/Sample/Attributes Editor")]
        public static void OpenEditor()
        {
            AttributeDefinitionDatabaseEditorWindow wnd = GetWindow<AttributeDefinitionDatabaseEditorWindow>();
            wnd.titleContent = new GUIContent(wnd.GetWindowTitle());
        }
        
        public override string GetWindowTitle() => "Attribute Definition Editor";
    }
}