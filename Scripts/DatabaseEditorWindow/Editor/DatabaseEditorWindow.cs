using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptableObjectDatabase
{
    public abstract class DatabaseEditorWindow<Database, Entry> : EditorWindow 
        where Database : ScriptableDatabase<Entry> 
        where Entry : ScriptableItem
    {
        [SerializeField] protected VisualTreeAsset editorTreeAsset;
        [SerializeField] protected VisualTreeAsset visualTreeAsset = default;
        [SerializeField] protected VisualTreeAsset listElementTreeAsset;

        private InspectorElement inspectorElement = null;
        
        public virtual void CreateGUI()
        {
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
            toolbarMenu.menu.AppendAction("Save", Save);
        }
        
        private void Save(DropdownMenuAction dropdownMenuAction)
        {
            var database = ScriptableDatabaseLoader.LoadDatabase(typeof(Database)) as Database;
            AssetDatabase.SaveAssetIfDirty(database);
        }

        protected void CreateDatabaseListGUI(VisualElement root)
        {
            var database = ScriptableDatabaseLoader.LoadDatabase(typeof(Database)) as Database;
            IEnumerable<Entry> items = database.Items;

            items = FilterEntries(items);
            
            var listView = root.Q("ItemListView") as ListView;
            listView.Clear();
            listView.makeItem = MakeSkillItem;
            listView.bindItem = BindEntryItem;
            listView.itemsSource = (IList)items;
            listView.selectionType = SelectionType.Single;
            listView.selectionChanged += OnEntrySelectionChanged;
        }

        protected abstract IEnumerable<Entry> FilterEntries(IEnumerable<Entry> entries);

        private void OnEntrySelectionChanged(IEnumerable<object> selection)
        {
            var entry = selection.FirstOrDefault() as Entry;

            var databaseEditor = rootVisualElement.Q<VisualElement>("ItemEditor");
            if (inspectorElement != null) databaseEditor.Remove(inspectorElement);

            inspectorElement = new InspectorElement(new SerializedObject(entry));
            databaseEditor.Add(inspectorElement);
        }

        private void BindEntryItem(VisualElement element, int index)
        {
            var database = ScriptableDatabaseLoader.LoadDatabase(typeof(Database)) as Database;
            IEnumerable<Entry> items = database.Items;
            
            items = FilterEntries(items);
            
            var item = items.ToArray()[index];
            
            element.Q<Label>().text = item.Name;
            element.Q("Icon").style.backgroundImage = new StyleBackground(GetEntryIcon(item));
        }

        protected abstract Sprite GetEntryIcon(Entry entry);
        
        private VisualElement MakeSkillItem() => listElementTreeAsset.CloneTree();
    }
}