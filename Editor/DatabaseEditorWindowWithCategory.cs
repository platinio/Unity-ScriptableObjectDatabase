using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArcaneOnyx.ScriptableObjectDatabase
{
    public abstract class DatabaseEditorWindowWithCategory<Database, Entry> : DatabaseEditorWindow<Database, Entry>
        where Database : ScriptableDatabase<Entry> 
        where Entry : ScriptableItem
    {
        protected static Category selectedCategory = null;

        public override void CreateGUI()
        {
            var visualTreeAssetElement = GetVisualTreeAssetOrDefault();
            var listElementTreeAssetElement = GetListElementTreeAssetOrDefault();
            
            if (visualTreeAssetElement == null)
            {
                Debug.LogError($"visualTreeAsset is null for database editor of type {typeof(Database)}");
                return;
            }
            
            if (listElementTreeAssetElement == null)
            {
                Debug.LogError($"listElementTreeAsset is null for database editor of type {typeof(Database)}");
                return;
            }
            
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            VisualElement labelFromUXML = visualTreeAssetElement.Instantiate();
            root.Add(labelFromUXML);

            activeDatabases = ScriptableDatabaseUtil.GetDatabases<Database>();
            
            SetSelectedDatabase();
            CreateCategoryListGUI(root);
            CreateDatabaseListGUI(root);
            SetupToolBar(root.Q<ToolbarMenu>());
            SetupDatabaseDropdown(root.Q<DropdownField>());
            
            //select first item on open
            if (selectedItem == null) ChangeSelection(0);
        }

        private void CreateCategoryListGUI(VisualElement root)
        {
            var listView = root.Q("CategoryListView") as ListView;
            listView.Clear();
            listView.makeItem = MakeItem;
            listView.bindItem = BindCategoryTypeItem;
            listView.itemsSource = GetDatabaseCategories();
            listView.selectionType = SelectionType.Single;
            listView.selectionChanged += OnCategorySelectionChanged;
        }
        
        private void BindCategoryTypeItem(VisualElement element, int index)
        {
            var item = GetDatabaseCategories()[index];
        
            element.Q<Label>().text = item.Name;
            element.Q("Icon").style.backgroundImage = new StyleBackground(item.Icon);
        }
        
        private void OnCategorySelectionChanged(IEnumerable<object> selection)
        {
            var databaseEditor = rootVisualElement.Q<VisualElement>("ItemEditor");
            if (inspectorElement != null && databaseEditor.Contains(inspectorElement)) databaseEditor.Remove(inspectorElement);
            
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
            var listView = rootVisualElement.Q("ItemListView") as ListView;
            var items = FilterEntries(SelectedDatabase.Items);

            listView.itemsSource = (IList)items;
            listView.RefreshItems();
        }

        protected abstract Category GetItemCategory(Entry item);

        protected int GetCategoryIndex(Category category)
        {
            var categories = GetDatabaseCategories();
            
            for (int i = 0; i < categories.Count; i++)
            {
                if (categories[i].ID == category.ID) return i;
            }

            return 0;
        }

        public override void ChangeSelection(Entry item)
        {
            selectedCategory = GetItemCategory(item);
            
            var listView = rootVisualElement.Q("CategoryListView") as ListView;
            listView.selectedIndex = GetCategoryIndex(selectedCategory);
            
            base.ChangeSelection(item);
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

