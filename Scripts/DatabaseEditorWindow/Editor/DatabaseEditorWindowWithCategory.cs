using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptableObjectDatabase
{
    public abstract class DatabaseEditorWindowWithCategory<Database, Entry> : DatabaseEditorWindow<Database, Entry>
        where Database : ScriptableDatabase<Entry> 
        where Entry : ScriptableItem
    {
        protected static Category selectedCategory = null;
       
        public override void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            VisualElement labelFromUXML = visualTreeAsset.Instantiate();
            root.Add(labelFromUXML);

            CreateCategoryListGUI(root);
            CreateDatabaseListGUI(root);
            SetupToolBar(root.Q<ToolbarMenu>());
        }

        private void CreateCategoryListGUI(VisualElement root)
        {
            var listView = root.Q("CategoryListView") as ListView;
            listView.Clear();
            listView.makeItem = MakeCategoryTypeItem;
            listView.bindItem = BindCategoryTypeItem;
            listView.itemsSource = GetDatabaseCategories();
            listView.selectionType = SelectionType.Single;
            listView.selectionChanged += OnCategorySelectionChanged;
        }

        private VisualElement MakeCategoryTypeItem() => listElementTreeAsset.CloneTree();
        
        private void BindCategoryTypeItem(VisualElement element, int index)
        {
            var item = GetDatabaseCategories()[index];
        
            element.Q<Label>().text = item.Name;
            element.Q("Icon").style.backgroundImage = new StyleBackground(item.Icon);
        }
        
        private void OnCategorySelectionChanged(IEnumerable<object> selection)
        {
            selectedCategory = selection.FirstOrDefault() as Category;
            UpdateSelectedCategory();
        }

        protected override void DuplicateEntry(DropdownMenuAction dropdownMenuAction)
        {
            base.DuplicateEntry(dropdownMenuAction);
            if (selectedItem != null) UpdateSelectedCategory();
        }

        protected override void RemoveEntry(DropdownMenuAction dropdownMenuAction)
        {
            if (selectedItem == null) return; 
            
            base.RemoveEntry(dropdownMenuAction);
            UpdateSelectedCategory();
        }

        protected void UpdateSelectedCategory()
        {
            var skillDatabase = ScriptableDatabaseLoader.LoadDatabase(typeof(Database)) as Database;
            IEnumerable<Entry> items = skillDatabase.Items;
        
            var listView = rootVisualElement.Q("ItemListView") as ListView;
            items = FilterEntries(items);

            listView.itemsSource = (IList)items;
            listView.RefreshItems();
        }

        protected abstract List<Category> GetDatabaseCategories();
    }

    public class Category
    {
        public string ID;
        public string Name;
        public Sprite Icon;

        public Category(string id, string name, Sprite icon)
        {
            ID = id;
            Name = name;
            Icon = icon;
        }
    }

}

