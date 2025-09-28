using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ArcaneOnyx.ScriptableObjectDatabase
{
    public abstract class DatabaseEditorWindow<Database, Entry> : EditorWindow 
        where Database : ScriptableDatabase<Entry> 
        where Entry : ScriptableItem
    {
        [SerializeField] protected VisualTreeAsset visualTreeAsset;
        [SerializeField] protected VisualTreeAsset listElementTreeAsset;

        protected InspectorElement inspectorElement = null;
        private Database selectedDatabase;
        protected Entry selectedItem;
        protected List<Database> activeDatabases;
        private Dictionary<Type, Database> databaseCache = new();

        public abstract string GetWindowTitle();

        public Database SelectedDatabase => selectedDatabase;
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

            activeDatabases = ScriptableDatabaseUtil.GetDatabases<Database>();
            
            SetSelectedDatabase();
            CreateDatabaseListGUI(root);
            SetupToolBar(root.Q<ToolbarMenu>());
            SetupDatabaseDropdown(root.Q<DropdownField>());
            
            //select first item on open
            if (selectedItem == null) ChangeSelection(0);
        }

        protected void SetSelectedDatabase()
        {
            selectedDatabase = ScriptableDatabaseUtil.GetActiveDatabase<Entry, Database>();
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
            AddCreateItemOptions(toolbarMenu);
            foreach (var database in activeDatabases)
            {
                toolbarMenu.menu.AppendAction($"Copy To/{database.name}", (_) => CopyTo(database, selectedItem));
            }
            toolbarMenu.menu.AppendAction("Duplicate Selected Item", DuplicateEntry);
            toolbarMenu.menu.AppendAction("Remove Selected Item", RemoveEntry);
            toolbarMenu.menu.AppendAction("Move Up", (_) => MoveSelectedItem(-1));
            toolbarMenu.menu.AppendAction("Move Down", (_) => MoveSelectedItem(1));
            toolbarMenu.menu.AppendAction("Migrate Ids", (_) => MigrateIds());
            toolbarMenu.menu.AppendAction("Save", Save);
        }

        private void CopyTo(Database database, Entry entry)
        {
            var clone = Instantiate(entry);
            database.AddItem(clone);

            clone.name = entry.name;
            clone.Name = entry.name;
        }

        protected void MigrateIds()
        {
            selectedDatabase.MigrateIds();
        }

        protected void SetupDatabaseDropdown(DropdownField dropdownField)
        {
            dropdownField.choices = activeDatabases.Select(x => x.name).ToList();
            dropdownField.index = activeDatabases.IndexOf(selectedDatabase);
            
            dropdownField.RegisterValueChangedCallback((evt) =>
            {
                selectedDatabase = activeDatabases[dropdownField.index];
                selectedItem = null;
                
                ScriptableDatabaseUtil.UpdateActiveDatabase<Entry, Database>(selectedDatabase);
                
                CreateDatabaseListGUI(rootVisualElement);
                RebuildDatabaseList(rootVisualElement);
                ClearInspector();
                ChangeSelection(0);
            });
        }

        protected void AddCreateItemOptions(ToolbarMenu toolbarMenu)
        {
            var types = GetEnumerableOfType(typeof(Entry));

            foreach (var type in types)
            {
                toolbarMenu.menu.AppendAction($"Create/{type.Name}", dropdownMenuAction =>
                {
                    CreateNewEntry(type);

                    if (selectedItem == null || !GetFilteredEntries().Contains(selectedItem))
                    {
                        ChangeSelection(0);
                    }
                });
            }
        }
        
        public IEnumerable<Type> GetEnumerableOfType(Type t)
        {
            List<Type> types = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && t.IsAssignableFrom(myType)))
                {
                    types.Add(type);
                }
            }

            return types;
        }

        protected virtual void MoveSelectedItem(int dir)
        {
            if (selectedItem == null) return;
            
            var items = FilterEntries(selectedDatabase.Items);

            int targetIndex = GetSelectedItemIndex() + dir;
            if (targetIndex < 0 || targetIndex >= selectedDatabase.Items.Count) return;
            
            selectedDatabase.SwapItems(selectedItem, items[targetIndex]);
            
            CreateDatabaseListGUI(rootVisualElement);
            RebuildDatabaseList(rootVisualElement);
        }

        private int GetSelectedItemIndex()
        {
            if (selectedItem == null) return -1;
          
            var items = FilterEntries(selectedDatabase.Items);

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == selectedItem) return i;
            }

            return -1;
        }

        protected Entry CreateNewEntry(Type type)
        {
            var newEntry = CreateInstance(type) as Entry;
            selectedDatabase.AddItem(newEntry);
            
            CreateDatabaseListGUI(rootVisualElement);
            RebuildDatabaseList(rootVisualElement);

            return newEntry;
        }

        public void UpdateDatabaseList() => RebuildDatabaseList(rootVisualElement);
        
        protected void RebuildDatabaseList(VisualElement root)
        {
            var listView = root.Q("ItemListView") as ListView;
            listView.Rebuild();
        }

        protected virtual void DuplicateEntry(DropdownMenuAction dropdownMenuAction)
        {
            if (selectedItem == null) return;
            
            var clone = Instantiate(selectedItem);
            selectedDatabase.AddItem(clone);
            
            CreateDatabaseListGUI(rootVisualElement);
            RebuildDatabaseList(rootVisualElement);
        }

        protected virtual void RemoveEntry(DropdownMenuAction dropdownMenuAction)
        {
            if (selectedItem == null) return;

            if (EditorUtility.DisplayDialog("Delete selected item?", $"\"{selectedItem.name}\" will be deleted\n \n \nYou can't undo this action", 
                    "Delete Forever", "Cancel"))
            {
                var selectedIndex = GetFilteredEntries().ToList().IndexOf(selectedItem);
                
                var databaseEditor = rootVisualElement.Q<VisualElement>("ItemEditor");
                if (inspectorElement != null) databaseEditor.Remove(inspectorElement);
              
                selectedDatabase.RemoveItem(selectedItem);
                DestroyImmediate(selectedItem, true);
                EditorUtility.SetDirty(selectedDatabase);
                
                SaveInternal();
            
                CreateDatabaseListGUI(rootVisualElement);
                RebuildDatabaseList(rootVisualElement);
                
                IReadOnlyList<Entry> items = GetFilteredEntries();
                if (selectedIndex >= items.Count) selectedIndex = items.Count - 1;
                ChangeSelection(selectedIndex);
                UpdateInspector();
            }
        }

        protected void Save(DropdownMenuAction dropdownMenuAction) => SaveInternal();

        private void SaveInternal()
        {
            ScriptableDatabaseUtil.SaveDatabase<Entry, Database>();
            
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

        public void ChangeDatabaseSelection(Database database)
        {
            selectedDatabase = database;
            selectedItem = null;
                
            ScriptableDatabaseUtil.UpdateActiveDatabase<Entry, Database>(selectedDatabase);
            
            CreateDatabaseListGUI(rootVisualElement);
            RebuildDatabaseList(rootVisualElement);
            ClearInspector();
            ChangeSelection(0);
        }

        public virtual void ChangeSelection(Entry item)
        {
            if (item == null) return;
            
            selectedItem = item;

            var databaseEditor = rootVisualElement.Q<VisualElement>("ItemEditor");
            if (inspectorElement != null && databaseEditor.Contains(inspectorElement))
            {
                databaseEditor.Remove(inspectorElement);
            }

            inspectorElement = new InspectorElement(new SerializedObject(item));
            databaseEditor.Add(inspectorElement);
           
            var listView = rootVisualElement.Q("ItemListView") as ListView;
            listView.selectedIndex = GetSelectedItemIndex();
        }
        
        private void ClearInspector()
        {
            var databaseEditor = rootVisualElement.Q<VisualElement>("ItemEditor");
            if (inspectorElement != null && databaseEditor.Contains(inspectorElement))
            {
                databaseEditor.Remove(inspectorElement);
            }
        }

        public void UpdateInspector()
        {
            ChangeSelection(selectedItem);
        }

        protected virtual void ChangeSelection(int index)
        {
            IReadOnlyList<Entry> items = GetFilteredEntries();
            if (items.Count == 0) return;
            
            if (index >= items.Count) return;
            
            ChangeSelection(items[index]);
        }

        protected IReadOnlyList<Entry> GetFilteredEntries()
        {
            return FilterEntries(selectedDatabase.Items);
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
        
    }
}