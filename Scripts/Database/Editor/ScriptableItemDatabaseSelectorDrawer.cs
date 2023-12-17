using System;
using System.Collections.Generic;
using System.Reflection;
using Platinio;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

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

            var methodIndo = database.GetType().GetMethod("GetItems");
            var result = methodIndo.Invoke(database, null);
            
          
            
            var items = result as IEnumerable<ScriptableItem>;
            if (items == null) return;
            
            List<DropdownItem<ScriptableItem>> dropDownItems = new();

            foreach (var item in items)
            {
                dropDownItems.Add(new DropdownItem<ScriptableItem>(item.Name, null, item));
            }
            
            var dropdown = ScriptableObject.CreateInstance<AdvancedDropdown>();
            dropdown.ShowAsDropDown(new Rect(GetCurrentMousePosition(), new Vector2(0.0f, 0.0f)), new Vector2(500.0f, 500.0f));
            dropdown.ShowDropdown(dropDownItems);
            

            /*
            var attr = attribute as ScriptableItemDatabaseSelector;
            var database = ScriptableDatabaseLoader.LoadDatabase(attr.DatabaseType);
            if (database == null) return;

            GenericMenu menu = new GenericMenu();

            var methodIndo = database.GetType().GetMethod("GetItemsNameAndId");
            var result = methodIndo.Invoke(database, null);
            
            var items = result as IEnumerable<ScriptableItemNameId>;
            if (items == null) return;

            //add null option
            menu.AddItem(new GUIContent("Null"), false, data =>
            {
                property.serializedObject.Update();
                property.objectReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            }, -1);
            
            foreach (var item in items)
            {
                if (item == null) continue;
                
                menu.AddItem(new GUIContent(item.Name), false, data =>
                {
                    OnDropDownSelectionChanged(property, database, (uint)data);
                }, item.Id);
            }

            menu.DropDown(rect);*/
        }
        
        private static Func<Vector2> _getCurrentMousePosition;
        
        private Vector2 GetCurrentMousePosition()
        {
            if (_getCurrentMousePosition == null)
            {
                var currentMousePositionMethod = typeof(Editor).GetMethod("GetCurrentMousePosition", BindingFlags.NonPublic | BindingFlags.Static);
                Assert.IsNotNull(currentMousePositionMethod);
                _getCurrentMousePosition = (Func<Vector2>) Delegate.CreateDelegate(typeof(Func<Vector2>), currentMousePositionMethod);
            }

            return _getCurrentMousePosition();
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