# Scriptable Object Database

A data management system for organizing, editing, and querying collections of ScriptableObject assets through a unified editor window and runtime API.

## Overview

Managing large numbers of ScriptableObjects across a project gets messy fast — assets scattered in folders, manual inspector assignments, no central place to browse or search. The Scriptable Object Database module gives you a structured alternative.

You define an item type and a database type, create a database asset, and fill it through a dedicated editor window. At runtime you query items by name or ID. In the inspector, a dropdown attribute replaces manual asset references with a searchable selector tied to the database.

It is the foundation several other Arcane Onyx modules are built on, including Skills and Game Sounds.

## How to Install

Install from the [Unity Asset Store](https://assetstore.unity.com/packages/slug/290278).

**Unity package (recommended)** — download and import [the latest `.unitypackage`](https://github.com/platinio/Unity-ScriptableObjectDatabase/releases/latest/download/ScriptableObjectDatabase.unitypackage). It is self-contained: the Advanced Dropdown dependency is bundled in, so there is nothing else to install.

**Package Manager (git URL)** — see [how to install from a git URL](https://docs.unity3d.com/2020.1/Documentation/Manual/upm-ui-giturl.html). Add both, dependency first:

```
https://github.com/platinio/Unity-AdvancedDropdown.git
https://github.com/platinio/Unity-ScriptableObjectDatabase.git#2.2.0
```

## Getting Started

Prefer video? [Watch the walkthrough](https://www.youtube.com/watch?v=ILqRAyVZgZc). A longer written version lives on [arcaneonyx.com](https://www.arcaneonyx.com/scriptable-object-database).

**1. Define your item type**

Create a class that inherits from `ScriptableItem`:

```csharp
using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEngine;

public class WeaponDefinition : ScriptableItem
{
    [SerializeField] private int damage;
    [SerializeField] private float attackSpeed;

    public int Damage => damage;
    public float AttackSpeed => attackSpeed;
}
```

**2. Define your database type**

Create a class that inherits from `ScriptableDatabase<T>`:

```csharp
using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEngine;

[CreateAssetMenu(menuName = "MyGame/Weapon Database")]
public class WeaponDatabase : ScriptableDatabase<WeaponDefinition> { }
```

**3. Create custom editors**

```csharp
using ArcaneOnyx.ScriptableObjectDatabase.Editor;
using UnityEditor;

public class WeaponEditorWindow : DatabaseEditorWindow<WeaponDatabase, WeaponDefinition>
{
    [MenuItem("Window/MyGame/Weapon Editor")]
    public static void Open() => GetWindow<WeaponEditorWindow>("Weapon Editor");
}
```

```csharp
[CustomEditor(typeof(WeaponDatabase))]
public class WeaponDatabaseEditor : ScriptableDatabaseEditor<WeaponEditorWindow, WeaponDatabase, WeaponDefinition> { }

[CustomEditor(typeof(WeaponDefinition))]
public class WeaponDefinitionEditor : ScriptableItemDefaultEditor<WeaponEditorWindow, WeaponDatabase, WeaponDefinition> { }
```

**4. Create a database asset**

Right-click in the Project window → **Create → MyGame → Weapon Database**.

**5. Open the editor window and add items**

Select the database asset and click **Open Editor Window** in the inspector. From the toolbar use **Create → WeaponDefinition** to add new items and fill in their properties.

![Editor window](https://github.com/platinio/Unity-ScriptableObjectDatabase/blob/main/ReadmeResources/customEditorWindow.png?raw=true)

> [!NOTE]
> Items are stored as sub-assets inside the database `.asset` file. You do not need to manage them as separate files.

## Referencing Items in the Inspector

Use the `[ScriptableItemDatabaseSelector]` attribute to replace a manual asset reference with a searchable dropdown tied to a specific database:

```csharp
using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEngine;

public class PlayerLoadout : MonoBehaviour
{
    [SerializeField]
    [ScriptableItemDatabaseSelector(typeof(WeaponDatabase))]
    private WeaponDefinition startingWeapon;
}
```

The dropdown shows all items in the database with their icons. The arrow button (►) next to the field opens the selected item directly in the editor window.

![Database dropdown](https://github.com/platinio/Unity-ScriptableObjectDatabase/blob/main/ReadmeResources/customDropdown.png?raw=true)

## Querying at Runtime

```csharp
// Fetch by name
WeaponDefinition sword = weaponDatabase.GetItemByName("Sword");

// Fetch by unique ID
WeaponDefinition item = weaponDatabase.GetItem(savedItemId);

// Iterate all items
foreach (var weapon in weaponDatabase.Items)
{
    Debug.Log(weapon.Name);
}
```

## Editor Window Toolbar Actions

| Action                      | Description                                          |
| --------------------------- | ---------------------------------------------------- |
| **Create**                  | Add a new item of any registered subtype             |
| **Duplicate Selected Item** | Clone the currently selected item                    |
| **Remove Selected Item**    | Delete the selected item                             |
| **Copy To**                 | Clone items into a different database                |
| **Move Up / Move Down**     | Reorder items in the list                            |
| **Migrate IDs**             | Upgrade items from legacy numeric IDs to GUID format |
| **Save**                    | Persist all changes to disk                          |

## Dependencies

Scriptable Object Database depends on [Unity-AdvancedDropdown](https://github.com/platinio/Unity-AdvancedDropdown). The `.unitypackage` release bundles it automatically; if you clone the repository or install via git URL, add Advanced Dropdown as well.
