using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArcaneOnyx.ScriptableObjectDatabase
{
    public abstract class DatabaseEditorWindow<Database, Entry> : EditorWindow 
        where Database : ScriptableDatabase<Entry> 
        where Entry : ScriptableItem
    {
        [SerializeField] protected VisualTreeAsset visualTreeAsset;
        [SerializeField] protected VisualTreeAsset listElementTreeAsset;

        protected InspectorElement inspectorElement = null;
        protected Entry selectedItem;
        private Dictionary<Type, Database> databaseCache = new();

        public abstract string GetWindowTitle();

        public Entry SelectedItem => selectedItem;
        
        public virtual void CreateGUI()
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

            CreateDatabaseListGUI(root);
            SetupToolBar(root.Q<ToolbarMenu>());
            
            //select first item on open
            if (selectedItem == null) ChangeSelection(0);
        }
        
        protected VisualTreeAsset GetListElementTreeAssetOrDefault()
        {
            if (listElementTreeAsset == null) return LoadAssetByName("ListElementTreeAsset");
            return listElementTreeAsset;
        }

        private VisualTreeAsset LoadAssetByName(string assetName)
        {
            var assetGuid = AssetDatabase.FindAssets(assetName).FirstOrDefault();
            if (string.IsNullOrEmpty(assetGuid)) return null;

            var path = AssetDatabase.GUIDToAssetPath(assetGuid);
            return AssetDatabase.LoadAssetAtPath(path, typeof(VisualTreeAsset)) as VisualTreeAsset;
        }

        protected VisualTreeAsset GetVisualTreeAssetOrDefault()
        {
            if (visualTreeAsset == null) return LoadAssetByName("GenericItemEditorTreeAsset");
            return visualTreeAsset;
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

        protected virtual void MoveSelectedItem(int dir)
        {
            if (selectedItem == null) return;

            var database = GetDatabase();
            var items = FilterEntries(database.Items);

            int targetIndex = GetSelectedItemIndex() + dir;
            if (targetIndex < 0 || targetIndex >= database.Items.Count) return;
            
            database.SwapItems(selectedItem, items[targetIndex]);
            
            CreateDatabaseListGUI(rootVisualElement);
            RebuildDatabaseList(rootVisualElement);
        }

        private int GetSelectedItemIndex()
        {
            if (selectedItem == null) return -1;
        
            var database = GetDatabase();
            var items = FilterEntries(database.Items);

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == selectedItem) return i;
            }

            return -1;
        }

        protected virtual void CreateNewEntry(DropdownMenuAction dropdownMenuAction)
        {
            var database = GetDatabase();
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

            var database = GetDatabase();
            database.AddItem(clone);
            
            CreateDatabaseListGUI(rootVisualElement);
            RebuildDatabaseList(rootVisualElement);
        }

        protected virtual void RemoveEntry(DropdownMenuAction dropdownMenuAction)
        {
            if (selectedItem == null) return;

            if (EditorUtility.DisplayDialog("Delete selected item?", $"\"{selectedItem.name}\" will be deleted\n \n \nYou can't undo this action", 
                    "Delete Forever", "Cancel"))
            {
                var databaseEditor = rootVisualElement.Q<VisualElement>("ItemEditor");
                if (inspectorElement != null) databaseEditor.Remove(inspectorElement);

                var database = GetDatabase();
                database.RemoveItem(selectedItem);
            
                DestroyImmediate(selectedItem, true);
            
                EditorUtility.SetDirty(database);
                SaveInternal();
            
                CreateDatabaseListGUI(rootVisualElement);
                RebuildDatabaseList(rootVisualElement);
            }
        }

        protected void Save(DropdownMenuAction dropdownMenuAction) => SaveInternal();

        private void SaveInternal()
        {
            var database = GetDatabase();
            database.OnSave();
            AssetDatabase.SaveAssetIfDirty(database);
            
            CreateDatabaseListGUI(rootVisualElement);
            RebuildDatabaseList(rootVisualElement);
        }

        protected void CreateDatabaseListGUI(VisualElement root)
        {
            IReadOnlyList<Entry> items = GetFilteredEntries();

            var listView = root.Q("ItemListView") as ListView;
            listView.Clear();
            listView.makeItem = MakeItem;
            listView.bindItem = BindEntryItem;
            listView.itemsSource = (IList)items;
            listView.selectionType = SelectionType.Single;
            listView.selectionChanged += OnEntrySelectionChanged;
        }

        protected virtual IReadOnlyList<Entry> FilterEntries(IReadOnlyList<Entry> entries) => entries;

        protected virtual void OnEntrySelectionChanged(IEnumerable<object> selection)
        {
            var entry = selection.FirstOrDefault() as Entry;
           ChangeSelection(entry);
        }

        public virtual void ChangeSelection(Entry item)
        {
            if (item == null) return;
            
            selectedItem = item;

            var databaseEditor = rootVisualElement.Q<VisualElement>("ItemEditor");
            if (inspectorElement != null && databaseEditor.Contains(inspectorElement)) databaseEditor.Remove(inspectorElement);

            inspectorElement = new InspectorElement(new SerializedObject(item));
            databaseEditor.Add(inspectorElement);
           
            var listView = rootVisualElement.Q("ItemListView") as ListView;
            listView.selectedIndex = GetSelectedItemIndex();
        }

        protected virtual void ChangeSelection(int index)
        {
            IReadOnlyList<Entry> items = GetFilteredEntries();
            if (index >= items.Count) return;
            
            ChangeSelection(items[index]);
        }

        protected IReadOnlyList<Entry> GetFilteredEntries()
        {
            var database = GetDatabase();
            IReadOnlyList<Entry> items = database.Items;

            return FilterEntries(items);
        }

        private void BindEntryItem(VisualElement element, int index)
        {
            IReadOnlyList<Entry> items = GetFilteredEntries();

            var item = items.ToArray()[index];
            
            element.Q<Label>().text = item.name;
            element.Q("Icon").style.backgroundImage = new StyleBackground(item.Icon);
        }

        protected VisualElement MakeItem()
        {
            var listElementTreeAsset = GetListElementTreeAssetOrDefault();
            if (listElementTreeAsset == null)
            {
                Debug.LogError("List Element Tree Asset is null");
                return null;
            }
            
            return listElementTreeAsset.CloneTree();
        }

        private Database GetDatabase()
        {
            if (databaseCache.TryGetValue(typeof(Database), out var database))
            {
                if (database == null || !database.IsEnabled()) databaseCache.Remove(typeof(Database));
                else return database;
            }

            database = ScriptableDatabaseLoader.LoadDatabase(typeof(Database)) as Database;
            databaseCache[typeof(Database)] = database;
            return database;
        } 
    }
}