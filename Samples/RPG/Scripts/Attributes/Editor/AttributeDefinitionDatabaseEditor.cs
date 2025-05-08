using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEditor;

namespace ArcaneOnyx.RPGSample.Editor
{
    [CustomEditor(typeof(AttributeDefinitionDatabase))]
    public class AttribureDefinitionDatabaseEditor : ScriptableDatabaseEditor<AttributeDefinitionDatabaseEditorWindow, AttributeDefinitionDatabase, AttributeDefinition>
    {
        
    }
}