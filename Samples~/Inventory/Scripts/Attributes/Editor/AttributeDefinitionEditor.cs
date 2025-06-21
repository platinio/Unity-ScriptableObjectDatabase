using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEditor;

namespace ArcaneOnyx.RPGSample.Editor
{
    [CustomEditor(typeof(AttributeDefinition))]
    public class AttributeDefinitionEditor : ScriptableItemDefaultEditor<AttributeDefinitionDatabaseEditorWindow, AttributeDefinitionDatabase, AttributeDefinition>
    {
        
    }
}