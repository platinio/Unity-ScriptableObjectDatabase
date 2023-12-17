using System.Collections.Generic;
using System.Linq;
using Platinio;
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
            if (!EditorGUI.DropdownButton(rect, new GUIContent(GetSelectedItemName(property)), FocusType.Passive)) return;

            var attr = attribute as ScriptableItemDatabaseSelector;
            if (attr == null) return;

            var dropDownItems = GetDropdownItems(property);

            AdvancedDropdown.ShowDropdown(dropDownItems, delegate(ScriptableItem item)
            {
                UpdateDropdownValue(property, item);
            });
        }

        private List<DropdownItem<ScriptableItem>> GetDropdownItems(SerializedProperty property)
        {
            var items = GetScriptableItems();
            
            var attr = attribute as ScriptableItemDatabaseSelector;
            List<DropdownItem<ScriptableItem>> dropDownItems = new();

            if (attr.CanBeNull)
            {
                dropDownItems.Add(new DropdownItem<ScriptableItem>("Null", null, property.objectReferenceValue == null, null));
            }
            else if (property.objectReferenceValue == null) 
            {
                UpdateDropdownValue(property, items.FirstOrDefault());
            }
            
            foreach (var item in items)
            {
                bool isSelected = item == property.objectReferenceValue;
                dropDownItems.Add(new DropdownItem<ScriptableItem>(item.Name, item.Icon,  isSelected, item));
            }
            
            return dropDownItems;
        }

        private IEnumerable<ScriptableItem> GetScriptableItems()
        {
            var attr = attribute as ScriptableItemDatabaseSelector;
            var database = ScriptableDatabaseLoader.LoadDatabase(attr.DatabaseType);
            if (database == null) return null;

            var methodIndo = database.GetType().GetMethod("GetItems");
            var result = methodIndo.Invoke(database, null);
            return result as IEnumerable<ScriptableItem>;
        }

        private string GetSelectedItemName(SerializedProperty property)
        {
            if (property.objectReferenceValue is ScriptableItem item) return item.name;
            return "null";
        }

        private void UpdateDropdownValue(SerializedProperty property, ScriptableItem item)
        {
            property.serializedObject.Update();
            property.objectReferenceValue = item;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}