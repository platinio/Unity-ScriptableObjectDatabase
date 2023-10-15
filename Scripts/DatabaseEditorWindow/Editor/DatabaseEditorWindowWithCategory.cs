using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptableObjectDatabase
{
    public abstract class DatabaseEditorWindowWithCategory<Database, Entry, CategoryDatabase, CategoryEntry> : DatabaseEditorWindow<Database, Entry>
        where Database : ScriptableDatabase<Entry> 
        where Entry : ScriptableItem
        where CategoryDatabase : ScriptableDatabase<CategoryEntry> 
        where CategoryEntry : ScriptableItem
    {
        protected static CategoryEntry SelectedCategory = null;
        
        public override void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            VisualElement labelFromUXML = visualTreeAsset.Instantiate();
            root.Add(labelFromUXML);

            CreateCategoryListGUI(root);
            CreateDatabaseListGUI(root);
        }
        
        private void CreateCategoryListGUI(VisualElement root)
        {
            var categoryDatabase = ScriptableDatabaseLoader.LoadDatabase(typeof(CategoryDatabase)) as CategoryDatabase;

            var listView = root.Q("SkillClassTypeListView") as ListView;
            listView.Clear();
            listView.makeItem = MakeCategoryTypeItem;
            listView.bindItem = BindCategoryTypeItem;
            listView.itemsSource = (IList)categoryDatabase.Items;
            listView.selectionType = SelectionType.Single;
            listView.selectionChanged += OnCategorySelectionChanged;
        }
        
        private VisualElement MakeCategoryTypeItem() => listElementTreeAsset.CloneTree();
        
        private void BindCategoryTypeItem(VisualElement element, int index)
        {
            var categoryDatabase = ScriptableDatabaseLoader.LoadDatabase(typeof(CategoryDatabase)) as CategoryDatabase;
            var item = categoryDatabase.Items.ToArray()[index];
        
            element.Q<Label>().text = item.Name;
            element.Q("Icon").style.backgroundImage = new StyleBackground(GetCategoryIcon(item));
        }
        
        private void OnCategorySelectionChanged(IEnumerable<object> selection)
        {
            SelectedCategory = selection.FirstOrDefault() as CategoryEntry;
        
            var skillDatabase = ScriptableDatabaseLoader.LoadDatabase(typeof(Database)) as Database;
            IEnumerable<Entry> items = skillDatabase.Items;
        
            var listView = rootVisualElement.Q("SkillListView") as ListView;
            items = FilterEntries(items);

            listView.itemsSource = (IList)items;
            listView.RefreshItems();
        }

        protected abstract Sprite GetCategoryIcon(CategoryEntry categoryEntry);

    }

}

