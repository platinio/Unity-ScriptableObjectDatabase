using System;
using System.Collections.Generic;
using ArcaneOnyx.AdvancedDropdown;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ArcaneOnyx.ScriptableObjectDatabase
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
            
            Rect buttonDropdownRect = position;
            buttonDropdownRect.width = 25;
            buttonDropdownRect.x += (position.width / 2.0f) - (buttonDropdownRect.width) - 5.0f;

            Rect iconRect = buttonDropdownRect;
            iconRect.width = 30;
            iconRect.x -= buttonDropdownRect.width + 10;

            buttonDropdownRect.height /= 1.5f;
            
            EditorGUI.LabelField(labelRect, label);
            DrawDatabaseDropDown(dropdownRect, property);
            
            dynamic item = property.objectReferenceValue;

            if (item != null && item.Icon != null)
            {
                Sprite sprite = item.Icon;
                GUI.DrawTexture(iconRect, sprite.texture);
            }

            if (property.objectReferenceValue != null && GUI.Button(buttonDropdownRect, "►"))
            {
                
                Editor editorInstance = Editor.CreateEditor(property.objectReferenceValue);

                try
                {
                    ((dynamic)editorInstance).OpenInEditorWindow();
                }
                catch (Exception e)
                {
                    EditorGUIUtility.PingObject(property.objectReferenceValue);
                }

                Object.DestroyImmediate(editorInstance);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 1.5f;
        }

        private void DrawDatabaseDropDown(Rect rect, SerializedProperty property)
        {
            if (!EditorGUI.DropdownButton(rect, new GUIContent(GetSelectedItemName(property)), FocusType.Passive)) return;

            var attr = attribute as ScriptableItemDatabaseSelector;
            if (attr == null) return;

            var dropDownItems = GetDropdownItems(property);

            AdvancedDropdownEditorWindow.ShowDropdown(dropDownItems, delegate(ScriptableItem item)
            {
                UpdateDropdownValue(property, item);
            });
        }

        private List<DropdownItem<ScriptableItem>> GetDropdownItems(SerializedProperty property)
        {
            var items = GetScriptableItems();
            
            var attr = attribute as ScriptableItemDatabaseSelector;
            List<DropdownItem<ScriptableItem>> dropDownItems = new();

            dropDownItems.Add(new DropdownItem<ScriptableItem>("Null", property.objectReferenceValue == null, null));
            
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