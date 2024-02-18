using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Platinio.ScriptableObjectDatabase
{
    public abstract class DatabaseEditorWindow<Database, Entry> : EditorWindow 
        where Database : ScriptableDatabase<Entry> 
        where Entry : ScriptableItem
    {
        [SerializeField] protected VisualTreeAsset visualTreeAsset;
        [SerializeField] protected VisualTreeAsset listElementTreeAsset;

        private InspectorElement inspectorElement = null;
        protected Entry selectedItem;

        public abstract string GetWindowTitle();
        
        public virtual void CreateGUI()
        {
            if (visualTreeAsset == null)
            {
                Debug.LogError($"visualTreeAsset is null for database editor of type {typeof(Database)}");
                return;
            }
            
            if (listElementTreeAsset == null)
            {
                Debug.LogError($"listElementTreeAsset is null for database editor of type {typeof(Database)}");
                return;
            }

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            VisualElement labelFromUXML = visualTreeAsset.Instantiate();
            root.Add(labelFromUXML);

            CreateDatabaseListGUI(root);
            SetupToolBar(root.Q<ToolbarMenu>());
        }

        protected virtual void SetupToolBar(ToolbarMenu toolbarMenu)
        {
            toolbarMenu.menu.AppendAction("Create New Item", CreateNewEntry);
            toolbarMenu.menu.AppendAction("Duplicate Selected Item", DuplicateEntry);
            toolbarMenu.menu.AppendAction("Remove Selected Item", RemoveEntry);
            toolbarMenu.menu.AppendAction("Move Up", (_) => MoveSelectedItem(-1));
            toolbarMenu.menu.AppendAction("Move Down", (_) => MoveSelectedItem(1));
            toolbarMenu.menu.AppendAction("Save", Save);
        }

        protected void MoveSelectedItem(int dir)
        {
            if (selectedItem == null) return;
            
            var database = ScriptableDatabaseLoader.LoadDatabase(typeof(Database)) as Database;

            int targetIndex = database.GetItemIndex(selectedItem) + dir;
            if (targetIndex < 0 || targetIndex >= database.Items.Count) return;
            
            database.SwapItems(selectedItem, database.Items[targetIndex]);
            
            CreateDatabaseListGUI(rootVisualElement);
            RebuildDatabaseList(rootVisualElement);
        }

        protected virtual void CreateNewEntry(DropdownMenuAction dropdownMenuAction)
        {
            var database = ScriptableDatabaseLoader.LoadDatabase(typeof(Database)) as Database;
            database.AddItem(CreateInstance<Entry>());
            
            CreateDatabaseListGUI(rootVisualElement);
            RebuildDatabaseList(rootVisualElement);
        }

        protected void RebuildDatabaseList(VisualElement root)
        {
            var listView = root.Q("ItemListView") as ListView;
            listView.Rebuild();
        }

        protected virtual void DuplicateEntry(DropdownMenuAction dropdownMenuAction)
        {
            if (selectedItem == null) return;
            
            var clone = Instantiate(selectedItem);
            
            var database = ScriptableDatabaseLoader.LoadDatabase(typeof(Database)) as Database;
            database.AddItem(clone);
            
            CreateDatabaseListGUI(rootVisualElement);
            RebuildDatabaseList(rootVisualElement);
        }

        protected virtual void RemoveEntry(DropdownMenuAction dropdownMenuAction)
        {
            if (selectedItem == null) return;
            
            var databaseEditor = rootVisualElement.Q<VisualElement>("ItemEditor");
            if (inspectorElement != null) databaseEditor.Remove(inspectorElement);
            
            var database = ScriptableDatabaseLoader.LoadDatabase(typeof(Database)) as Database;
            database.RemoveItem(selectedItem);
            
            DestroyImmediate(selectedItem, true);
            
            CreateDatabaseListGUI(rootVisualElement);
            RebuildDatabaseList(rootVisualElement);
        }

        protected void Save(DropdownMenuAction dropdownMenuAction)
        {
            var database = ScriptableDatabaseLoader.LoadDatabase(typeof(Database)) as Database;
            database.OnSave();
            AssetDatabase.SaveAssetIfDirty(database);
            
            CreateDatabaseListGUI(rootVisualElement);
            RebuildDatabaseList(rootVisualElement);
        }

        protected void CreateDatabaseListGUI(VisualElement root)
        {
            var database = ScriptableDatabaseLoader.LoadDatabase(typeof(Database)) as Database;
            IEnumerable<Entry> items = database.Items;
            //items = items.OrderBy(x => x.ListOrder).ToList();

            items = FilterEntries(items);
            
            var listView = root.Q("ItemListView") as ListView;
            listView.Clear();
            listView.makeItem = MakeItem;
            listView.bindItem = BindEntryItem;
            listView.itemsSource = (IList)items;
            listView.selectionType = SelectionType.Single;
            listView.selectionChanged += OnEntrySelectionChanged;
        }

        protected abstract IEnumerable<Entry> FilterEntries(IEnumerable<Entry> entries);

        protected virtual void OnEntrySelectionChanged(IEnumerable<object> selection)
        {
            var entry = selection.FirstOrDefault() as Entry;
            selectedItem = entry;

            var databaseEditor = rootVisualElement.Q<VisualElement>("ItemEditor");
            if (inspectorElement != null && databaseEditor.Contains(inspectorElement)) databaseEditor.Remove(inspectorElement);

            inspectorElement = new InspectorElement(new SerializedObject(entry));
            databaseEditor.Add(inspectorElement);
        }

        private void BindEntryItem(VisualElement element, int index)
        {
            var database = ScriptableDatabaseLoader.LoadDatabase(typeof(Database)) as Database;
            IEnumerable<Entry> items = database.Items;
            //items = items.OrderBy(x => x.ListOrder).ToList();
            
            items = FilterEntries(items);
            
            var item = items.ToArray()[index];
            
            element.Q<Label>().text = item.Name;
            element.Q("Icon").style.backgroundImage = new StyleBackground(item.Icon);
        }


        private VisualElement MakeItem() => listElementTreeAsset.CloneTree();
    }
}