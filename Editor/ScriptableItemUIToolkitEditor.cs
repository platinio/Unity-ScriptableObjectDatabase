using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ArcaneOnyx.ScriptableObjectDatabase
{
    public class ScriptableItemUIToolkitEditor<ItemEditorWindow, Database, Entry> : ScriptableItemInspector<ItemEditorWindow, Database, Entry> 
        where ItemEditorWindow : DatabaseEditorWindow<Database, Entry> 
        where Database : ScriptableDatabase<Entry> 
        where Entry : ScriptableItem
    {
        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();

            var iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    var propertyField = new PropertyField(iterator.Copy()) { name = "PropertyField:" + iterator.propertyPath };

                    if (iterator.propertyPath == "m_Script" && serializedObject.targetObject != null)
                        propertyField.SetEnabled(value: false);

                    container.Add(propertyField);
                }
                while (iterator.NextVisible(false));
            }

            return container;
        }
    }
}