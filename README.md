# ScriptableObjectDatabase

Scriptable Object Database is the best way to organize and manage your scriptable objects in Unity, forget about trying to organize your scriptable objects by hand with complicated folder structures, with this tool you can create as many scriptable objects as you need contained in a single object, a nice and easy to extend editor window to edit/create your scriptable objects and a custom dropdown to select those objects when you want to reference it from your scripts.

# How to Install?

Install from the [Unity Asset Store](https://assetstore.unity.com/packages/slug/290278).

Or

Import [This](https://github.com/platinio/Unity-ScriptableObjectDatabase/releases/download/1.1/SOD_1.1.unitypackage) Unity package into your project.

# Getting Started

If you prefer here is a youtube video.

<a href="http://www.youtube.com/watch?feature=player_embedded&v=ILqRAyVZgZc
" target="_blank"><img src="http://img.youtube.com/vi/ILqRAyVZgZc/0.jpg" 
alt="Best way to manage Scriptable Objects in Unity (Free Asset!)" width="240" height="180" border="10" /></a>

Longer text version [here](https://www.arcaneonyx.com/scriptable-object-database).

# Creating a Database

``` csharp
using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEngine;

namespace ArcaneOnyx.InventorySample
{
    public class ItemDefinition : ScriptableItem
    {
        [SerializeField, TextArea] private string description;       
        [SerializeField] private int stackableAmount;

        public int StackableAmount => stackableAmount;
        public string Description => description;
    }
}
```

``` csharp
using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEngine;

namespace ArcaneOnyx.InventorySample
{
    [CreateAssetMenu(menuName = "Sample/Database/Item Database")]
    public class ItemDefinitionDatabase : ScriptableDatabase<ItemDefinition>
    {
   
    }
}
```

# Create Custom Editors

``` csharp
using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEditor;

namespace ArcaneOnyx.InventorySample
{
    [CustomEditor(typeof(ItemDefinitionDatabase))]
    public class ItemDefinitionDatabaseEditor : 
        ScriptableItemDefaultEditor<ItemDefinitionDatabaseEditorWindow, 
            ItemDefinitionDatabase, ItemDefinition>
    {
        
    }
}
```

``` csharp
using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEditor;

namespace ArcaneOnyx.InventorySample
{
    [CustomEditor(typeof(ItemDefinition))]
    public class ItemDefinitionEditor : ScriptableItemEditor<ItemDefinitionDatabaseEditorWindow,
        ItemDefinitionDatabase, ItemDefinition>
    {
        
    }
}
```

# Create Custom Editor Window

![alt text](https://github.com/platinio/Unity-ScriptableObjectDatabase/blob/main/ReadmeResources/customEditorWindow.png?raw=true)

``` csharp
using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEditor;
using UnityEngine;

namespace ArcaneOnyx.InventorySample
{
    public class ItemDefinitionDatabaseEditorWindow : 
        DatabaseEditorWindow<ItemDefinitionDatabase, ItemDefinition>
    {
        [MenuItem("Window/Sample/Items Editor")]
        public static void OpenEditor()
        {
            ItemDefinitionDatabaseEditorWindow wnd = GetWindow<ItemDefinitionDatabaseEditorWindow>();
            wnd.titleContent = new GUIContent(wnd.GetWindowTitle());
        }
        
        public override string GetWindowTitle() => "Item Definition Editor";
    }
}
```

# Database Dropdown

![alt text](https://github.com/platinio/Unity-ScriptableObjectDatabase/blob/main/ReadmeResources/customDropdown.png?raw=true)

``` csharp
using ArcaneOnyx.InventorySample;
using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEngine;

public class DropdownTest : MonoBehaviour
{
    [SerializeField]
    [ScriptableItemDatabaseSelector(typeof(ItemDefinitionDatabase))]
    private ItemDefinition item;
}
```

# Dependencies

Scriptable Object Database depends on Unity-AdvanceDropdown, so if you want to clone this, you will need to clone both reporistories.

