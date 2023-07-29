using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjectDatabase
{
    /// <summary>
    /// Dropdown drawer to select a item form a database
    /// </summary>
    [CustomPropertyDrawer(typeof(ScriptableItemDatabaseSelector))]
    public class ScriptableItemDatabaseSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI (Rect position,SerializedProperty property,GUIContent label) 
        {
            EditorGUI.BeginProperty(position,label,property);
            
            Rect labelRect = position;
            labelRect.width = position.width / 2.0f;
            
            Rect dropdownRect = position;
            dropdownRect.x += position.width / 2.0f;
            dropdownRect.width = position.width / 2.0f;
            
            EditorGUI.LabelField(labelRect, label);
            DrawDatabaseDropDown(dropdownRect, property);

            EditorGUI.EndProperty();
        }

        private void DrawDatabaseDropDown(Rect rect, SerializedProperty property)
        {
            string selectedItemName = GetSelectedItemName(property);

            if (!EditorGUI.DropdownButton(rect, new GUIContent(selectedItemName), FocusType.Passive))
            {
                return;
            }

            var attr = attribute as ScriptableItemDatabaseSelector;
            var database = ScriptableDatabaseLoader.LoadDatabase(attr.DatabaseType);
            if (database == null) return;

            GenericMenu menu = new GenericMenu();

            var methodIndo = database.GetType().GetMethod("GetItemsNameAndId");
            var result = methodIndo.Invoke(database, null);
            
            var items = result as IEnumerable<ScriptableItemNameId>;
            if (items == null) return;

            foreach (var item in items)
            {
                if (item == null) continue;
                
                menu.AddItem(new GUIContent(item.Name), false, data =>
                {
                    OnDropDownSelectionChanged(property, database, (uint)data);
                }, item.Id);
            }

            menu.DropDown(rect);
        }

        private string GetSelectedItemName(SerializedProperty property)
        {
            if (property.objectReferenceValue is ScriptableItem item) return item.name;
            return "null";
        }

        private void OnDropDownSelectionChanged(SerializedProperty property, dynamic scriptableDatabase, uint id)
        {
            property.serializedObject.Update();
            property.objectReferenceValue = scriptableDatabase.GetItem(id);
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}