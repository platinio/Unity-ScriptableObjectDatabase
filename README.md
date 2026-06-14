# ScriptableObjectDatabase

A data management system for organizing, editing, and querying collections of ScriptableObject assets through a unified editor window and runtime API.

# Overview

Managing large numbers of ScriptableObjects across a project gets messy fast — assets scattered in folders, manual inspector assignments, no central place to browse or search. The Scriptable Object Database module gives you a structured alternative.

You define an item type and a database type, create a database asset, and fill it through a dedicated editor window. At runtime you query items by name or ID. In the inspector, a dropdown attribute replaces manual asset references with a searchable selector tied to the database.

# How to Install?

Install from the [Unity Asset Store](https://assetstore.unity.com/packages/slug/290278).

Install using a Unity Package Import [This](https://github.com/platinio/Unity-ScriptableObjectDatabase/releases/download/2.1/Unity-ScriptableObjectDatabase.2.1.unitypackage) into your project.

### Install using the package manager

[How to install using the package manager](https://docs.unity3d.com/2020.1/Documentation/Manual/upm-ui-giturl.html)

Install advanced dropdown (dependency) 
```
https://github.com/platinio/Unity-AdvancedDropdown.git
```

Install Scriptable Object Database
```
https://github.com/platinio/Unity-ScriptableObjectDatabase.git#2.1
```

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

Scriptable Object Database depends on [Unity-AdvanceDropdown](https://github.com/platinio/Unity-AdvancedDropdown), so if you want to clone this, you will need to clone both repositories.
